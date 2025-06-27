using System.ComponentModel.DataAnnotations;

namespace DevLife.Models.CodeCasino;

public class UserStats
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    [Required]
    public int TotalPoints { get; set; } = 1000; // Starting points

    [Required]
    public int CurrentStreak { get; set; } = 0;

    [Required]
    public int LongestStreak { get; set; } = 0;

    [Required]
    public int TotalGamesPlayed { get; set; } = 0;

    [Required]
    public int TotalGamesWon { get; set; } = 0;

    public DateTime? LastDailyChallenge { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Calculated property
    public double WinRate => TotalGamesPlayed > 0 ? (double)TotalGamesWon / TotalGamesPlayed * 100 : 0;
}
