namespace TestChatSignalR.Models
{
    public class Chat
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsGroup { get; set; } = false; //true = группа, false = диалог
        public List<User> Users { get; set; } = new List<User>();
        public Chat() { }

        public Chat(string name)
        {
            Name = name;
            //IsGroup = isGroup;
        }
    }
}
