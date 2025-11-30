using Microsoft.AspNetCore.Http.HttpResults;
using TestChatSignalR.Contracts;
using TestChatSignalR.Interfaces;
using TestChatSignalR.Models;

namespace TestChatSignalR.Services
{
    public class UsersService : IUsersService
    {
        private readonly IUsersRepository _usersRepository;
        public UsersService(IUsersRepository usersRepository)
        {
            _usersRepository = usersRepository;
        }
        public async Task RegisterAsync(RegisterUserRequest request)
        {
            try
            {
                var user = new User(request.UserName, request.Email, request.Password);
                await _usersRepository.CreateAsync(user);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException($"Registration failed: {ex.Message}");
            }
        }

        public async Task<string> LoginAsync(LoginUserRequest request)
        {
            User user = await _usersRepository.GetByEmailAsync(request.Email);
            bool result = user.PasswordHash == request.Password;
            if (result == false)
            {
                throw new UnauthorizedAccessException("Failed to login");
            }
            return user.Id.ToString();// тестово возвращаем Id пользователя
        }
    }
}
