using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TestChatSignalR.Models;

namespace TestChatSignalR.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
           builder.HasData(
                new User { Id = 1, UserName = "Wera", PasswordHash = "1111", Email = "aaaa" },
                new User { Id = 2, UserName = "Alex", PasswordHash = "2222", Email = "bbbb" },
                new User { Id = 3, UserName = "Ben", PasswordHash = "3333", Email = "cccc" },
                new User { Id = 4, UserName = "Zero", PasswordHash = "4444", Email = "dddd" }
            );
        }
    }
}
