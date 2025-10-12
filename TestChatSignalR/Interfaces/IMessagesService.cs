using TestChatSignalR.Models;

namespace TestChatSignalR.Interfaces
{
    public interface IMessagesService
    {
        public Task <IEnumerable<Message>> LoadHistoryByChatIdAsync(int chatId, int skip);
    }
}
