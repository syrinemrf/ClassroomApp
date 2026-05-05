using ClassroomApp.Data;
using ClassroomApp.Filters;
using ClassroomApp.Models.Entities;
using ClassroomApp.Models.Enums;
using ClassroomApp.Models.ViewModels.Assignment;
using ClassroomApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ClassroomApp.Controllers
{
    public class AssignmentController : Controller
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;

        public AssignmentController(AppDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        [RoleAuthorize("Teacher")]
        public async Task<IActionResult> Index(int page = 1)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);
            if (teacher == null) return NotFound();

            var pageSize = 10;
            var query = _context.Assignments
                .Include(a => a.Classroom).ThenInclude(c => c.Students)
                .Include(a => a.Subject)
                .Include(a => a.Submissions)
                .Where(a => a.TeacherId == teacher.Id)
                .OrderByDescending(a => a.CreatedAt);

            var assignments = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AssignmentViewModel
                {
                    Id = a.Id,
                    Title = a.Title,
                    Description = a.Description,
                    Deadline = a.Deadline,
                    MaxScore = a.MaxScore,
                    CreatedAt = a.CreatedAt,
                    ClassroomName = a.Classroom.Name,
                    ClassroomId = a.ClassroomId,
                    SubjectId = a.SubjectId,
                    SubjectName = a.Subject != null ? a.Subject.Name : null,
                    TeacherId = a.TeacherId,
                    TotalStudents = a.Classroom.Students.Count,
                    SubmissionCount = a.Submissions.Count,
                    GradedCount = a.Submissions.Count(s => s.Status == SubmissionStatus.Graded)
                })
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(await query.CountAsync() / (double)pageSize);
            return View(assignments);
        }

        [RoleAuthorize("Teacher")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);
            if (teacher == null) return NotFound();

            var subjectIdParam = Request.Query["subjectId"].FirstOrDefault();
            Guid? preselectedSubjectId = Guid.TryParse(subjectIdParam, out var parsedId) ? parsedId : null;

            var teacherSubjects = await _context.Subjects
                .Include(s => s.SubjectClassrooms).ThenInclude(sc => sc.Classroom)
                .Where(s => s.TeacherId == teacher.Id)
                .OrderBy(s => s.Name)
                .ToListAsync();

            Dictionary<Guid, string> availableClassrooms;
            if (preselectedSubjectId.HasValue)
            {
                var subject = teacherSubjects.FirstOrDefault(s => s.Id == preselectedSubjectId.Value);
                availableClassrooms = subject?.SubjectClassrooms
                    .ToDictionary(sc => sc.ClassroomId, sc => sc.Classroom.Name)
                    ?? new Dictionary<Guid, string>();
            }
            else
            {
                availableClassrooms = await _context.Classrooms
                    .OrderBy(c => c.Name)
                    .ToDictionaryAsync(c => c.Id, c => c.Name);
            }

            var model = new CreateAssignmentViewModel
            {
                SubjectId = preselectedSubjectId,
                Deadline = DateTime.UtcNow.AddDays(7),
                AvailableSubjects = teacherSubjects.ToDictionary(s => s.Id, s => s.Name),
                AvailableClassrooms = availableClassrooms
            };
            return View(model);
        }

        [RoleAuthorize("Teacher")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAssignmentViewModel model)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);
            if (teacher == null) return NotFound();

            async Task ReloadDropdowns()
            {
                var subjects = await _context.Subjects
                    .Include(s => s.SubjectClassrooms).ThenInclude(sc => sc.Classroom)
                    .Where(s => s.TeacherId == teacher.Id).OrderBy(s => s.Name).ToListAsync();
                model.AvailableSubjects = subjects.ToDictionary(s => s.Id, s => s.Name);
                if (model.SubjectId.HasValue)
                {
                    var sub = subjects.FirstOrDefault(s => s.Id == model.SubjectId.Value);
                    model.AvailableClassrooms = sub?.SubjectClassrooms.ToDictionary(sc => sc.ClassroomId, sc => sc.Classroom.Name)
                        ?? new Dictionary<Guid, string>();
                }
                else
                {
                    model.AvailableClassrooms = await _context.Classrooms.OrderBy(c => c.Name).ToDictionaryAsync(c => c.Id, c => c.Name);
                }
            }

            if (!ModelState.IsValid)
            {
                await ReloadDropdowns();
                return View(model);
            }

            var assignment = new Assignment
            {
                Id = Guid.NewGuid(),
                Title = model.Title,
                Description = model.Description,
                Deadline = model.Deadline,
                MaxScore = model.MaxScore,
                TeacherId = teacher.Id,
                ClassroomId = model.ClassroomId,
                SubjectId = model.SubjectId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();

            await _notificationService.NotifyClassroomAsync(
                model.ClassroomId,
                "Nouveau devoir",
                $"Nouveau devoir '{model.Title}'. Date limite : {model.Deadline:dd MMM yyyy HH:mm}",
                NotificationType.NewAssignment,
                assignment.Id,
                "Assignment"
            );

            TempData["Success"] = "Devoir cree avec succes.";

            if (model.SubjectId.HasValue)
                return RedirectToAction("Details", "Subject", new { id = model.SubjectId.Value });

            return RedirectToAction(nameof(Index));
        }

        [RoleAuthorize("Teacher")]
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);
            if (teacher == null) return NotFound();

            var assignment = await _context.Assignments
                .Include(a => a.Classroom)
                .Include(a => a.Subject)
                .FirstOrDefaultAsync(a => a.Id == id && a.TeacherId == teacher.Id);
            if (assignment == null) return NotFound();

            var teacherSubjects = await _context.Subjects
                .Include(s => s.SubjectClassrooms).ThenInclude(sc => sc.Classroom)
                .Where(s => s.TeacherId == teacher.Id)
                .OrderBy(s => s.Name)
                .ToListAsync();

            var model = new CreateAssignmentViewModel
            {
                Title = assignment.Title,
                Description = assignment.Description,
                Deadline = assignment.Deadline,
                MaxScore = assignment.MaxScore,
                ClassroomId = assignment.ClassroomId,
                SubjectId = assignment.SubjectId,
                AvailableSubjects = teacherSubjects.ToDictionary(s => s.Id, s => s.Name),
                AvailableClassrooms = await _context.Classrooms.OrderBy(c => c.Name).ToDictionaryAsync(c => c.Id, c => c.Name)
            };
            ViewBag.AssignmentId = id;
            return View(model);
        }

        [RoleAuthorize("Teacher")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CreateAssignmentViewModel model)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);
            if (teacher == null) return NotFound();

            if (!ModelState.IsValid)
            {
                model.AvailableSubjects = await _context.Subjects.Where(s => s.TeacherId == teacher.Id).OrderBy(s => s.Name).ToDictionaryAsync(s => s.Id, s => s.Name);
                model.AvailableClassrooms = await _context.Classrooms.OrderBy(c => c.Name).ToDictionaryAsync(c => c.Id, c => c.Name);
                ViewBag.AssignmentId = id;
                return View(model);
            }

            var assignment = await _context.Assignments.FirstOrDefaultAsync(a => a.Id == id && a.TeacherId == teacher.Id);
            if (assignment == null) return NotFound();

            assignment.Title = model.Title;
            assignment.Description = model.Description;
            assignment.Deadline = model.Deadline;
            assignment.MaxScore = model.MaxScore;
            assignment.ClassroomId = model.ClassroomId;
            assignment.SubjectId = model.SubjectId;
            assignment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Devoir modifie avec succes.";

            if (model.SubjectId.HasValue)
                return RedirectToAction("Details", "Subject", new { id = model.SubjectId.Value });

            return RedirectToAction(nameof(Index));
        }

        [RoleAuthorize("Teacher")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);
            if (teacher == null) return NotFound();

            var assignment = await _context.Assignments.FirstOrDefaultAsync(a => a.Id == id && a.TeacherId == teacher.Id);
            if (assignment == null) return NotFound();

            var subjectId = assignment.SubjectId;
            _context.Assignments.Remove(assignment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Devoir supprime avec succes.";

            var returnSubjectId = Request.Form["returnSubjectId"].FirstOrDefault();
            if (Guid.TryParse(returnSubjectId, out var returnId))
                return RedirectToAction("Details", "Subject", new { id = returnId });

            return RedirectToAction(nameof(Index));
        }

        [RoleAuthorize("Teacher")]
        public async Task<IActionResult> Details(Guid id)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);
            if (teacher == null) return NotFound();

            var assignment = await _context.Assignments
                .Include(a => a.Classroom).ThenInclude(c => c.Students).ThenInclude(s => s.User)
                .Include(a => a.Submissions).ThenInclude(s => s.Student).ThenInclude(s => s.User)
                .Include(a => a.Comments).ThenInclude(c => c.User)
                .FirstOrDefaultAsync(a => a.Id == id && a.TeacherId == teacher.Id);

            if (assignment == null) return NotFound();

            var submittedStudentIds = assignment.Submissions.Select(s => s.StudentId).ToHashSet();

            var vm = new AssignmentViewModel
            {
                Id = assignment.Id,
                Title = assignment.Title,
                Description = assignment.Description,
                Deadline = assignment.Deadline,
                MaxScore = assignment.MaxScore,
                CreatedAt = assignment.CreatedAt,
                ClassroomName = assignment.Classroom.Name,
                ClassroomId = assignment.ClassroomId,
                TeacherId = assignment.TeacherId,
                TotalStudents = assignment.Classroom.Students.Count,
                SubmissionCount = assignment.Submissions.Count,
                GradedCount = assignment.Submissions.Count(s => s.Status == SubmissionStatus.Graded),
                Submissions = assignment.Classroom.Students.Select(student =>
                {
                    var sub = assignment.Submissions.FirstOrDefault(s => s.StudentId == student.Id);
                    return new SubmissionDetailItem
                    {
                        SubmissionId = sub?.Id ?? Guid.Empty,
                        StudentName = student.User.FirstName + " " + student.User.LastName,
                        StudentNumber = student.StudentNumber,
                        Status = sub?.Status ?? SubmissionStatus.Pending,
                        SubmittedAt = sub?.SubmittedAt,
                        Score = sub?.Score,
                        MaxScore = assignment.MaxScore,
                        FileName = sub?.FileName
                    };
                }).ToList(),
                Comments = assignment.Comments
                    .Where(c => c.ParentCommentId == null)
                    .OrderBy(c => c.CreatedAt)
                    .Select(c => new CommentItem
                    {
                        Id = c.Id,
                        Content = c.Content,
                        UserName = c.User.FirstName + " " + c.User.LastName,
                        UserRole = c.User.Role.ToString(),
                        CreatedAt = c.CreatedAt,
                        UserId = c.UserId
                    }).ToList()
            };

            return View(vm);
        }

        [RoleAuthorize("Student")]
        public async Task<IActionResult> MyAssignments(Guid? subjectId = null)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var student = await _context.Students
                .Include(s => s.Submissions)
                .FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null) return NotFound();

            var query = _context.Assignments
                .Include(a => a.Teacher).ThenInclude(t => t.User)
                .Include(a => a.Subject)
                .Where(a => a.ClassroomId == student.ClassroomId);

            if (subjectId.HasValue)
                query = query.Where(a => a.SubjectId == subjectId.Value);

            var assignments = await query.OrderByDescending(a => a.CreatedAt).ToListAsync();
            var submissionDict = student.Submissions.ToDictionary(s => s.AssignmentId);

            var vm = assignments.Select(a =>
            {
                submissionDict.TryGetValue(a.Id, out var sub);
                return new AssignmentViewModel
                {
                    Id = a.Id,
                    Title = a.Title,
                    Description = a.Description,
                    Deadline = a.Deadline,
                    MaxScore = a.MaxScore,
                    CreatedAt = a.CreatedAt,
                    ClassroomId = a.ClassroomId,
                    SubjectId = a.SubjectId,
                    SubjectName = a.Subject?.Name,
                    TeacherName = a.Teacher.User.FirstName + " " + a.Teacher.User.LastName,
                    MySubmissionStatus = sub?.Status,
                    MyScore = sub?.Score,
                    MyTeacherComment = sub?.TeacherComment,
                    MySubmissionId = sub?.Id
                };
            }).ToList();

            return View(vm);
        }

        [RoleAuthorize("Student")]
        public async Task<IActionResult> StudentDetails(Guid id)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var student = await _context.Students
                .Include(s => s.Submissions)
                .FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null) return NotFound();

            var assignment = await _context.Assignments
                .Include(a => a.Teacher).ThenInclude(t => t.User)
                .Include(a => a.Comments).ThenInclude(c => c.User)
                .FirstOrDefaultAsync(a => a.Id == id && a.ClassroomId == student.ClassroomId);
            if (assignment == null) return NotFound();

            var sub = student.Submissions.FirstOrDefault(s => s.AssignmentId == id);

            var vm = new AssignmentViewModel
            {
                Id = assignment.Id,
                Title = assignment.Title,
                Description = assignment.Description,
                Deadline = assignment.Deadline,
                MaxScore = assignment.MaxScore,
                CreatedAt = assignment.CreatedAt,
                TeacherName = assignment.Teacher.User.FirstName + " " + assignment.Teacher.User.LastName,
                MySubmissionStatus = sub?.Status,
                MyScore = sub?.Score,
                MyTeacherComment = sub?.TeacherComment,
                MySubmissionId = sub?.Id,
                Comments = assignment.Comments
                    .Where(c => c.ParentCommentId == null)
                    .OrderBy(c => c.CreatedAt)
                    .Select(c => new CommentItem
                    {
                        Id = c.Id,
                        Content = c.Content,
                        UserName = c.User.FirstName + " " + c.User.LastName,
                        UserRole = c.User.Role.ToString(),
                        CreatedAt = c.CreatedAt,
                        UserId = c.UserId
                    }).ToList()
            };

            return View(vm);
        }
    }
}