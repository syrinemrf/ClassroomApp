using ClassroomApp.Data;
using ClassroomApp.Filters;
using ClassroomApp.Models.Entities;
using ClassroomApp.Models.Enums;
using ClassroomApp.Models.ViewModels.Course;
using ClassroomApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ClassroomApp.Controllers
{
    public class CourseController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IFileService _fileService;
        private readonly INotificationService _notificationService;

        public CourseController(AppDbContext context, IFileService fileService, INotificationService notificationService)
        {
            _context = context;
            _fileService = fileService;
            _notificationService = notificationService;
        }

        [RoleAuthorize("Teacher")]
        public async Task<IActionResult> Index(int page = 1)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);
            if (teacher == null) return NotFound();

            var pageSize = 10;
            var query = _context.Courses
                .Include(c => c.Subject)
                .Include(c => c.CourseClassrooms).ThenInclude(cc => cc.Classroom)
                .Where(c => c.TeacherId == teacher.Id)
                .OrderByDescending(c => c.CreatedAt);

            var courses = (await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync())
                .Select(c => new CourseViewModel
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    FileName = c.FileName,
                    FileSize = c.FileSize,
                    ContentType = c.ContentType,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    SubjectId = c.SubjectId,
                    SubjectName = c.Subject?.Name,
                    ClassroomNames = c.CourseClassrooms.Select(cc => cc.Classroom.Name).ToList(),
                    ClassroomIds = c.CourseClassrooms.Select(cc => cc.ClassroomId).ToList(),
                    TeacherId = c.TeacherId
                })
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(await query.CountAsync() / (double)pageSize);
            return View(courses);
        }

        [RoleAuthorize("Teacher")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);
            if (teacher == null) return NotFound();

            var subjectIdParam = Request.Query["subjectId"].FirstOrDefault();
            Guid? preselectedSubjectId = Guid.TryParse(subjectIdParam, out var parsedSubjectId) ? parsedSubjectId : null;

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

            var model = new CreateCourseViewModel
            {
                SubjectId = preselectedSubjectId ?? Guid.Empty,
                AvailableSubjects = teacherSubjects.ToDictionary(s => s.Id, s => s.Name),
                AvailableClassrooms = availableClassrooms
            };
            return View(model);
        }

        [RoleAuthorize("Teacher")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(52_428_800)] // 50 MB
        [RequestFormLimits(MultipartBodyLengthLimit = 52_428_800)]
        public async Task<IActionResult> Create(CreateCourseViewModel model)
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
                if (model.SubjectId != Guid.Empty)
                {
                    var sub = subjects.FirstOrDefault(s => s.Id == model.SubjectId);
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

            var subject = await _context.Subjects
                .Include(s => s.SubjectClassrooms)
                .FirstOrDefaultAsync(s => s.Id == model.SubjectId && s.TeacherId == teacher.Id);
            if (subject == null)
            {
                ModelState.AddModelError("SubjectId", "Matiere invalide.");
                await ReloadDropdowns();
                return View(model);
            }

            if (!_fileService.IsAllowedCourseFile(model.File))
            {
                ModelState.AddModelError("File", "Type ou taille de fichier invalide (max 50MB). Autorises : PDF, DOCX, PPTX, ZIP, images.");
                await ReloadDropdowns();
                return View(model);
            }

            if (model.ClassroomIds == null || !model.ClassroomIds.Any())
            {
                ModelState.AddModelError("ClassroomIds", "Veuillez selectionner au moins une classe.");
                await ReloadDropdowns();
                return View(model);
            }

            string filePath;
            try
            {
                filePath = await _fileService.SaveFileAsync(model.File, "courses");
            }
            catch (Exception)
            {
                ModelState.AddModelError("File", "Une erreur est survenue lors de l'enregistrement du fichier.");
                await ReloadDropdowns();
                return View(model);
            }

            var course = new Course
            {
                Id = Guid.NewGuid(),
                Title = model.Title,
                Description = model.Description,
                FilePath = filePath,
                FileName = model.File.FileName,
                FileSize = model.File.Length,
                ContentType = model.File.ContentType,
                TeacherId = teacher.Id,
                SubjectId = model.SubjectId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Courses.Add(course);

            foreach (var classroomId in model.ClassroomIds)
            {
                _context.CourseClassrooms.Add(new CourseClassroom
                {
                    CourseId = course.Id,
                    ClassroomId = classroomId
                });
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                _fileService.DeleteFile(filePath);
                ModelState.AddModelError(string.Empty, "Une erreur est survenue lors de l'enregistrement du cours.");
                await ReloadDropdowns();
                return View(model);
            }

            // Send notifications after the course is committed to the database
            foreach (var classroomId in model.ClassroomIds)
            {
                try
                {
                    await _notificationService.NotifyClassroomAsync(
                        classroomId,
                        "Nouveau cours",
                        $"Un nouveau cours '{model.Title}' a \u00e9t\u00e9 mis en ligne.",
                        NotificationType.NewCourse,
                        course.Id,
                        "Course"
                    );
                }
                catch
                {
                    // Notification failure must not block the course creation
                }
            }

            TempData["Success"] = "Cours mis en ligne avec succ\u00e8s.";

            if (model.SubjectId != Guid.Empty)
                return RedirectToAction("Details", "Subject", new { id = model.SubjectId });

            return RedirectToAction(nameof(Index));
        }

        [RoleAuthorize("Teacher")]
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);
            if (teacher == null) return NotFound();

            var course = await _context.Courses
                .Include(c => c.CourseClassrooms).ThenInclude(cc => cc.Classroom)
                .FirstOrDefaultAsync(c => c.Id == id && c.TeacherId == teacher.Id);
            if (course == null) return NotFound();

            var model = new EditCourseViewModel
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                FileName = course.FileName,
                ClassroomNames = course.CourseClassrooms.Select(cc => cc.Classroom.Name).ToList()
            };
            return View(model);
        }

        [RoleAuthorize("Teacher")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditCourseViewModel model)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);
            if (teacher == null) return NotFound();

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == model.Id && c.TeacherId == teacher.Id);
            if (course == null) return NotFound();

            course.Title = model.Title;
            course.Description = model.Description;
            course.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Cours modifie avec succes.";

            var refSubjectId = Request.Form["returnSubjectId"].FirstOrDefault();
            if (Guid.TryParse(refSubjectId, out var subId))
                return RedirectToAction("Details", "Subject", new { id = subId });

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

            var course = await _context.Courses
                .Include(c => c.Subject)
                .FirstOrDefaultAsync(c => c.Id == id && c.TeacherId == teacher.Id);
            if (course == null) return NotFound();

            var subjectId = course.SubjectId;
            _fileService.DeleteFile(course.FilePath);
            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Cours supprime avec succes.";

            var returnSubjectId = Request.Form["returnSubjectId"].FirstOrDefault();
            if (Guid.TryParse(returnSubjectId, out var returnId))
                return RedirectToAction("Details", "Subject", new { id = returnId });

            return RedirectToAction(nameof(Index));
        }

        [RoleAuthorize("Student")]
        public async Task<IActionResult> MyClassroomCourses(Guid? subjectId = null)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null) return NotFound();

            var query = _context.Courses
                .Include(c => c.Teacher).ThenInclude(t => t.User)
                .Include(c => c.Subject)
                .Include(c => c.CourseClassrooms).ThenInclude(cc => cc.Classroom)
                .Where(c => c.CourseClassrooms.Any(cc => cc.ClassroomId == student.ClassroomId));

            if (subjectId.HasValue)
                query = query.Where(c => c.SubjectId == subjectId.Value);

            var courses = (await query.OrderByDescending(c => c.CreatedAt).ToListAsync())
                .Select(c => new CourseViewModel
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    FileName = c.FileName,
                    FileSize = c.FileSize,
                    ContentType = c.ContentType,
                    CreatedAt = c.CreatedAt,
                    SubjectId = c.SubjectId,
                    SubjectName = c.Subject?.Name,
                    TeacherName = c.Teacher.User.FirstName + " " + c.Teacher.User.LastName,
                    ClassroomNames = c.CourseClassrooms.Select(cc => cc.Classroom.Name).ToList(),
                    ClassroomIds = c.CourseClassrooms.Select(cc => cc.ClassroomId).ToList(),
                    TeacherId = c.TeacherId
                })
                .ToList();

            return View(courses);
        }

        [RoleAuthorize("Student", "Teacher")]
        public async Task<IActionResult> Download(Guid id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = User.FindFirst(ClaimTypes.Role)!.Value;

            if (role == "Student")
            {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
                if (student == null) return Forbid();
                var hasAccess = await _context.CourseClassrooms
                    .AnyAsync(cc => cc.CourseId == course.Id && cc.ClassroomId == student.ClassroomId);
                if (!hasAccess) return Forbid();
            }
            else if (role == "Teacher")
            {
                var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);
                if (teacher == null) return Forbid();
                if (course.TeacherId != teacher.Id) return Forbid();
            }

            var stream = _fileService.GetFileStream(course.FilePath);
            if (stream == null) return NotFound();

            return File(stream, course.ContentType, course.FileName);
        }
    }
}