namespace TestChatSignalR.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }= string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
        public List<Chat> Chats { get; set; } = new List<Chat>();

        public User() { }
        public User( string name, string email, string passwordHash)
        {   
            Name = name;
            Email = email;
            PasswordHash = passwordHash;
        }
        public static User Create(int id, string userName, string email, string passwordHash)
        {
            return new User( userName, email, passwordHash);
        }
    }
}
