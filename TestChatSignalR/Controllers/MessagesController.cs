using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestChatSignalR.Contracts;
using TestChatSignalR.Data;
using TestChatSignalR.Interfaces;

namespace TestChatSignalR.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IMessagesService _messagesService;
        private readonly ChatDbContext _dbContext;
        public MessagesController(IMessagesService messagesService, ChatDbContext dbContext)
        {
            _messagesService = messagesService;
            _dbContext = dbContext;
        }

        [HttpGet("{chatId}")]
        public async Task<IActionResult> GetMessagesByChatId(int chatId, int skip=0)
        {
            var messages = await _messagesService.LoadHistoryByChatIdAsync(chatId, skip); //используем базовай метод, чтобы репо и сервис не знали про DTO
            var userIds=messages.Select(m=>m.UserId).Distinct().ToList();//собираем id юзеров в messages

            var usersDict = await _dbContext.Users//вытягиваем userName, чтобы в словаре были пары id-name
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, u.Name })
                .ToDictionaryAsync(u => u.Id, u => u.Name);

            //мапим DTO на клиента
            var messageResponses = messages
            .Select(m => new MessageResponse(
                m.Body,
                usersDict.TryGetValue(m.UserId, out var name) ? name : "Unknown", 
                m.UserId))
            .ToList();
            return Ok(messageResponses);
        }
    }
}
