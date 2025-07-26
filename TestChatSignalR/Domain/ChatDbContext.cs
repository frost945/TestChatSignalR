using Microsoft.EntityFrameworkCore;
using TestChatSignalR.Models;

namespace TestChatSignalR.Domain
{
    public class ChatDbContext: DbContext
    {
        public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options) { }
        public DbSet<ChatMessage> chatMessages { get; set; }
    }
}
