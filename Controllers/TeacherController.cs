using ClassroomApp.Data;
using ClassroomApp.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ClassroomApp.Controllers
{
    [RoleAuthorize("Teacher")]
    public class TeacherController : Controller
    {
        private readonly AppDbContext _context;

        public TeacherController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var teacher = await _context.Teachers
                .Include(t => t.User)
                .Include(t => t.Courses).ThenInclude(c => c.CourseClassrooms).ThenInclude(cc => cc.Classroom).ThenInclude(cl => cl.Students)
                .Include(t => t.Assignments).ThenInclude(a => a.Submissions)
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (teacher == null) return NotFound();

            var allClassrooms = teacher.Courses
                .SelectMany(c => c.CourseClassrooms.Select(cc => cc.Classroom))
                .DistinctBy(cl => cl.Id)
                .ToList();

            ViewBag.TeacherName = $"{teacher.User.FirstName} {teacher.User.LastName}";
            ViewBag.ProfilePicture = teacher.User.ProfilePicturePath;
            ViewBag.ClassroomCount = allClassrooms.Count;
            ViewBag.CourseCount = teacher.Courses.Count;
            ViewBag.AssignmentCount = teacher.Assignments.Count;
            ViewBag.PendingGrades = teacher.Assignments
                .SelectMany(a => a.Submissions)
                .Count(s => s.Status == Models.Enums.SubmissionStatus.Submitted || s.Status == Models.Enums.SubmissionStatus.Late);

            var now = DateTime.UtcNow;
            ViewBag.UpcomingDeadlines = teacher.Assignments
                .Where(a => a.Deadline > now && a.Deadline <= now.AddDays(7))
                .OrderBy(a => a.Deadline)
                .Take(5)
                .ToList();

            ViewBag.RecentAssignments = teacher.Assignments
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .ToList();

            return View();
        }
    }
}
