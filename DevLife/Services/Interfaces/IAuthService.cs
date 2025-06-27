using DevLife.Dtos;
using DevLife.Enums;

namespace DevLife.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterDto registerDto);
    Task<AuthResponse> LoginAsync(LoginDto loginDto);
    Task<UserProfileDto?> GetUserProfileAsync(string username);
    ZodiacSign CalculateZodiacSign(DateTime dateOfBirth);
}
