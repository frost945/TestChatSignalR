using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TestChatSignalR.Models;

namespace TestChatSignalR.Configurations
{
    public class MessageConfiguration : IEntityTypeConfiguration<Message>
    {
        public void Configure(EntityTypeBuilder<Message> builder)
        {
            builder.HasData(
                new Message { Id = 1, ChatId = 1, UserId = 1, Body = "Hi, Alex!" },
                new Message { Id = 2, ChatId = 1, UserId = 2, Body = "Hi, Wera!" },
                new Message { Id = 3, ChatId = 2, UserId = 2, Body = "Hello в группу!" },
                new Message { Id = 4, ChatId = 2, UserId = 3, Body = "Hello, everyone!" },
                new Message { Id = 5, ChatId = 2, UserId = 2, Body = "как дела?" },
                new Message { Id = 6, ChatId = 3, UserId = 3, Body = "как дела,говорит ben!" },
                new Message { Id = 7, ChatId = 3, UserId = 1, Body = "норм ответ веры)" }
            );
        }
    }
}
