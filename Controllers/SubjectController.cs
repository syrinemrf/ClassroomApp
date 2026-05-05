using ClassroomApp.Data;
using ClassroomApp.Filters;
using ClassroomApp.Models.ViewModels.Assignment;
using ClassroomApp.Models.ViewModels.Course;
using ClassroomApp.Models.ViewModels.Subject;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ClassroomApp.Controllers
{
    public class SubjectController : Controller
    {
        private readonly AppDbContext _context;

        public SubjectController(AppDbContext context)
        {
            _context = context;
        }

        // Teacher: card grid of their subjects
        [RoleAuthorize("Teacher")]
        public async Task<IActionResult> Index()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var teacher = await _context.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.UserId == userId);
            if (teacher == null) return NotFound();

            var subjects = await _context.Subjects
                .Include(s => s.SubjectClassrooms)
                    .ThenInclude(sc => sc.Classroom)
                    .ThenInclude(c => c.Students)
                .Include(s => s.Courses)
                .Include(s => s.Assignments)
                .Where(s => s.TeacherId == teacher.Id)
                .OrderBy(s => s.Name)
                .ToListAsync();

            var cards = subjects.Select(s =>
            {
                var now = DateTime.UtcNow;
                var firstName = teacher.User.FirstName;
                var lastName = teacher.User.LastName;
                return new SubjectCardViewModel
                {
                    SubjectId = s.Id,
                    SubjectName = s.Name,
                    Description = s.Description,
                    Color = s.Color,
                    TeacherName = firstName + " " + lastName,
                    TeacherInitials = (firstName.Length > 0 ? firstName[0].ToString() : "") + (lastName.Length > 0 ? lastName[0].ToString() : ""),
                    TeacherProfilePicture = teacher.User.ProfilePicturePath,
                    ClassroomNames = s.SubjectClassrooms.Select(sc => sc.Classroom.Name).ToList(),
                    CourseCount = s.Courses.Count,
                    AssignmentCount = s.Assignments.Count,
                    PendingAssignments = s.Assignments.Count(a => a.Deadline > now),
                    StudentCount = s.SubjectClassrooms.Sum(sc => sc.Classroom.Students.Count)
                };
            }).ToList();

            ViewData["Title"] = "Mes matieres";
            return View(cards);
        }

        // Student: card grid of subjects in their classroom
        [RoleAuthorize("Student")]
        public async Task<IActionResult> StudentIndex()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var student = await _context.Students
                .Include(s => s.Classroom)
                .Include(s => s.Submissions)
                .FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null) return NotFound();

            var subjects = await _context.Subjects
                .Include(s => s.Teacher).ThenInclude(t => t.User)
                .Include(s => s.SubjectClassrooms).ThenInclude(sc => sc.Classroom)
                .Include(s => s.Courses).ThenInclude(c => c.CourseClassrooms)
                .Include(s => s.Assignments)
                .Where(s => s.SubjectClassrooms.Any(sc => sc.ClassroomId == student.ClassroomId))
                .OrderBy(s => s.Name)
                .ToListAsync();

            var now = DateTime.UtcNow;
            var submittedIds = student.Submissions.Select(sub => sub.AssignmentId).ToHashSet();

            var cards = subjects.Select(s =>
            {
                var tFirst = s.Teacher.User.FirstName;
                var tLast = s.Teacher.User.LastName;
                return new SubjectCardViewModel
                {
                    SubjectId = s.Id,
                    SubjectName = s.Name,
                    Description = s.Description,
                    Color = s.Color,
                    TeacherName = tFirst + " " + tLast,
                    TeacherInitials = (tFirst.Length > 0 ? tFirst[0].ToString() : "") + (tLast.Length > 0 ? tLast[0].ToString() : ""),
                    TeacherProfilePicture = s.Teacher.User.ProfilePicturePath,
                    ClassroomNames = s.SubjectClassrooms.Select(sc => sc.Classroom.Name).ToList(),
                    CourseCount = s.Courses.Count(c => c.CourseClassrooms.Any(cc => cc.ClassroomId == student.ClassroomId)),
                    AssignmentCount = s.Assignments.Count(a => a.ClassroomId == student.ClassroomId),
                    PendingAssignments = s.Assignments.Count(a => a.ClassroomId == student.ClassroomId && a.Deadline > now && !submittedIds.Contains(a.Id))
                };
            }).ToList();

            ViewData["Title"] = "Mes matieres";
            return View(cards);
        }

        // Shared detail page (Stream / Classwork / People)
        [RoleAuthorize("Teacher", "Student")]
        public async Task<IActionResult> Details(Guid id, string tab = "stream")
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = User.FindFirst(ClaimTypes.Role)!.Value;

            var subject = await _context.Subjects
                .Include(s => s.Teacher).ThenInclude(t => t.User)
                .Include(s => s.SubjectClassrooms).ThenInclude(sc => sc.Classroom).ThenInclude(c => c.Students).ThenInclude(st => st.User)
                .Include(s => s.Courses).ThenInclude(c => c.CourseClassrooms)
                .Include(s => s.Assignments).ThenInclude(a => a.Submissions)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (subject == null) return NotFound();

            // Access check
            if (role == "Teacher")
            {
                var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);
                if (teacher == null || subject.TeacherId != teacher.Id) return Forbid();
            }
            else
            {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
                if (student == null) return NotFound();
                if (!subject.SubjectClassrooms.Any(sc => sc.ClassroomId == student.ClassroomId)) return Forbid();
            }

            Guid? studentClassroomId = null;
            Guid? myStudentId = null;

            if (role == "Student")
            {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
                studentClassroomId = student?.ClassroomId;
                myStudentId = student?.Id;
            }

            var tFirst = subject.Teacher.User.FirstName;
            var tLast = subject.Teacher.User.LastName;

            // Filter classrooms if student
            var classrooms = subject.SubjectClassrooms
                .Select(sc => new ClassroomInfo
                {
                    Id = sc.ClassroomId,
                    Name = sc.Classroom.Name,
                    StudentCount = sc.Classroom.Students.Count
                }).ToList();

            // Courses visible to the current user
            var courses = subject.Courses
                .Where(c => studentClassroomId == null || c.CourseClassrooms.Any(cc => cc.ClassroomId == studentClassroomId))
                .OrderByDescending(c => c.CreatedAt)
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
                    TeacherId = c.TeacherId,
                    ClassroomNames = c.CourseClassrooms.Select(cc => cc.ClassroomId.ToString()).ToList()
                }).ToList();

            // Assignments visible
            var assignments = subject.Assignments
                .Where(a => studentClassroomId == null || a.ClassroomId == studentClassroomId)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a =>
                {
                    var mySub = myStudentId.HasValue ? a.Submissions.FirstOrDefault(s => s.StudentId == myStudentId.Value) : null;
                    return new AssignmentViewModel
                    {
                        Id = a.Id,
                        Title = a.Title,
                        Description = a.Description,
                        Deadline = a.Deadline,
                        MaxScore = a.MaxScore,
                        CreatedAt = a.CreatedAt,
                        ClassroomId = a.ClassroomId,
                        TeacherId = a.TeacherId,
                        SubjectId = a.SubjectId,
                        TotalStudents = subject.SubjectClassrooms
                            .FirstOrDefault(sc => sc.ClassroomId == a.ClassroomId)?.Classroom.Students.Count ?? 0,
                        SubmissionCount = a.Submissions.Count,
                        GradedCount = a.Submissions.Count(s => s.Status == Models.Enums.SubmissionStatus.Graded),
                        MySubmissionStatus = mySub?.Status,
                        MyScore = mySub?.Score,
                        MyTeacherComment = mySub?.TeacherComment,
                        MySubmissionId = mySub?.Id
                    };
                }).ToList();

            // Stream: combine courses and assignments ordered by date
            var streamItems = new List<StreamItem>();
            foreach (var c in courses.Take(10))
            {
                streamItems.Add(new StreamItem
                {
                    Id = c.Id,
                    Type = "Course",
                    Title = c.Title,
                    Description = c.Description,
                    CreatedAt = c.CreatedAt,
                    AuthorName = tFirst + " " + tLast,
                    AuthorInitials = (tFirst.Length > 0 ? tFirst[0].ToString() : "") + (tLast.Length > 0 ? tLast[0].ToString() : ""),
                    AuthorPicture = subject.Teacher.User.ProfilePicturePath,
                    FileName = c.FileName,
                    FileIcon = c.FileTypeIcon
                });
            }
            foreach (var a in assignments.Take(10))
            {
                streamItems.Add(new StreamItem
                {
                    Id = a.Id,
                    Type = "Assignment",
                    Title = a.Title,
                    Description = a.Description,
                    CreatedAt = a.CreatedAt,
                    AuthorName = tFirst + " " + tLast,
                    AuthorInitials = (tFirst.Length > 0 ? tFirst[0].ToString() : "") + (tLast.Length > 0 ? tLast[0].ToString() : ""),
                    AuthorPicture = subject.Teacher.User.ProfilePicturePath,
                    Deadline = a.Deadline
                });
            }
            streamItems = streamItems.OrderByDescending(i => i.CreatedAt).ToList();

            // People
            var students = subject.SubjectClassrooms
                .SelectMany(sc => sc.Classroom.Students, (sc, st) => new PersonInfo
                {
                    Id = st.Id,
                    Name = st.User.FirstName + " " + st.User.LastName,
                    ProfilePicture = st.User.ProfilePicturePath,
                    Initials = (st.User.FirstName.Length > 0 ? st.User.FirstName[0].ToString() : "") +
                               (st.User.LastName.Length > 0 ? st.User.LastName[0].ToString() : ""),
                    StudentNumber = st.StudentNumber,
                    ClassroomName = sc.Classroom.Name
                })
                .OrderBy(s => s.ClassroomName).ThenBy(s => s.Name)
                .ToList();

            var vm = new SubjectDetailsViewModel
            {
                SubjectId = subject.Id,
                SubjectName = subject.Name,
                SubjectDescription = subject.Description,
                Color = subject.Color,
                TeacherName = tFirst + " " + tLast,
                TeacherInitials = (tFirst.Length > 0 ? tFirst[0].ToString() : "") + (tLast.Length > 0 ? tLast[0].ToString() : ""),
                TeacherProfilePicture = subject.Teacher.User.ProfilePicturePath,
                TeacherUserId = subject.Teacher.UserId,
                IsTeacher = role == "Teacher",
                Classrooms = classrooms,
                StreamItems = streamItems,
                Courses = courses,
                Assignments = assignments,
                Students = students
            };

            ViewData["Title"] = subject.Name;
            ViewBag.ActiveTab = tab;
            return View(vm);
        }
    }
}