namespace TestChatSignalR.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public string? userName { get; set; }
        public string? chatName { get; set; }
        public string? message { get; set; }
        public DateTime sentAt { get; set; } = DateTime.Now;
    }
}
