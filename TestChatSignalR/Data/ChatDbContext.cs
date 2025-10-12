using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;
using TestChatSignalR.Models;

namespace TestChatSignalR.Data
{
    public class ChatDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<Message> Messages { get; set; }

        public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.LogTo(Console.WriteLine, new[] { DbLoggerCategory.Database.Command.Name }, LogLevel.Information).EnableSensitiveDataLogging();//чтобы видеть sql-запросы в консоли
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasMany(u => u.Chats)
                .WithMany(c => c.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "JoinUserChat",
                    j => j.HasOne<Chat>()
                        .WithMany()
                        .HasForeignKey("ChatId")
                        .HasConstraintName("FK_JoinUserChat_Chat"),
                    j => j.HasOne<User>()
                        .WithMany()
                        .HasForeignKey("UserId")
                        .HasConstraintName("FK_JoinUserChat_User"),
                    j =>
                    {
                        j.HasKey("UserId", "ChatId");
                        j.ToTable("JoinUserChat");
                        j.HasData(
                            new { UserId = 1, ChatId = 1 },
                            new { UserId = 2, ChatId = 1 },
                            new { UserId = 1, ChatId = 2 },
                            new { UserId = 2, ChatId = 2 },
                            new { UserId = 3, ChatId = 2 },
                            new { UserId = 2, ChatId = 3 },
                            new { UserId = 3, ChatId = 3 }
                            );
                    });

            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Name = "Wera", PasswordHash = "1111", Email = "aaaa"},
                new User { Id = 2, Name = "Alex", PasswordHash = "2222", Email = "bbbb" },
                new User { Id = 3, Name = "Ben", PasswordHash = "3333", Email = "cccc" }
            );
            modelBuilder.Entity<Chat>().HasData(
                new Chat { Id = 1, Name = "Wera-Alex", IsGroup = false },
                new Chat { Id = 2, Name = "Group3", IsGroup = true},
                new Chat { Id = 3, Name = "чатик", IsGroup = false }
            );
            modelBuilder.Entity<Message>().HasData(
                new Message { Id = 1, ChatId = 1, UserId = 1, Body = "Hi, Alex!", CreatedAt = DateTime.Now },
                new Message { Id = 2, ChatId = 1, UserId = 2, Body = "Hi, Wera!", CreatedAt = DateTime.Now },
                new Message { Id = 3, ChatId = 2, UserId = 1, Body = "Hello в группу!", CreatedAt = DateTime.Now },
                new Message { Id = 4, ChatId = 2, UserId = 2, Body = "Hello, everyone!", CreatedAt = DateTime.Now },
                new Message { Id = 5, ChatId = 2, UserId = 3, Body = "Hello, от Bena!", CreatedAt = DateTime.Now },
                new Message { Id = 6, ChatId = 3, UserId = 2, Body = "как дела,говорит alex!", CreatedAt = DateTime.Now },
                new Message { Id = 7, ChatId = 3, UserId = 3, Body = "норм, отвечает бен-ладен)", CreatedAt = DateTime.Now }
            );
        }


    }
}
