using TestChatSignalR.Models;
using Microsoft.EntityFrameworkCore;


namespace TestChatSignalR.Domain
{
    public class ChatRepository
    {
        private readonly ChatDbContext context;

        public ChatRepository(ChatDbContext Context)
        {
            context = Context;
        }

        //в процессе доработки
        public async Task<List<ChatMessage>> GetMessagesAsync(string ChatName, int count, int skip)
        {
            return await context.chatMessages
                .Where(m => m.chatName == ChatName)
                .OrderByDescending(m => m.sentAt)
                .Skip(skip)
                .Take(count)
                .ToListAsync();
        }
    }
}
