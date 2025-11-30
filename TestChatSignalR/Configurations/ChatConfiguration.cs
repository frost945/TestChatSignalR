using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TestChatSignalR.Models;

namespace TestChatSignalR.Configurations
{
    public class ChatConfiguration : IEntityTypeConfiguration<Chat>
    {
        public void Configure(EntityTypeBuilder<Chat> builder)
        {
            builder
                .HasOne(u => u.User1)
                .WithMany(c => c.ChatsAsUser1)
                .HasForeignKey(u => u.User1Id)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(u => u.User2)
                .WithMany(c => c.ChatsAsUser2)
                .HasForeignKey(u => u.User2Id)
                .OnDelete(DeleteBehavior.Restrict);

            // Уникальный индекс для предотвращения дублирования чатов
            builder
                .HasIndex(c => new { c.User1Id, c.User2Id })
                .IsUnique();

            builder.HasData(
                new Chat(1, 1, 2),
                new Chat(2, 2, 3),
                new Chat(3, 1, 3)
            /* new Chat { Id = 1, User1Id=1, User2Id=2 },
             new Chat { Id = 2, User1Id=2, User2Id=3},
             new Chat { Id = 3, User1Id=1, User2Id=3 }*/
            );
        }
    }
}
