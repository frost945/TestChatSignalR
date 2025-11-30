namespace TestChatSignalR.Models
{
   /* public class Chat
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<User> Users { get; set; } = new List<User>();
        public Chat() { }

        public Chat(string name)
        {
            Name = name;
        }
    }*/

    public class Chat
    {
        public int Id { get;  set; }
        public int User1Id { get;  set; } 
        public int User2Id { get;  set; }
        public DateTime CreatedAt { get; private set; } = DateTime.Now;
        public User User1 { get; private set; }= null!;// Navigation property
        public User User2 { get; private set; }= null!;// Navigation property
        public List<Message>Messages { get; private set; }= new List<Message>(); 

        private Chat() { }

        public Chat(int user1Id, int user2Id)
        {
            Console.WriteLine($"конструктор чата между пользователями {user1Id} и {user2Id}");
            if (user1Id == user2Id)
            {
                Console.WriteLine("==> ИДЕНТИЧНЫ! Исключение!");
                throw new ArgumentException("A user cannot chat with themselves.");
            }
            User1Id = user1Id;
            User2Id = user2Id;
        }

        public Chat(int id, int user1Id, int user2Id)
        {   Id = id;
            User1Id = user1Id;
            User2Id = user2Id;
        }

        // Метод для получения названия чата в зависимости от текущего пользователя
        public string GetDisplayName(int currentUserId)
        {
            if (currentUserId == User1Id)
            {
                return User2.UserName;
            }
            else if (currentUserId == User2Id)
                return User1.UserName; // Название = имя собеседника
            else
                throw new UnauthorizedAccessException("User is not a participant of this chat");
        }
    }
}
