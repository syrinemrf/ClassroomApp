using ClassroomApp.Data;
using ClassroomApp.Filters;
using ClassroomApp.Models.Entities;
using ClassroomApp.Models.ViewModels.Calendar;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ClassroomApp.Controllers
{
    [RoleAuthorize("Admin", "Teacher", "Student")]
    public class CalendarController : Controller
    {
        private readonly AppDbContext _context;

        public CalendarController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetEvents(DateTime? start, DateTime? end)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = User.FindFirst(ClaimTypes.Role)!.Value;

            var events = new List<CalendarEventViewModel>();
            var classroomIds = new List<Guid>();

            if (role == "Student")
            {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
                if (student != null) classroomIds.Add(student.ClassroomId);
            }
            else if (role == "Teacher")
            {
                var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);
                if (teacher != null)
                {
                    classroomIds = await _context.CourseClassrooms
                        .Where(cc => cc.Course.TeacherId == teacher.Id)
                        .Select(cc => cc.ClassroomId)
                        .Distinct()
                        .ToListAsync();
                }
            }
            else
            {
                classroomIds = await _context.Classrooms.Select(c => c.Id).ToListAsync();
            }

            // Assignment deadlines
            var assignments = await _context.Assignments
                .Where(a => classroomIds.Contains(a.ClassroomId))
                .Select(a => new CalendarEventViewModel
                {
                    Id = a.Id,
                    Title = $"?? {a.Title}",
                    Description = $"Assignment deadline - Max score: {a.MaxScore}",
                    Start = a.Deadline.AddHours(-1),
                    End = a.Deadline,
                    Color = "#EF4444",
                    EventType = "assignment"
                })
                .ToListAsync();
            events.AddRange(assignments);

            // Calendar events
            var calendarEvents = await _context.CalendarEvents
                .Where(e => e.ClassroomId == null || classroomIds.Contains(e.ClassroomId.Value))
                .Select(e => new CalendarEventViewModel
                {
                    Id = e.Id,
                    Title = e.Title,
                    Description = e.Description,
                    Start = e.StartDate,
                    End = e.EndDate,
                    Color = e.CreatedByUserId == userId ? "#10B981" : (e.Color ?? "#4F46E5"),
                    EventType = "event"
                })
                .ToListAsync();
            events.AddRange(calendarEvents);

            return Json(events);
        }

        [RoleAuthorize("Teacher")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvent([FromBody] CalendarEventViewModel model)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var calEvent = new CalendarEvent
            {
                Id = Guid.NewGuid(),
                Title = model.Title,
                Description = model.Description,
                StartDate = model.Start,
                EndDate = model.End,
                Color = model.Color ?? "#4F46E5",
                CreatedByUserId = userId,
                ClassroomId = model.ClassroomId
            };

            _context.CalendarEvents.Add(calEvent);
            await _context.SaveChangesAsync();

            return Json(new { success = true, id = calEvent.Id });
        }

        [RoleAuthorize("Teacher", "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEvent(Guid id)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = User.FindFirst(ClaimTypes.Role)!.Value;

            var calEvent = await _context.CalendarEvents.FindAsync(id);
            if (calEvent == null) return NotFound();

            if (role != "Admin" && calEvent.CreatedByUserId != userId)
                return Forbid();

            _context.CalendarEvents.Remove(calEvent);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
    }
}
