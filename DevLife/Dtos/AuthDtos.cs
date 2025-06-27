using DevLife.Enums;
using System.ComponentModel.DataAnnotations;

namespace DevLife.Dtos;

public class RegisterDto
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    public DateTime DateOfBirth { get; set; }

    [Required]
    public TechnologyStack TechStack { get; set; }

    [Required]
    public ExperienceLevel Experience { get; set; }
}

public class LoginDto
{
    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;
}

public class UserProfileDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public TechnologyStack TechStack { get; set; }
    public ExperienceLevel Experience { get; set; }
    public ZodiacSign ZodiacSign { get; set; }
    public int Age { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AuthResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public UserProfileDto? User { get; set; }
}
