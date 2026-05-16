using ClassroomApp.Data;
using ClassroomApp.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ClassroomApp.Controllers
{
    [RoleAuthorize("Student")]
    public class StudentController : Controller
    {
        private readonly AppDbContext _context;

        public StudentController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var student = await _context.Students
                .Include(s => s.User)
                .Include(s => s.Classroom).ThenInclude(c => c.Assignments)
                .Include(s => s.Submissions).ThenInclude(sub => sub.Assignment)
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null) return NotFound();

            var courseCount = await _context.CourseClassrooms
                .CountAsync(cc => cc.ClassroomId == student.ClassroomId);

            ViewBag.StudentName = $"{student.User.FirstName} {student.User.LastName}";
            ViewBag.ProfilePicture = student.User.ProfilePicturePath;
            ViewBag.CourseCount = courseCount;

            var allAssignments = student.Classroom.Assignments.ToList();
            var submittedIds = student.Submissions.Select(s => s.AssignmentId).ToHashSet();

            ViewBag.PendingCount = allAssignments.Count(a => !submittedIds.Contains(a.Id) && a.Deadline > DateTime.UtcNow);
            ViewBag.SubmittedCount = student.Submissions.Count;

            var gradedSubmissions = student.Submissions.Where(s => s.Score.HasValue).ToList();
            ViewBag.AverageGrade = gradedSubmissions.Any()
                ? Math.Round(gradedSubmissions.Average(s => (double)s.Score!.Value / s.Assignment.MaxScore * 100), 1)
                : 0;

            var now = DateTime.UtcNow;
            ViewBag.UpcomingDeadlines = allAssignments
                .Where(a => a.Deadline > now)
                .OrderBy(a => a.Deadline)
                .Take(5)
                .Select(a => new
                {
                    a.Title,
                    a.Deadline,
                    a.Id,
                    IsSubmitted = submittedIds.Contains(a.Id),
                    HoursLeft = (a.Deadline - now).TotalHours,
                    ClassroomName = student.Classroom.Name
                })
                .ToList();

            ViewBag.RecentGrades = student.Submissions
                .Where(s => s.Score.HasValue)
                .OrderByDescending(s => s.GradedAt)
                .Take(5)
                .ToList();

            return View();
        }
    }
}
