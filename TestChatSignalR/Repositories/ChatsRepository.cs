using Microsoft.EntityFrameworkCore;
using TestChatSignalR.Data;
using TestChatSignalR.Interfaces;
using TestChatSignalR.Models;

namespace TestChatSignalR.Repositories
{
    public class ChatsRepository : IChatsRepository
    {
        private readonly ChatDbContext _dbContext;
        public ChatsRepository(ChatDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // получить все чаты в которых пользователь написал сообщение, если чатов нет, то вернется пустой список
        public async Task<IEnumerable<Chat>> GetByUserId(int userId)
        {
            var chats = await _dbContext.Users
                .Where(u => u.Id == userId)
                .SelectMany(u=>u.Chats)
                .ToListAsync();

            return chats;
        }

        public async Task<Chat> GetByIdAsync(int chatId)
        {
            Chat chat = await _dbContext.Chats
                .FirstAsync(c => c.Id == chatId) ?? throw new InvalidOperationException($"Chat with chatId {chatId} not found.");
            return chat;
        }

        public async Task<Chat> GetByNameAsync(string chatName)
        {
            Chat chat = await _dbContext.Chats
                .FirstOrDefaultAsync(c => c.Name == chatName) ?? throw new InvalidOperationException($"Chat with name {chatName} not found.");
            return chat;
        }

        public async Task Add(Chat chat)
        {
            _dbContext.Chats.Add(chat);
            await _dbContext.SaveChangesAsync();
        }
    }
}