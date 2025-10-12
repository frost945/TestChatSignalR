using TestChatSignalR.Contracts;
using TestChatSignalR.Data;
using TestChatSignalR.Interfaces;
using TestChatSignalR.Models;
using TestChatSignalR.Repositories;

namespace TestChatSignalR.Services
{
    public class ChatsService : IChatsService
    {
        private readonly IChatsRepository _chatsRepository;
        private readonly IUsersRepository _usersRepository;
        private readonly ChatDbContext _dbContext;
        public ChatsService(IChatsRepository chatsRepository, IUsersRepository usersRepository, ChatDbContext dbContext)
        {
            _chatsRepository = chatsRepository;
            _usersRepository = usersRepository;
            _dbContext = dbContext;
        }
        public async Task<IEnumerable<Chat>> GetChatsByUserId(int userId)
        {
            return await _chatsRepository.GetByUserId(userId);
        }

        public async Task<Chat> GetChatByIdAsync(int chatId)
        {
            try
            {
                Chat chat = await _chatsRepository.GetByIdAsync(chatId);
                return chat;
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException($"Chat with chatId {chatId} not found.", ex);
            }
        }

        public async Task<Chat> GetChatByNameAsync(string chatName)
        {
            try
            {
                Chat chat = await _chatsRepository.GetByNameAsync(chatName);
                return chat;
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException($"Chat with name {chatName} not found.", ex);
            }
        }

        public async Task CreateChat(ChatRequest request)
        {
            Chat chat = new Chat { Name = request.Name };

            User user = await _usersRepository.GetByIdAsync(request.UserId);

            // добавляем пользователя в чат, чтобы в таблице JoinUserChat была связь id чата и id пользователя
            chat.Users.Add(user);

            await _chatsRepository.Add(chat);
        }

        public async Task AddUserToChat(int chatId, int userId)
        {
            Console.WriteLine($"AddUserToChat on: chatID:{chatId}, userId{userId}");

            Chat chat = await _chatsRepository.GetByIdAsync(chatId);
            User user = await _usersRepository.GetByIdAsync(userId);
            // добавляем пользователя в чат, чтобы в таблице JoinUserChat была связь id чата и id пользователя
            chat.Users.Add(user);
            await _dbContext.SaveChangesAsync();
        }
    }
}
