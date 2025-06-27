namespace DevLife.Models;

using DevLife.Enums;
using System.ComponentModel.DataAnnotations;

public class User
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
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

    public ZodiacSign ZodiacSign { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
