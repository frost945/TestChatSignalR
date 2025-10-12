using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TestChatSignalR.Contracts;
using TestChatSignalR.Data;
using TestChatSignalR.Interfaces;
using TestChatSignalR.Models;

namespace TestChatSignalR.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ChatsController : ControllerBase
    {
        private readonly IChatsService _chatsService;
        public ChatsController(IChatsService chatsService)
        {
            _chatsService = chatsService;
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetChatsByUserId (int userId)
        {
            var chats = await _chatsService.GetChatsByUserId(userId);
            return Ok(chats);
        }

        [HttpGet("{chatId}")]
        public async Task<IActionResult> GetChatById(int chatId)
        {
            try 
            {
                var chat = await _chatsService.GetChatByIdAsync(chatId);
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
        }

        [HttpGet("by-name/{chatName}")]
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
        }

        [HttpPost]
        public async Task<IActionResult> CreateChat(ChatRequest request)
        {
            await _chatsService.CreateChat(request);
            return Ok("chat successful created");
        }

        [HttpPost("{chatId}/add-user/{userId}")]
        public async Task<IActionResult> AddUserToChat(int chatId, int userId)
        {
            try
            {
                await _chatsService.AddUserToChat(chatId, userId);
                return Ok("user successful added to chat");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
