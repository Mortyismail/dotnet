namespace WebApplication1.Models.ViewModels
{
    public class ConversationViewModel
    {
        public string OtherUserId { get; set; } = string.Empty;
        public string OtherUserEmail { get; set; } = string.Empty;
        public string LastMessage { get; set; } = string.Empty;
        public DateTime LastMessageDate { get; set; }
        public int UnreadCount { get; set; }
    }
}