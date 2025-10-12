using TestChatSignalR.Contracts;

namespace TestChatSignalR.Interfaces
{
    public interface IUsersService
    {
        public Task RegisterAsync(RegisterUserRequest request);
        public Task<string> LoginAsync(LoginUserRequest request);
    }
}
