using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TestChatSignalR.Contracts;
using TestChatSignalR.Data;
using TestChatSignalR.Interfaces;
using TestChatSignalR.Models;
using TestChatSignalR.Repositories;

namespace TestChatSignalR.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ChatsController : ControllerBase
    {
        private readonly IChatsService _chatsService;
        private readonly IChatsRepository _chatsRepository;
        public ChatsController(IChatsService chatsService, IChatsRepository chatsRepository)
        {
            _chatsService = chatsService;
            _chatsRepository = chatsRepository;
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetChatsByUserId(int userId)
        {
            var chats = await _chatsService.GetChatsByUserId(userId);

            var chatsDTO = chats.Select(c => new
            { 
                Id = c.Id,
                DisplayName = c.GetDisplayName(userId)
            });
            return Ok(chatsDTO);
        }

        [HttpGet("{chatId}")]
        public async Task<IActionResult> GetChatById(int chatId)
        {
                var chat = await _chatsService.GetChatByIdAsync(chatId);
                return Ok(chat);
        }

       /* [HttpGet("by-name/{chatName}")]
        public async Task<IActionResult> GetChatByName(string chatName)
        {
            try
            {
                var chat = await _chatsService.GetChatByNameAsync(chatName);
                return Ok(chat);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }*/

        [HttpPost]
        public async Task<IActionResult> CreateChat(ChatRequest request)
        {
                await _chatsService.CreateChat(request);
                return Ok("chat successful created");
        }
    }
}
