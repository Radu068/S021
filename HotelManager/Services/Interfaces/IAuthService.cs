using HotelManager.Models;
using HotelManager.Models.Auth;

namespace HotelManager.Services.Interfaces;

public interface IAuthService
{
    Task<(bool Success, string? Error)> RegisterGuestAsync(
        RegisterInput input);

    Task<User?> AuthenticateAsync(LoginInput input);

    Task<User?> GetUserByIdAsync(int userId);
}