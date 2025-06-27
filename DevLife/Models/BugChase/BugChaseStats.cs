using System.ComponentModel.DataAnnotations;

namespace DevLife.Models.BugChase;

public class BugChaseStats
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    [Required]
    public int BestScore { get; set; } = 0;

    [Required]
    public int TotalGamesPlayed { get; set; } = 0;

    [Required]
    public int TotalDistance { get; set; } = 0;

    [Required]
    public TimeSpan TotalSurvivalTime { get; set; } = TimeSpan.Zero;

    [Required]
    public int TotalBugsAvoided { get; set; } = 0;

    [Required]
    public int TotalDeadlinesAvoided { get; set; } = 0;

    [Required]
    public int TotalMeetingsAvoided { get; set; } = 0;

    [Required]
    public int TotalCoffeeCollected { get; set; } = 0;

    [Required]
    public int TotalWeekendsCollected { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Calculated properties
    public double AverageScore => TotalGamesPlayed > 0 ? (double)TotalDistance / TotalGamesPlayed : 0;
    public double AverageSurvivalTime => TotalGamesPlayed > 0 ? TotalSurvivalTime.TotalSeconds / TotalGamesPlayed : 0;
}
