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

            var messages = await _messagesService.LoadHistoryByChatIdAsync(chatId, skip);

            var messageResponses = messages
                .Select(m => new MessageResponse(m.Body, m.UserId))
                .ToList();

            return Ok(messageResponses);
        }
    }
}
