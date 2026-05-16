using ClassroomApp.Data;
using ClassroomApp.Filters;
using ClassroomApp.Models.Entities;
using ClassroomApp.Models.Enums;
using ClassroomApp.Models.ViewModels.Submission;
using ClassroomApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ClassroomApp.Controllers
{
    public class SubmissionController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IFileService _fileService;
        private readonly INotificationService _notificationService;

        public SubmissionController(AppDbContext context, IFileService fileService, INotificationService notificationService)
        {
            _context = context;
            _fileService = fileService;
            _notificationService = notificationService;
        }

        [RoleAuthorize("Student")]
        [HttpGet]
        public async Task<IActionResult> Submit(Guid assignmentId)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null) return NotFound();

            var assignment = await _context.Assignments
                .Include(a => a.Classroom)
                .FirstOrDefaultAsync(a => a.Id == assignmentId && a.ClassroomId == student.ClassroomId);
            if (assignment == null) return NotFound();

            var existingSubmission = await _context.Submissions
                .FirstOrDefaultAsync(s => s.StudentId == student.Id && s.AssignmentId == assignmentId);

            ViewBag.Assignment = assignment;
            ViewBag.ExistingSubmission = existingSubmission;
            return View();
        }

        [RoleAuthorize("Student")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(Guid assignmentId, IFormFile file)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null) return NotFound();

            var assignment = await _context.Assignments.FindAsync(assignmentId);
            if (assignment == null) return NotFound();

            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Veuillez sélectionner un fichier.";
                return RedirectToAction(nameof(Submit), new { assignmentId });
            }

            if (!_fileService.IsAllowedSubmissionFile(file))
            {
                TempData["Error"] = "Type ou taille de fichier invalide (maximum 20MB).";
                return RedirectToAction(nameof(Submit), new { assignmentId });
            }

            var existingSubmission = await _context.Submissions
                .FirstOrDefaultAsync(s => s.StudentId == student.Id && s.AssignmentId == assignmentId);

            if (existingSubmission != null)
            {
                TempData["Error"] = "Vous avez déjà remis un devoir pour cet assignement.";
                return RedirectToAction(nameof(Submit), new { assignmentId });
            }

            var filePath = await _fileService.SaveFileAsync(file, "submissions");
            var now = DateTime.UtcNow;
            var isLate = now > assignment.Deadline;

            var submission = new Submission
            {
                Id = Guid.NewGuid(),
                StudentId = student.Id,
                AssignmentId = assignmentId,
                FilePath = filePath,
                FileName = file.FileName,
                FileSize = file.Length,
                SubmittedAt = now,
                Status = isLate ? SubmissionStatus.Late : SubmissionStatus.Submitted
            };

            _context.Submissions.Add(submission);
            await _context.SaveChangesAsync();

            TempData["Success"] = isLate
                ? "Remise chargée (marquée comme tard)."
                : "Remise chargée avec succès.";

            return RedirectToAction("MyAssignments", "Assignment");
        }

        [RoleAuthorize("Student")]
        public async Task<IActionResult> MySubmissions(int page = 1, SubmissionStatus? status = null)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null) return NotFound();

            var pageSize = 10;
            var query = _context.Submissions
                .Include(s => s.Assignment).ThenInclude(a => a.Classroom)
                .Where(s => s.StudentId == student.Id);

            if (status.HasValue)
                query = query.Where(s => s.Status == status.Value);

            var total = await query.CountAsync();
            var submissions = await query
                .OrderByDescending(s => s.SubmittedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new SubmissionItem
                {
                    Id = s.Id,
                    AssignmentTitle = s.Assignment.Title,
                    ClassroomName = s.Assignment.Classroom.Name,
                    SubmittedAt = s.SubmittedAt,
                    Status = s.Status,
                    Score = s.Score,
                    MaxScore = s.Assignment.MaxScore,
                    TeacherComment = s.TeacherComment,
                    FileName = s.FileName,
                    Deadline = s.Assignment.Deadline
                })
                .ToListAsync();

            var vm = new MySubmissionsViewModel
            {
                Submissions = submissions,
                FilterStatus = status,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize)
            };
            return View(vm);
        }

        [RoleAuthorize("Teacher")]
        [HttpGet]
        public async Task<IActionResult> GradeStudent(Guid assignmentId, Guid studentId)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);
            if (teacher == null) return Forbid();

            var assignment = await _context.Assignments.FirstOrDefaultAsync(a => a.Id == assignmentId && a.TeacherId == teacher.Id);
            if (assignment == null) return NotFound();

            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == studentId);
            if (student == null) return NotFound();

            var submission = await _context.Submissions
                .FirstOrDefaultAsync(s => s.StudentId == studentId && s.AssignmentId == assignmentId);

            if (submission == null)
            {
                submission = new Submission
                {
                    Id = Guid.NewGuid(),
                    StudentId = studentId,
                    AssignmentId = assignmentId,
                    Status = SubmissionStatus.Pending,
                    SubmittedAt = DateTime.UtcNow
                };
                _context.Submissions.Add(submission);
                await _context.SaveChangesAsync();
            }

            var vm = new GradeSubmissionViewModel
            {
                SubmissionId = submission.Id,
                StudentName = $"{student.User.FirstName} {student.User.LastName}",
                StudentNumber = student.StudentNumber,
                AssignmentTitle = assignment.Title,
                MaxScore = assignment.MaxScore,
                FileName = submission.FileName ?? "(Pas de remise)",
                SubmittedAt = submission.SubmittedAt,
                Status = submission.Status.ToString(),
                Score = submission.Score ?? 0,
                TeacherComment = submission.TeacherComment
            };
            return View("Grade", vm);
        }

        [RoleAuthorize("Teacher")]
        [HttpGet]
        public async Task<IActionResult> Grade(Guid id)
        {
            var submission = await _context.Submissions
                .Include(s => s.Student).ThenInclude(st => st.User)
                .Include(s => s.Assignment)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (submission == null) return NotFound();

            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);
            if (teacher == null || submission.Assignment.TeacherId != teacher.Id) return Forbid();

            var vm = new GradeSubmissionViewModel
            {
                SubmissionId = submission.Id,
                StudentName = $"{submission.Student.User.FirstName} {submission.Student.User.LastName}",
                StudentNumber = submission.Student.StudentNumber,
                AssignmentTitle = submission.Assignment.Title,
                MaxScore = submission.Assignment.MaxScore,
                FileName = submission.FileName,
                SubmittedAt = submission.SubmittedAt,
                Status = submission.Status.ToString(),
                Score = submission.Score ?? 0,
                TeacherComment = submission.TeacherComment
            };
            return View(vm);
        }

        [RoleAuthorize("Teacher")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Grade(GradeSubmissionViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var submission = await _context.Submissions
                .Include(s => s.Student)
                .Include(s => s.Assignment)
                .FirstOrDefaultAsync(s => s.Id == model.SubmissionId);

            if (submission == null) return NotFound();

            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);
            if (teacher == null || submission.Assignment.TeacherId != teacher.Id) return Forbid();

            submission.Score = model.Score;
            submission.TeacherComment = model.TeacherComment;
            submission.Status = SubmissionStatus.Graded;
            submission.GradedAt = DateTime.UtcNow;
            submission.GradedByTeacherId = teacher.Id;

            await _context.SaveChangesAsync();

            await _notificationService.CreateNotificationAsync(
                submission.Student.UserId,
                "Nouvelle note !",
                $"Votre devoir '{submission.Assignment.Title}' a ete note : {model.Score}/{submission.Assignment.MaxScore}",
                NotificationType.NewGrade,
                submission.AssignmentId,
                "Assignment"
            );

            TempData["Success"] = "Devoir noté avec succès.";
            return RedirectToAction("Details", "Assignment", new { id = submission.AssignmentId });
        }

        [RoleAuthorize("Teacher")]
        public async Task<IActionResult> Download(Guid id)
        {
            var submission = await _context.Submissions
                .Include(s => s.Assignment)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (submission == null || string.IsNullOrEmpty(submission.FilePath)) return NotFound();

            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);
            if (teacher == null || submission.Assignment.TeacherId != teacher.Id) return Forbid();

            var stream = _fileService.GetFileStream(submission.FilePath);
            if (stream == null) return NotFound();

            return File(stream, "application/octet-stream", submission.FileName ?? "submission");
        }
    }
}
