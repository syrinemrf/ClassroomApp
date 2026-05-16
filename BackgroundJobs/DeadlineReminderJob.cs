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
        private readonly IEmailService _emailService;

        public DeadlineReminderJob(
            AppDbContext context,
            INotificationService notificationService,
            IEmailService emailService)
        {
            _context = context;
            _notificationService = notificationService;
            _emailService = emailService;
        }

        public async Task ExecuteAsync()
        {
            var now = DateTime.UtcNow;
            var windowEnd = now.AddHours(24);

            var upcomingAssignments = await _context.Assignments
                .Include(a => a.Classroom)
                    .ThenInclude(c => c.Students)
                        .ThenInclude(s => s.User)
                .Include(a => a.Submissions)
                .Where(a => a.Deadline > now && a.Deadline <= windowEnd)
                .ToListAsync();

            foreach (var assignment in upcomingAssignments)
            {
                var submittedStudentIds = assignment.Submissions
                    .Select(s => s.StudentId)
                    .ToHashSet();

                // Fetch students who already received a reminder for this assignment
                var alreadyRemindedUserIds = (await _context.Notifications
                    .Where(n => n.RelatedEntityId == assignment.Id
                                && n.Type == NotificationType.DeadlineReminder)
                    .Select(n => n.UserId)
                    .ToListAsync())
                    .ToHashSet();

                var hoursLeft = (assignment.Deadline - now).TotalHours;

                foreach (var student in assignment.Classroom.Students)
                {
                    if (submittedStudentIds.Contains(student.Id)) continue;
                    if (alreadyRemindedUserIds.Contains(student.UserId)) continue;

                    try
                    {
                        await _notificationService.CreateNotificationAsync(
                            student.UserId,
                            "Rappel : date limite dans moins de 24h !",
                            $"Le devoir '{assignment.Title}' doit etre rendu dans moins de 24 heures !",
                            NotificationType.DeadlineReminder,
                            assignment.Id,
                            "Assignment"
                        );
                    }
                    catch { }

                    try
                    {
                        var body = BuildReminderEmail(
                            student.User.FirstName,
                            assignment.Title,
                            assignment.Deadline,
                            hoursLeft
                        );
                        await _emailService.SendAsync(
                            student.User.Email,
                            student.User.FirstName + " " + student.User.LastName,
                            $"[ClassroomApp] Rappel : devoir '{assignment.Title}' dans moins de 24h",
                            body
                        );
                    }
                    catch { }
                }
            }
        }

        private static string BuildReminderEmail(
            string studentFirstName,
            string assignmentTitle,
            DateTime deadline,
            double hoursLeft)
        {
            return $"""
                <!DOCTYPE html>
                <html>
                <body style="font-family:Arial,sans-serif;color:#222;max-width:600px;margin:auto;">
                  <div style="background:#dc2626;padding:24px;border-radius:8px 8px 0 0;">
                    <h2 style="color:#fff;margin:0;">&#9200; Rappel de date limite</h2>
                  </div>
                  <div style="padding:24px;border:1px solid #e5e7eb;border-top:0;border-radius:0 0 8px 8px;">
                    <p>Bonjour <strong>{System.Net.WebUtility.HtmlEncode(studentFirstName)}</strong>,</p>
                    <p>Ce rappel vous informe que la date limite du devoir suivant approche :</p>
                    <div style="background:#fef2f2;border-left:4px solid #dc2626;padding:16px;border-radius:4px;margin:16px 0;">
                      <p style="margin:0;font-size:18px;font-weight:bold;">{System.Net.WebUtility.HtmlEncode(assignmentTitle)}</p>
                      <p style="margin:8px 0 0;color:#dc2626;">Date limite : {deadline:dd/MM/yyyy HH:mm} UTC</p>
                      <p style="margin:4px 0 0;color:#6b7280;font-size:13px;">Il vous reste environ {hoursLeft:F0} heure(s).</p>
                    </div>
                    <p>Connectez-vous a ClassroomApp pour soumettre votre travail avant qu'il ne soit trop tard.</p>
                    <p style="color:#6b7280;font-size:13px;margin-top:24px;">Vous recevez cet email car vous avez un devoir non soumis.</p>
                  </div>
                </body>
                </html>
                """;
        }
    }
}