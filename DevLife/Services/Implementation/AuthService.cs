using DevLife.Database;
using DevLife.Dtos;
using DevLife.Enums;
using DevLife.Models;
using DevLife.Models.BugChase;
using DevLife.Models.CodeCasino;
using DevLife.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;

namespace DevLife.Services.Implementation;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;

    public AuthService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterDto registerDto)
    {
        try
        {
            // Check if username already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == registerDto.Username);

            if (existingUser != null)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Username already exists"
                };
            }

            // Validate age (must be at least 16 years old)
            var age = DateTime.Now.Year - registerDto.DateOfBirth.Year;
            if (registerDto.DateOfBirth.Date > DateTime.Now.AddYears(-age)) age--;

            if (age < 16)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "User must be at least 16 years old"
                };
            }

            // Create new user
            var user = new User
            {
                Username = registerDto.Username,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                DateOfBirth = registerDto.DateOfBirth,
                TechStack = registerDto.TechStack,
                Experience = registerDto.Experience,
                ZodiacSign = CalculateZodiacSign(registerDto.DateOfBirth),
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Initialize casino stats for new user
            await InitializeUserStatsDirectly(user.Id);

            // Initialize bug chase stats for new user
            await InitializeBugChaseStatsDirectly(user.Id);

            var userProfile = MapToUserProfile(user);

            return new AuthResponse
            {
                Success = true,
                Message = "Registration successful! Welcome to DevLife Casino! 🎰",
                User = userProfile
            };
        }
        catch (Exception ex)
        {
            return new AuthResponse
            {
                Success = false,
                Message = $"Registration failed: {ex.Message}"
            };
        }
    }

    public async Task<AuthResponse> LoginAsync(LoginDto loginDto)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == loginDto.Username);

            if (user == null)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            // Ensure user has casino stats initialized
            await InitializeUserStatsDirectly(user.Id);

            // Ensure user has bug chase stats initialized
            await InitializeBugChaseStatsDirectly(user.Id);

            var userProfile = MapToUserProfile(user);

            return new AuthResponse
            {
                Success = true,
                Message = "Login successful! Ready to play some games? 🎰🏃",
                User = userProfile
            };
        }
        catch (Exception ex)
        {
            return new AuthResponse
            {
                Success = false,
                Message = $"Login failed: {ex.Message}"
            };
        }
    }

    public async Task<UserProfileDto?> GetUserProfileAsync(string username)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username);

        return user != null ? MapToUserProfile(user) : null;
    }

    public ZodiacSign CalculateZodiacSign(DateTime dateOfBirth)
    {
        var month = dateOfBirth.Month;
        var day = dateOfBirth.Day;

        return (month, day) switch
        {
            (3, >= 21) or (4, <= 19) => ZodiacSign.Aries,
            (4, >= 20) or (5, <= 20) => ZodiacSign.Taurus,
            (5, >= 21) or (6, <= 20) => ZodiacSign.Gemini,
            (6, >= 21) or (7, <= 22) => ZodiacSign.Cancer,
            (7, >= 23) or (8, <= 22) => ZodiacSign.Leo,
            (8, >= 23) or (9, <= 22) => ZodiacSign.Virgo,
            (9, >= 23) or (10, <= 22) => ZodiacSign.Libra,
            (10, >= 23) or (11, <= 21) => ZodiacSign.Scorpio,
            (11, >= 22) or (12, <= 21) => ZodiacSign.Sagittarius,
            (12, >= 22) or (1, <= 19) => ZodiacSign.Capricorn,
            (1, >= 20) or (2, <= 18) => ZodiacSign.Aquarius,
            (2, >= 19) or (3, <= 20) => ZodiacSign.Pisces,
            _ => ZodiacSign.Aquarius
        };
    }

    private async Task InitializeUserStatsDirectly(int userId)
    {
        var existingStats = await _context.UserStats.FirstOrDefaultAsync(us => us.UserId == userId);
        if (existingStats == null)
        {
            var userStats = new UserStats { UserId = userId };
            _context.UserStats.Add(userStats);
            await _context.SaveChangesAsync();
        }
    }

    private async Task InitializeBugChaseStatsDirectly(int userId)
    {
        var existingStats = await _context.BugChaseStats.FirstOrDefaultAsync(bcs => bcs.UserId == userId);
        if (existingStats == null)
        {
            var bugChaseStats = new BugChaseStats { UserId = userId };
            _context.BugChaseStats.Add(bugChaseStats);
            await _context.SaveChangesAsync();
        }
    }

    private UserProfileDto MapToUserProfile(User user)
    {
        var age = DateTime.Now.Year - user.DateOfBirth.Year;
        if (user.DateOfBirth.Date > DateTime.Now.AddYears(-age)) age--;

        return new UserProfileDto
        {
            Id = user.Id,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            DateOfBirth = user.DateOfBirth,
            TechStack = user.TechStack,
            Experience = user.Experience,
            ZodiacSign = user.ZodiacSign,
            Age = age,
            CreatedAt = user.CreatedAt
        };
    }
}
