using Application.Dtos;

namespace Application.Interfaces
{
    public interface IAuthService
    {
        public Task<bool> RegisterAsync(RegisterRequest model);
        public Task<string> LoginAsync(LoginRequest model);
    }
}