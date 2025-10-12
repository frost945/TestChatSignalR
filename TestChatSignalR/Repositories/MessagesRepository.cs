using TestChatSignalR.Models;
using Microsoft.EntityFrameworkCore;
using TestChatSignalR.Data;
using TestChatSignalR.Interfaces;

namespace TestChatSignalR.Repositories
{
    public class MessagesRepository : IMessagesRepository
    {
        private readonly ChatDbContext _context;

        public MessagesRepository(ChatDbContext context)
        {
            _context = context;
        }

        public async Task<List<Message>> GetByChatIdAsync(int chatId, int count, int skip)
        {
            return await _context.Messages
                .Where(m => m.ChatId == chatId)
                .OrderByDescending(m => m.CreatedAt)
                .Skip(skip)
                .Take(count)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task CreateAsync(Message message)
        {
            await _context.AddAsync(message);
            await _context.SaveChangesAsync();
        }
    }
}
