using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ClassroomApp.Data;
using ClassroomApp.Models.Entities;
using System.Security.Claims;

namespace ClassroomApp.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly AppDbContext _context;

        public ChatHub(AppDbContext context)
        {
            _context = context;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"chat_{userId}");
            }
            await base.OnConnectedAsync();
        }

        public async Task SendMessage(string receiverId, string content)
        {
            var senderIdStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(senderIdStr)) return;

            var senderId = Guid.Parse(senderIdStr);
            var receiverGuid = Guid.Parse(receiverId);

            var message = new Message
            {
                Id = Guid.NewGuid(),
                SenderId = senderId,
                ReceiverId = receiverGuid,
                Content = content,
                SentAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            var senderName = $"{Context.User?.FindFirst(ClaimTypes.GivenName)?.Value} {Context.User?.FindFirst(ClaimTypes.Surname)?.Value}";

            var messageData = new
            {
                message.Id,
                message.SenderId,
                SenderName = senderName,
                message.Content,
                message.SentAt,
                message.IsRead
            };

            await Clients.Group($"chat_{receiverId}").SendAsync("ReceiveMessage", messageData);
            await Clients.Caller.SendAsync("MessageSent", messageData);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"chat_{userId}");
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
