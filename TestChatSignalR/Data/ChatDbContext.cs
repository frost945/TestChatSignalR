using Microsoft.EntityFrameworkCore;
using TestChatSignalR.Configurations;
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
            optionsBuilder.LogTo(log => FormatEfLog(log), new[] { DbLoggerCategory.Database.Command.Name }, LogLevel.Information).EnableSensitiveDataLogging();//чтобы видеть sql-запросы в консоли
        } //log=>FormatEfLog(log)

        private static void FormatEfLog(string log)
        {
            // Фильтруем только команды SQL, без мусора
            if (log.Contains("CommandExecuting"))
                return;

            // Добавляем разделитель между запросами
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n═══════════════════════════════════════════════════════════════════════");
            Console.ForegroundColor = ConsoleColor.Yellow;

            // Подсветка ключевых слов SQL
            string formatted = log
                .Replace("SELECT", "\nSELECT")
                .Replace("FROM", "\nFROM")
                .Replace("WHERE", "\nWHERE")
                .Replace("INNER JOIN", "\nINNER JOIN")
                .Replace("LEFT JOIN", "\nLEFT JOIN")
                .Replace("VALUES", "\nVALUES")
                .Replace("INSERT INTO", "\nINSERT INTO")
                .Replace("UPDATE", "\nUPDATE")
                .Replace("DELETE", "\nDELETE")
                .Replace("Executed DbCommand", "\n Executed DbCommand");

            Console.WriteLine(formatted);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("═══════════════════════════════════════════════════════════════════════\n");
            Console.ResetColor();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ChatConfiguration());
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new MessageConfiguration());
        }
    }
}
