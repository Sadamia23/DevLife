using System.ComponentModel.DataAnnotations;

namespace DevLife.Models.CodeCasino;

public class UserGameSession
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    // Make nullable for AI challenges
    public int? CodeChallengeId { get; set; }
    public CodeChallenge? CodeChallenge { get; set; }

    // Store AI challenge data for AI-generated challenges
    public string? AIChallengeTitle { get; set; }
    public string? AIChallengeDescription { get; set; }
    public string? AICorrectCode { get; set; }
    public string? AIBuggyCode { get; set; }
    public string? AIExplanation { get; set; }
    public string? AITopic { get; set; }

    [Required]
    public int PointsBet { get; set; }

    [Required]
    public bool UserChoice { get; set; } // true = chose correct code, false = chose buggy code

    [Required]
    public bool IsCorrect { get; set; }

    [Required]
    public int PointsWon { get; set; }

    public double LuckMultiplier { get; set; } = 1.0;

    public DateTime PlayedAt { get; set; } = DateTime.UtcNow;

    public bool IsDailyChallenge { get; set; } = false;

    public bool IsAIGenerated { get; set; } = false;
}