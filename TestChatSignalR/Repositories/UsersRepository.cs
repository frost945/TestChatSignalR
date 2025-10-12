using Microsoft.EntityFrameworkCore;
using TestChatSignalR.Data;
using TestChatSignalR.Interfaces;
using TestChatSignalR.Models;

namespace TestChatSignalR.Repositories
{
    public class UsersRepository : IUsersRepository
    {
        private readonly ChatDbContext _dbContext;
        public UsersRepository(ChatDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task CreateAsync(User user)
        {
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();
        }
        public async Task<User> GetByEmailAsync(string email)
        {
            User user = await _dbContext.Users
                .FirstAsync(u=> u.Email==email)?? throw new InvalidOperationException($"User with email {email} not found.");
            return user;
        }
        public async Task<User> GetByIdAsync(int userId)
        {
            User user = await _dbContext.Users
                .FirstAsync(u => u.Id == userId) ?? throw new InvalidOperationException($"User with id {userId} not found.");
            return user;
        }
    }
}
