using System.ComponentModel.DataAnnotations;

namespace DevLife.Models.CodeCasino;

public class DailyChallenge
{
    public int Id { get; set; }

    [Required]
    public int CodeChallengeId { get; set; }
    public CodeChallenge CodeChallenge { get; set; } = null!;

    [Required]
    public DateTime ChallengeDate { get; set; }

    [Required]
    public int BonusMultiplier { get; set; } = 3; // Daily challenges give 3x points

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
