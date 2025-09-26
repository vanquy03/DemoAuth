using Application.Dtos;

namespace Application.Interfaces
{
    public interface IAuthService
    {
        Task<bool> RegisterAsync(RegisterRequest model);
        Task<string> LoginAsync(LoginRequest model);
    }
}