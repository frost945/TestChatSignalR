using System.ComponentModel.DataAnnotations.Schema;

namespace TestChatSignalR.Models
{
   /* public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; }= string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
        public List<Chat> Chats { get; set; } = new List<Chat>();

        public User() { }
        public User( string name, string email, string passwordHash)
        {   
            UserName = name;
            Email = email;
            PasswordHash = passwordHash;
        }
        public static User Create(int id, string userName, string email, string passwordHash)
        {
            return new User( userName, email, passwordHash);
        }
    }*/

    public class User
    {
        public int Id { get;  set; }
        public string UserName { get;  set; } = string.Empty;
        public string Email { get;  set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; private set; }= DateTime.Now;

        [NotMapped]
        public List<Chat> MyChats => ChatsAsUser1.Concat(ChatsAsUser2).ToList();

        // Разделяем навигационные свойства
        public List<Chat> ChatsAsUser1 { get; set; } = new List<Chat>();
        public List<Chat> ChatsAsUser2 { get; set; } = new List<Chat>();

        // public List<User> Friends { get; private set; } = new List<User>();// список друзей - для будущего

        public User() { }
        public User(string userName, string email, string passwordHash)
        {
            UserName = userName;
            Email = email.ToLowerInvariant().Trim();
            PasswordHash = passwordHash;
        }
        public static User Create(int id, string userName, string email, string passwordHash)
        {
            return new User(userName, email, passwordHash);
        }
    }
}
