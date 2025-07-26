using Microsoft.AspNetCore.SignalR;
using TestChatSignalR.Domain;
using TestChatSignalR.Models;


namespace TestChatSignalR.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ChatDbContext db;
        private readonly ChatRepository chatRepository;

        public ChatHub(ChatDbContext Db, ChatRepository ChatRepository)
        {
            db = Db;
            chatRepository = ChatRepository;
        }

        public async Task joinChat(UserConnection userConnection)
        {
            Console.WriteLine("joinChat on");
            Console.WriteLine($"{userConnection.userName} {userConnection.chatName}");

            //вывод истории чата
            await LoadChatHistory(userConnection.chatName);

            //вывод сообщения о присоединении
            await Groups.AddToGroupAsync(Context.ConnectionId, userConnection.chatName);
            await Clients.Group(userConnection.chatName).SendAsync("receiveMessage", "admin", userConnection.chatName, $"{userConnection.userName} присоединился к чату");
        }

        public async Task sendMessage(UserConnection userConnection, string Message)
        {
            Console.WriteLine("sendMessage on");
            Console.WriteLine($"{userConnection.userName} {userConnection.chatName} {Message}");

            ChatMessage chatMessage = new ChatMessage
            {
                userName = userConnection.userName,
                chatName = userConnection.chatName,
                message = Message
            };

            db.chatMessages.Add(chatMessage);// добавляем в БД сообщение
            await db.SaveChangesAsync();

            await Clients.Group(userConnection.chatName).SendAsync("receiveMessage", userConnection.userName, userConnection.chatName, Message);
        }

        public async Task LoadChatHistory(string chat, int skip = 0)
        {
            Console.WriteLine("loadChatHistory on");
            Console.WriteLine($"skip: {skip}");
            int countMessages = 20; // Количество сообщений для загрузки
            List<ChatMessage> messages = await chatRepository.GetMessagesAsync(chat, countMessages, skip);

            //если проверка истинна, то досрочно завершаем метод, чтобы лишний раз не отправлять сообщения на клиент
            if (messages == null || messages.Count == 0)
            {
                Console.WriteLine("No more messages to load.");
                return;
            }

            // Отправляем клиенту список сообщений
            Console.WriteLine($"Sending {messages.Count} messages to client...");

            await Clients.Caller.SendAsync("receiveHistory",  messages);
        }
    }
}
