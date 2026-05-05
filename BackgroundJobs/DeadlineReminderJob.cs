using ClassroomApp.Data;
using ClassroomApp.Models.Enums;
using ClassroomApp.Services;
using Microsoft.EntityFrameworkCore;

namespace ClassroomApp.BackgroundJobs
{
    public class DeadlineReminderJob
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;

        public DeadlineReminderJob(AppDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task ExecuteAsync()
        {
            var now = DateTime.UtcNow;
            var deadline = now.AddHours(24);

            var upcomingAssignments = await _context.Assignments
                .Include(a => a.Classroom)
                    .ThenInclude(c => c.Students)
                .Include(a => a.Submissions)
                .Where(a => a.Deadline > now && a.Deadline <= deadline)
                .ToListAsync();

            foreach (var assignment in upcomingAssignments)
            {
                var submittedStudentIds = assignment.Submissions
                    .Select(s => s.StudentId)
                    .ToHashSet();

                foreach (var student in assignment.Classroom.Students)
                {
                    if (!submittedStudentIds.Contains(student.Id))
                    {
                        await _notificationService.CreateNotificationAsync(
                            student.UserId,
                            "? Date limite dans moins de 24h !",
                            $"Le devoir '{assignment.Title}' doit être rendu dans moins de 24 heures !",
                            NotificationType.DeadlineReminder,
                            assignment.Id,
                            "Assignment"
                        );
                    }
                }
            }
        }
    }
}
