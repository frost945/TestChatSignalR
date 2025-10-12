using TestChatSignalR.Models;

namespace TestChatSignalR.Interfaces
{
    public interface IChatsRepository
    {
        public Task<IEnumerable<Chat>> GetByUserId(int userId);
        public Task<Chat> GetByIdAsync(int chatId);
        public Task<Chat> GetByNameAsync(string chatName);
        public Task Add(Chat chat);
    }
}
