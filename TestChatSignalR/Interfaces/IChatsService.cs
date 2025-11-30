using TestChatSignalR.Contracts;
using TestChatSignalR.Models;

namespace TestChatSignalR.Interfaces
{
    public interface IChatsService
    {
        public Task<IEnumerable<Chat>> GetChatsByUserId(int userId);
        public Task<Chat> GetChatByIdAsync(int chatId);
       // public Task<Chat> GetChatByNameAsync(string chatName);
        public Task<Chat> CreateChat(ChatRequest request);
    }
}
