namespace ClassroomApp.Models.ViewModels.Message
{
    public class ConversationViewModel
    {
        public List<ConversationListItem> Conversations { get; set; } = new();
        public Guid? ActiveUserId { get; set; }
        public string? ActiveUserName { get; set; }
        public string? ActiveUserProfilePic { get; set; }
        public List<MessageItem> Messages { get; set; } = new();
    }

    public class ConversationListItem
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? ProfilePicturePath { get; set; }
        public string? LastMessage { get; set; }
        public DateTime? LastMessageTime { get; set; }
        public int UnreadCount { get; set; }
        public string Role { get; set; } = string.Empty;
    }

    public class MessageItem
    {
        public Guid Id { get; set; }
        public Guid SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
        public bool IsFromCurrentUser { get; set; }
    }
}
