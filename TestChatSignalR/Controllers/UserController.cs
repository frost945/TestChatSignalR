using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using TestChatSignalR.Contracts;
using TestChatSignalR.Interfaces;

namespace TestChatSignalR.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUsersService _usersService;
        private readonly IUsersRepository _usersRepository;
        public UserController(IUsersService userService, IUsersRepository usersRepository)
        {
            _usersService = userService;
            _usersRepository = usersRepository;
        }

        [HttpGet("by-id/{userId}")]
        public async Task<IActionResult> GetUserById(int userId)
        {
            var user =  await _usersRepository.GetByIdAsync(userId);

            return Ok(user.UserName);
        }

        [HttpGet("by-name/{userName}")]
        public async Task<IActionResult> GetUserByName(string userName)
        {
            var user = await _usersRepository.GetByNameAsync(userName);

            UserReceiverResponse response= new UserReceiverResponse (user.Id, user.UserName);

            return Ok(response);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterUserRequest request)
        {

            await _usersService.RegisterAsync(request);

            return Ok("Registration successful");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginUserRequest request)
        {
            string tokenUserId = await _usersService.LoginAsync(request);

            return Ok(tokenUserId);
        }
    }
}
