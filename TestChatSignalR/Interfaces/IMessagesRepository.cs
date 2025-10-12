using TestChatSignalR.Models;

namespace TestChatSignalR.Interfaces
{
    public interface IMessagesRepository
    {
        public Task<List<Message>> GetByChatIdAsync(int chatId, int count, int skip);
        public Task CreateAsync(Message message);
    }
}
