using TestChatSignalR.Models;

namespace TestChatSignalR.Interfaces
{
    public interface IUsersRepository
    {
        public Task CreateAsync(User user);
        public Task<User> GetByEmailAsync(string email);
        public Task<User> GetByIdAsync(int userId);
        public Task<User> GetByNameAsync(string userName);
    }
}
