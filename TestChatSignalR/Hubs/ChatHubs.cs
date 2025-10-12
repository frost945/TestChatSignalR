using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using System.Security.Claims;
using System.Text.RegularExpressions;
using TestChatSignalR.Contracts;
using TestChatSignalR.Interfaces;
using TestChatSignalR.Models;
using TestChatSignalR.Repositories;


namespace TestChatSignalR.Hubs
{
    public interface IChatClient
    {
        public Task ReceiveMessage(string userName, string message, int userId, int chatId);
        public Task ReceiveSystemMessage(string message, string chatId);
    }
    public class ChatHub : Hub<IChatClient>
    {
        private readonly IMessagesRepository _messagesRepository;
        private readonly IChatsRepository _chatsRepository;
        private readonly IUsersRepository _usersRepository;

        public ChatHub(IMessagesRepository messagesRepository, IChatsRepository chatsRepository, IUsersRepository usersRepository)
        {
            _messagesRepository = messagesRepository;
            _chatsRepository = chatsRepository;
            _usersRepository = usersRepository;
        }

        //переопределяем метод, чтобы user автоматически подключался ко всем своим чатам при подключении к хабу
        public override async Task OnConnectedAsync()
        {
            try
            {
                var httpContext = Context.GetHttpContext();
                var userIdStr = httpContext.Request.Query["userId"];
                Console.WriteLine("invoke OnConnectedAsync");
                Console.WriteLine($"User.Identity.Name: {userIdStr}");

                if (!string.IsNullOrEmpty(userIdStr))
                {
                    
                    int userId = Int32.Parse(userIdStr);

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
            Console.WriteLine($"userCon: {userConnection.UserId} {userConnection.ChatId}");

            Chat currentChat = await _chatsRepository.GetByIdAsync(userConnection.ChatId);
            User currentUser = await _usersRepository.GetByIdAsync(userConnection.UserId);

            if (currentChat != null && currentUser != null )
            {
                // добавляем в comtext хаба данные о пользователе, чтобы переиспользовать их в других методах
                Context.Items["UserName"] = currentUser.Name;
                Context.Items["ChatName"] = currentChat.Name;
                Context.Items["Chatid"] = userConnection.ChatId;

                Console.WriteLine($"joinChat on chatName:{currentChat.Name} userName: {currentUser.Name}");

                string groupName = userConnection.ChatId.ToString();

                //вывод сообщения о присоединении
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
                // Сообщаем всем участникам чата о присоединении
                await Clients.Group(groupName).ReceiveSystemMessage($"{currentUser.Name} присоединился(ась) к чату", userConnection.ChatId.ToString());
            }
        }

        public async Task SendMessage(UserConnection userConnection, string message)
        {
            Console.WriteLine($"sendMessage on: {userConnection.UserId} {userConnection.ChatId}");
            Console.WriteLine($"sendMessage on:" + message);

            
            var userName = Context.Items["UserName"]?.ToString();
            var chatName = Context.Items["ChatName"]?.ToString();

            Message _message = new Message
            {
                UserId = userConnection.UserId,
                ChatId = userConnection.ChatId,
                Body = message
            };
            await _messagesRepository.CreateAsync(_message);

            if (!string.IsNullOrEmpty(chatName) && !string.IsNullOrEmpty(userName))
                await Clients.Group(userConnection.ChatId.ToString()).ReceiveMessage(userName, message, userConnection.UserId, userConnection.ChatId);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine("invoke OnDisconnected");

            var userName = Context.Items["UserName"]?.ToString();
            var chatName = Context.Items["ChatName"]?.ToString();
            var chatId = Context.Items["Chatid"]?.ToString();

            if (!string.IsNullOrEmpty(chatName) && !string.IsNullOrEmpty(userName))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatName);
                await Clients.Group(chatName).ReceiveSystemMessage($"{userName} вышел(ла) из чата", chatId);
            } 

            await base.OnDisconnectedAsync(exception);
        }
    }
}
