using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using System.Security.Claims;
using System.Text.RegularExpressions;
using TestChatSignalR.Contracts;
using TestChatSignalR.Interfaces;
using TestChatSignalR.Models;
using TestChatSignalR.Repositories;
using TestChatSignalR.Services;


namespace TestChatSignalR.Hubs
{
    public interface IChatClient
    {
        public Task ReceiveMessage(string message, int userId, int chatId);
        public Task ReceiveSystemMessage(string message, string chatId);
        public Task CreatedChat(int chatId);
    }
    public class ChatHub : Hub<IChatClient>
    {
        private readonly IMessagesRepository _messagesRepository;
        private readonly IChatsRepository _chatsRepository;
        private readonly IUsersRepository _usersRepository;
        private readonly IChatsService _chatsService;
        private static readonly List<UserConnection> ChatConnections = new();


        public ChatHub(IMessagesRepository messagesRepository, IChatsRepository chatsRepository, IUsersRepository usersRepository, IChatsService chatsService)
        {
            _messagesRepository = messagesRepository;
            _chatsRepository = chatsRepository;
            _usersRepository = usersRepository;
            _chatsService = chatsService;
        }

        //переопределяем метод, чтобы user автоматически подключался ко всем своим чатам при подключении к хабу
        public override async Task OnConnectedAsync()
        {
            try
            {
                var httpContext = Context.GetHttpContext();
                if (httpContext == null)
                {
                    Console.WriteLine("Ошибка: httpContext равен null");
                    await base.OnConnectedAsync();
                    return;
                }

                var userIdStr = httpContext.Request?.Query["userId"].ToString();

                Console.WriteLine("invoke OnConnectedAsync");
                Console.WriteLine($"User.Identity.Name: {userIdStr}");

                if (!string.IsNullOrEmpty(userIdStr))
                {
                    
                    int userId = Int32.Parse(userIdStr);

                    ChatConnections.Add(new UserConnection
                    {
                        UserId = userId,
                        ConnectionId = Context.ConnectionId
                    });

                    // Загружаем все чаты пользователя
                    var userChats = await _chatsRepository.GetByUserId(userId);

                    // Подключаем его во все группы
                    foreach (var chat in userChats)
                    {
                        string groupName = chat.Id.ToString();
                        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
                        Console.WriteLine($"Пользователь {userId} добавлен в группу {groupName}");
                    }
                }
                await base.OnConnectedAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка OnConnectedAsync: {ex.Message}");
            }
        }


        public async Task JoinChat(UserConnection userConnection)
        {
            Console.WriteLine($"JoinChat userCon: {userConnection.UserId} {userConnection.ChatId}");

            Chat currentChat = await _chatsRepository.GetByIdAsync(userConnection.ChatId);
            User currentUser = await _usersRepository.GetByIdAsync(userConnection.UserId);

            if (currentChat != null && currentUser != null )
            {
                // добавляем в comtext хаба данные о пользователе, чтобы переиспользовать их в других методах
                Context.Items["UserName"] = currentUser.UserName;
               // Context.Items["ChatName"] = currentChat.Name;
                Context.Items["Chatid"] = userConnection.ChatId;

               // Console.WriteLine($"chatName:{currentChat.Name} userName: {currentUser.UserName}");

                string groupName = userConnection.ChatId.ToString();

                //вывод сообщения о присоединении
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
                // Сообщаем всем участникам чата о присоединении
                await Clients.Group(groupName).ReceiveSystemMessage($"{currentUser.UserName} присоединился(ась) к чату", userConnection.ChatId.ToString());
            }
        }

        public async Task<int> SendMessage(UserConnection userConnection, int receiverId, string message)
        {
            Console.WriteLine($"sendMessage on: {userConnection.UserId} {userConnection.ChatId}");
            Console.WriteLine($"sendMessage on:" + message);

            // Если чат еще не создан, создаем его
            if (userConnection.ChatId == 0)
            {
                var chat = await _chatsService.CreateChat(new ChatRequest(userConnection.UserId, receiverId));
                userConnection.ChatId = chat.Id;

                string groupName = chat.Id.ToString();

                await Groups.AddToGroupAsync(Context.ConnectionId, chat.Id.ToString());

                // Находим подключение получателя, если он сейчас онлайн
                var connections = ChatConnections.Where(c => c.UserId == receiverId).Select(c => c.ConnectionId);
                foreach (var conn in connections)
                {
                    await Groups.AddToGroupAsync(conn, groupName);
                    Console.WriteLine($"conn found: {conn}");
                }
                 await Clients.Group(chat.Id.ToString()).CreatedChat(chat.Id);
                //await Task.Delay(100); //небольшая задержка, чтобы клиенты успели обработать создание чата и подключиться к группе
            }

            Message _message = new Message
            {
                UserId = userConnection.UserId,
                ChatId = userConnection.ChatId,
                Body = message
            };
            await _messagesRepository.CreateAsync(_message);

            await Clients.Group(userConnection.ChatId.ToString()).ReceiveMessage(message, userConnection.UserId, userConnection.ChatId);
            Console.WriteLine($"invoke ReceiveMessage " + userConnection.ChatId);

            return userConnection.ChatId;
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine("invoke OnDisconnected");

            var userName = Context.Items["UserName"]?.ToString();
            var chatId = Context.Items["Chatid"]?.ToString();

            Console.WriteLine($"userName: {userName} chatId: {chatId}");

            if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(chatId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId);
                await Clients.Group(chatId).ReceiveSystemMessage($"{userName} вышел(ла) из чата", chatId);

                // Удаляем соединение из списка активных соединений
                // ChatConnections.RemoveAll(c => c.ConnectionId == Context.ConnectionId);
                ChatConnections.RemoveAll(c =>
                {
                    if (c.ConnectionId == Context.ConnectionId)
                    {
                        Console.WriteLine($"conn remove: {c.ConnectionId}");
                        return true; // удалить
                    }
                    return false; // оставить
                });
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
