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
            try{ return await _chatsRepository.GetByUserId(userId); }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException($"Chats not found", ex);
            }
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
                throw new InvalidOperationException($"Chat not found.", ex);
            }
        }

       /* public async Task<Chat> GetChatByNameAsync(string chatName)
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
        }*/

        public async Task<Chat> CreateChat(ChatRequest request)
        {
            Chat chat = new Chat ( request.SenderId, request.ReceiverId );
            await _chatsRepository.Add(chat);

            return chat;
        }
    }
}
