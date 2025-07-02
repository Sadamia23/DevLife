using System.ComponentModel.DataAnnotations;

namespace DevLife.Models.DevDating;

public class DatingStats
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int TotalSwipes { get; set; } = 0;
    public int TotalLikes { get; set; } = 0;
    public int TotalMatches { get; set; } = 0;
    public int TotalMessages { get; set; } = 0;

    public DateTime LastActiveDate { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
