using TestChatSignalR.Interfaces;
using TestChatSignalR.Models;

namespace TestChatSignalR.Services
{
    public class MessagesService : IMessagesService
    {
        private readonly IMessagesRepository _messagesRepository;
        public MessagesService(IMessagesRepository messagesRepository)
        {
            _messagesRepository = messagesRepository;
        }
        public async Task <IEnumerable<Message>> LoadHistoryByChatIdAsync(int chatId, int skip)
        {
            int countMessages = 20; // Количество сообщений для загрузки
            
            Console.WriteLine("loadChatHistory on");
            Console.WriteLine($"skip: {skip}");
            
            List<Message> messages = await _messagesRepository.GetByChatIdAsync(chatId, countMessages, skip);

            //если проверка истинна, то досрочно завершаем метод, чтобы лишний раз не отправлять сообщения на клиент
            if (messages == null || messages.Count == 0)
            {
                Console.WriteLine("No more messages to load.");
                return Enumerable.Empty<Message>();
            }

            Console.WriteLine($"Sending {messages.Count} messages to client...");

            return messages;
        }
    }
}
