using AuthApi.Entities;
using AuthApi.Models;

namespace AuthApi.Services
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(UserDto request);
        Task<string?> LoginAsync(UserDto request);

    }
}
