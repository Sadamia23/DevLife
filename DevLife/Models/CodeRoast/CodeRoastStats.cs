using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DevLife.Models.CodeRoast;

public class CodeRoastStats
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }
    public virtual User User { get; set; } = null!;

    // Basic statistics
    public int TotalSubmissions { get; set; } = 0;
    public int TotalRoasts { get; set; } = 0;
    public int TotalPraises { get; set; } = 0;
    public int PerfectScores { get; set; } = 0;

    // Score statistics
    public double AverageScore { get; set; } = 0.0;
    public int HighestScore { get; set; } = 0;
    public int LowestScore { get; set; } = 100;

    // Streaks
    public int CurrentStreak { get; set; } = 0; // Current consecutive good scores (70+)
    public int LongestStreak { get; set; } = 0;
    public int CurrentRoastStreak { get; set; } = 0; // Current consecutive bad scores (<70)
    public int LongestRoastStreak { get; set; } = 0;

    // Time statistics
    public int TotalTimeSpentMinutes { get; set; } = 0;
    public double AverageTimePerTask { get; set; } = 0.0;

    // Difficulty breakdown
    public int JuniorTasksCompleted { get; set; } = 0;
    public int MiddleTasksCompleted { get; set; } = 0;
    public int SeniorTasksCompleted { get; set; } = 0;

    // Quality metrics (averages)
    public double AverageReadabilityScore { get; set; } = 0.0;
    public double AveragePerformanceScore { get; set; } = 0.0;
    public double AverageCorrectnessScore { get; set; } = 0.0;
    public double AverageBestPracticesScore { get; set; } = 0.0;

    // Recent performance (JSON array of last 10 scores)
    [Column(TypeName = "nvarchar(max)")]
    public string RecentScoresJson { get; set; } = "[]";

    // Achievements (JSON array of achievement names)
    [Column(TypeName = "nvarchar(max)")]
    public string UnlockedAchievementsJson { get; set; } = "[]";

    // Timestamps
    public DateTime? LastSubmission { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}