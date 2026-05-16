using ClassroomApp.Data;
using ClassroomApp.Filters;
using ClassroomApp.Models.Entities;
using ClassroomApp.Models.Enums;
using ClassroomApp.Models.ViewModels.Message;
using ClassroomApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ClassroomApp.Controllers
{
    [RoleAuthorize("Admin", "Teacher", "Student")]
    public class MessageController : Controller
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;

        public MessageController(AppDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Index(Guid? userId = null)
        {
            var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // Get all conversations
            var allMessages = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m => m.SenderId == currentUserId || m.ReceiverId == currentUserId)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();

            var conversationUsers = allMessages
                .Select(m => m.SenderId == currentUserId ? m.Receiver : m.Sender)
                .DistinctBy(u => u.Id)
                .ToList();

            var conversations = conversationUsers.Select(u =>
            {
                var lastMsg = allMessages
                    .Where(m => (m.SenderId == u.Id && m.ReceiverId == currentUserId) ||
                                (m.SenderId == currentUserId && m.ReceiverId == u.Id))
                    .OrderByDescending(m => m.SentAt)
                    .FirstOrDefault();

                return new ConversationListItem
                {
                    UserId = u.Id,
                    UserName = $"{u.FirstName} {u.LastName}",
                    ProfilePicturePath = u.ProfilePicturePath,
                    LastMessage = lastMsg?.Content,
                    LastMessageTime = lastMsg?.SentAt,
                    UnreadCount = allMessages.Count(m => m.SenderId == u.Id && m.ReceiverId == currentUserId && !m.IsRead),
                    Role = u.Role.ToString()
                };
            }).OrderByDescending(c => c.LastMessageTime).ToList();

            // Add users not yet in conversations (for new messages)
            if (!conversations.Any() || userId.HasValue)
            {
                var allUsers = await _context.Users
                    .Where(u => u.Id != currentUserId && u.IsActive)
                    .OrderBy(u => u.LastName)
                    .ToListAsync();

                var existingIds = conversations.Select(c => c.UserId).ToHashSet();
                foreach (var user in allUsers.Where(u => !existingIds.Contains(u.Id)))
                {
                    conversations.Add(new ConversationListItem
                    {
                        UserId = user.Id,
                        UserName = $"{user.FirstName} {user.LastName}",
                        ProfilePicturePath = user.ProfilePicturePath,
                        Role = user.Role.ToString()
                    });
                }
            }

            var vm = new ConversationViewModel
            {
                Conversations = conversations
            };

            // Load active conversation messages
            if (userId.HasValue)
            {
                var activeUser = await _context.Users.FindAsync(userId.Value);
                if (activeUser != null)
                {
                    vm.ActiveUserId = userId.Value;
                    vm.ActiveUserName = $"{activeUser.FirstName} {activeUser.LastName}";
                    vm.ActiveUserProfilePic = activeUser.ProfilePicturePath;

                    vm.Messages = await _context.Messages
                        .Where(m => (m.SenderId == currentUserId && m.ReceiverId == userId.Value) ||
                                    (m.SenderId == userId.Value && m.ReceiverId == currentUserId))
                        .OrderBy(m => m.SentAt)
                        .Select(m => new MessageItem
                        {
                            Id = m.Id,
                            SenderId = m.SenderId,
                            SenderName = m.Sender.FirstName + " " + m.Sender.LastName,
                            Content = m.Content,
                            SentAt = m.SentAt,
                            IsRead = m.IsRead,
                            IsFromCurrentUser = m.SenderId == currentUserId
                        })
                        .ToListAsync();

                    // Mark messages as read
                    var unread = await _context.Messages
                        .Where(m => m.SenderId == userId.Value && m.ReceiverId == currentUserId && !m.IsRead)
                        .ToListAsync();
                    foreach (var msg in unread)
                        msg.IsRead = true;
                    await _context.SaveChangesAsync();
                }
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(Guid receiverId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "Message cannot be empty.";
                return RedirectToAction(nameof(Index), new { userId = receiverId });
            }

            var senderId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var senderName = $"{User.FindFirst(ClaimTypes.GivenName)?.Value} {User.FindFirst(ClaimTypes.Surname)?.Value}";

            var message = new Message
            {
                Id = Guid.NewGuid(),
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = content,
                SentAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            await _notificationService.CreateNotificationAsync(
                receiverId,
                "Nouveau message",
                $"{senderName} vous a envoyé un message.",
                NotificationType.NewMessage
            );

            return RedirectToAction(nameof(Index), new { userId = receiverId });
        }
    }
}
