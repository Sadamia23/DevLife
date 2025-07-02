using DevLife.Enums;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace DevLife.Models.DevDating;

public class DatingProfile
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    [Required]
    public Gender Gender { get; set; }

    [Required]
    public Gender Preference { get; set; }

    [Required]
    [StringLength(500)]
    public string Bio { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
