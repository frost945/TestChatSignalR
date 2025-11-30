namespace TestChatSignalR.Models
{
    /*public class Message
    {
        public int Id { get; set; }
        public string Body { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int UserId { get; set; } // Foreign key to User
        public User? User { get; set; } // Navigation property
        public int ChatId { get; set; } // Foreign key to Chat
        public Chat? Chat { get; set; } // Navigation property

        public Message() { }

        public Message(string body, int userId, int chatId)
        {
            Body = body;
            UserId = userId;
            ChatId = chatId;
        }
    }*/

    public class Message
    {
        public int Id { get;  set; }
        public string Body { get;  set; } = string.Empty;
        public int UserId { get;  set; } // Foreign key to User
        public User? User { get; set; } // Navigation property
        public int ChatId { get; set; } // Foreign key to Chat
        public Chat? Chat { get; set; } // Navigation property
        public DateTime CreatedAt { get; private set; } = DateTime.Now;

        public Message() { }

        public Message(string body, int userId, int chatId)
        {
            Body = body;
            UserId = userId;
            ChatId = chatId;
        }
    }
}
