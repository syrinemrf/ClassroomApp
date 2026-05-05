using ClassroomApp.Data;
using ClassroomApp.Filters;
using ClassroomApp.Models.Entities;
using ClassroomApp.Models.Enums;
using ClassroomApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ClassroomApp.Controllers
{
    [RoleAuthorize("Teacher", "Student")]
    public class CommentController : Controller
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;

        public CommentController(AppDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Guid assignmentId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "Comment cannot be empty.";
                return RedirectBack(assignmentId);
            }

            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                Content = content,
                AssignmentId = assignmentId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Comment posted.";
            return RedirectBack(assignmentId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reply(Guid parentCommentId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "Reply cannot be empty.";
                return Redirect(Request.Headers.Referer.ToString());
            }

            var parent = await _context.Comments.FindAsync(parentCommentId);
            if (parent == null) return NotFound();

            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var reply = new Comment
            {
                Id = Guid.NewGuid(),
                Content = content,
                AssignmentId = parent.AssignmentId,
                UserId = userId,
                ParentCommentId = parentCommentId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(reply);
            await _context.SaveChangesAsync();

            // Notify the parent comment author
            if (parent.UserId != userId)
            {
                var replierName = $"{User.FindFirst(ClaimTypes.GivenName)?.Value} {User.FindFirst(ClaimTypes.Surname)?.Value}";
                await _notificationService.CreateNotificationAsync(
                    parent.UserId,
                    "?? New Reply",
                    $"{replierName} replied to your comment.",
                    NotificationType.NewComment,
                    parent.AssignmentId,
                    "Assignment"
                );
            }

            TempData["Success"] = "Reply posted.";
            return RedirectBack(parent.AssignmentId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (comment == null)
            {
                TempData["Error"] = "Comment not found or you don't have permission.";
                return Redirect(Request.Headers.Referer.ToString());
            }

            var assignmentId = comment.AssignmentId;
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Comment deleted.";
            return RedirectBack(assignmentId);
        }

        private IActionResult RedirectBack(Guid assignmentId)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role == "Teacher")
                return RedirectToAction("Details", "Assignment", new { id = assignmentId });
            return RedirectToAction("StudentDetails", "Assignment", new { id = assignmentId });
        }
    }
}
