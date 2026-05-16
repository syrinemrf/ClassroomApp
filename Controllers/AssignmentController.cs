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
        private readonly IFileService _fileService;
        private readonly IEmailService _emailService;

        public AssignmentController(
            AppDbContext context,
            INotificationService notificationService,
            IFileService fileService,
            IEmailService emailService)
        {
            _context = context;
            _notificationService = notificationService;
            _fileService = fileService;
            _emailService = emailService;
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
        [RequestSizeLimit(52_428_800)]
        [RequestFormLimits(MultipartBodyLengthLimit = 52_428_800)]
        public async Task<IActionResult> Create(CreateAssignmentViewModel model)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var teacher = await _context.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.UserId == userId);
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

            if (model.AttachmentFile != null && model.AttachmentFile.Length > 0
                && !_fileService.IsAllowedCourseFile(model.AttachmentFile))
            {
                ModelState.AddModelError("AttachmentFile",
                    "Type ou taille invalide (max 50 MB). Formats : PDF, DOCX, PPTX, ZIP, images.");
            }

            if (!ModelState.IsValid)
            {
                await ReloadDropdowns();
                return View(model);
            }

            string? filePath = null;
            string? fileName = null;
            long? fileSize = null;
            string? contentType = null;

            if (model.AttachmentFile != null && model.AttachmentFile.Length > 0)
            {
                try
                {
                    filePath = await _fileService.SaveFileAsync(model.AttachmentFile, "assignments");
                    fileName = model.AttachmentFile.FileName;
                    fileSize = model.AttachmentFile.Length;
                    contentType = model.AttachmentFile.ContentType;
                }
                catch
                {
                    ModelState.AddModelError("AttachmentFile", "Erreur lors de l'enregistrement du fichier.");
                    await ReloadDropdowns();
                    return View(model);
                }
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
                FilePath = filePath,
                FileName = fileName,
                FileSize = fileSize,
                ContentType = contentType,
                CreatedAt = DateTime.UtcNow
            };

            _context.Assignments.Add(assignment);

            // Create a calendar event for the deadline
            var calEvent = new CalendarEvent
            {
                Id = Guid.NewGuid(),
                Title = $"Deadline : {model.Title}",
                Description = $"Devoir '{model.Title}' — Note max : {model.MaxScore}",
                StartDate = model.Deadline.AddHours(-1),
                EndDate = model.Deadline,
                Color = "#EF4444",
                CreatedByUserId = userId,
                ClassroomId = model.ClassroomId
            };
            _context.CalendarEvents.Add(calEvent);

            await _context.SaveChangesAsync();

            // Load students for notifications and emails
            var students = await _context.Students
                .Include(s => s.User)
                .Where(s => s.ClassroomId == model.ClassroomId)
                .ToListAsync();

            try
            {
                await _notificationService.NotifyClassroomAsync(
                    model.ClassroomId,
                    "Nouveau devoir",
                    $"Nouveau devoir '{model.Title}'. Date limite : {model.Deadline:dd MMM yyyy HH:mm}",
                    NotificationType.NewAssignment,
                    assignment.Id,
                    "Assignment"
                );
            }
            catch { /* Notification failure must not block assignment creation */ }

            // Send email to each student in the classroom
            foreach (var student in students)
            {
                try
                {
                    var teacherFullName = teacher.User.FirstName + " " + teacher.User.LastName;
                    var emailBody = BuildNewAssignmentEmail(
                        student.User.FirstName,
                        model.Title,
                        model.Description,
                        model.Deadline,
                        model.MaxScore,
                        teacherFullName,
                        fileName
                    );
                    await _emailService.SendAsync(
                        student.User.Email,
                        student.User.FirstName + " " + student.User.LastName,
                        $"[ClassroomApp] Nouveau devoir : {model.Title}",
                        emailBody
                    );
                }
                catch { /* Email failure must not block assignment creation */ }
            }

            TempData["Success"] = "Devoir créé avec succès.";

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
                ExistingFileName = assignment.FileName,
                AvailableSubjects = teacherSubjects.ToDictionary(s => s.Id, s => s.Name),
                AvailableClassrooms = await _context.Classrooms.OrderBy(c => c.Name).ToDictionaryAsync(c => c.Id, c => c.Name)
            };
            ViewBag.AssignmentId = id;
            return View(model);
        }

        [RoleAuthorize("Teacher")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(52_428_800)]
        [RequestFormLimits(MultipartBodyLengthLimit = 52_428_800)]
        public async Task<IActionResult> Edit(Guid id, CreateAssignmentViewModel model)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);
            if (teacher == null) return NotFound();

            if (model.AttachmentFile != null && model.AttachmentFile.Length > 0
                && !_fileService.IsAllowedCourseFile(model.AttachmentFile))
            {
                ModelState.AddModelError("AttachmentFile",
                    "Type ou taille invalide (max 50 MB). Formats : PDF, DOCX, PPTX, ZIP, images.");
            }

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

            // Replace attachment if a new file was provided
            if (model.AttachmentFile != null && model.AttachmentFile.Length > 0)
            {
                if (!string.IsNullOrEmpty(assignment.FilePath))
                    _fileService.DeleteFile(assignment.FilePath);

                assignment.FilePath = await _fileService.SaveFileAsync(model.AttachmentFile, "assignments");
                assignment.FileName = model.AttachmentFile.FileName;
                assignment.FileSize = model.AttachmentFile.Length;
                assignment.ContentType = model.AttachmentFile.ContentType;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Devoir modifié avec succès.";

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

            if (!string.IsNullOrEmpty(assignment.FilePath))
                _fileService.DeleteFile(assignment.FilePath);

            _context.Assignments.Remove(assignment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Devoir supprimé avec succès.";

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
                AttachmentFileName = assignment.FileName,
                AttachmentFilePath = assignment.FilePath,
                AttachmentFileSize = assignment.FileSize,
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
                AttachmentFileName = assignment.FileName,
                AttachmentFilePath = assignment.FilePath,
                AttachmentFileSize = assignment.FileSize,
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

        /// <summary>Downloads the teacher-attached file for an assignment.</summary>
        [RoleAuthorize("Student", "Teacher")]
        [HttpGet]
        public async Task<IActionResult> Download(Guid id)
        {
            var assignment = await _context.Assignments
                .Include(a => a.Classroom)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (assignment == null || string.IsNullOrEmpty(assignment.FilePath))
                return NotFound();

            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = User.FindFirst(ClaimTypes.Role)!.Value;

            if (role == "Student")
            {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
                if (student == null || student.ClassroomId != assignment.ClassroomId)
                    return Forbid();
            }
            else if (role == "Teacher")
            {
                var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);
                if (teacher == null || assignment.TeacherId != teacher.Id)
                    return Forbid();
            }

            var stream = _fileService.GetFileStream(assignment.FilePath);
            if (stream == null) return NotFound();

            var ct = string.IsNullOrEmpty(assignment.ContentType) ? "application/octet-stream" : assignment.ContentType;
            return File(stream, ct, assignment.FileName ?? "attachment");
        }

        // --- Private helpers ---

        private static string BuildNewAssignmentEmail(
            string studentFirstName,
            string title,
            string description,
            DateTime deadline,
            int maxScore,
            string teacherName,
            string? attachmentFileName)
        {
            var attachmentSection = !string.IsNullOrEmpty(attachmentFileName)
                ? $"<p style='margin:12px 0;'><strong>Fichier joint :</strong> {System.Net.WebUtility.HtmlEncode(attachmentFileName)}</p>"
                : string.Empty;

            return $"""
                <!DOCTYPE html>
                <html>
                <body style="font-family:Arial,sans-serif;color:#222;max-width:600px;margin:auto;">
                  <div style="background:#4f46e5;padding:24px;border-radius:8px 8px 0 0;">
                    <h2 style="color:#fff;margin:0;">Nouveau devoir assign&eacute;</h2>
                  </div>
                  <div style="padding:24px;border:1px solid #e5e7eb;border-top:0;border-radius:0 0 8px 8px;">
                    <p>Bonjour <strong>{System.Net.WebUtility.HtmlEncode(studentFirstName)}</strong>,</p>
                    <p>Un nouveau devoir a &eacute;t&eacute; publi&eacute; par <strong>{System.Net.WebUtility.HtmlEncode(teacherName)}</strong>&nbsp;:</p>
                    <table style="width:100%;border-collapse:collapse;margin:16px 0;">
                      <tr style="background:#f3f4f6;">
                        <td style="padding:10px;font-weight:bold;width:40%;">Titre</td>
                        <td style="padding:10px;">{System.Net.WebUtility.HtmlEncode(title)}</td>
                      </tr>
                      <tr>
                        <td style="padding:10px;font-weight:bold;">Consignes</td>
                        <td style="padding:10px;">{System.Net.WebUtility.HtmlEncode(description)}</td>
                      </tr>
                      <tr style="background:#f3f4f6;">
                        <td style="padding:10px;font-weight:bold;">Date limite</td>
                        <td style="padding:10px;"><strong style="color:#dc2626;">{deadline:dd/MM/yyyy HH:mm} UTC</strong></td>
                      </tr>
                      <tr>
                        <td style="padding:10px;font-weight:bold;">Note maximale</td>
                        <td style="padding:10px;">{maxScore} pts</td>
                      </tr>
                    </table>
                    {attachmentSection}
                    <p style="color:#6b7280;font-size:13px;margin-top:24px;">Connectez-vous &agrave; ClassroomApp pour soumettre votre travail.</p>
                  </div>
                </body>
                </html>
                """;
        }
    }
}