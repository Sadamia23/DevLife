using System.ComponentModel.DataAnnotations;

namespace DevLife.Models.BugChase;

public class BugChaseScore
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    [Required]
    public int Score { get; set; }

    [Required]
    public int Distance { get; set; }

    [Required]
    public TimeSpan SurvivalTime { get; set; }

    public int BugsAvoided { get; set; } = 0;
    public int DeadlinesAvoided { get; set; } = 0;
    public int MeetingsAvoided { get; set; } = 0;
    public int CoffeeCollected { get; set; } = 0;
    public int WeekendsCollected { get; set; } = 0;

    public DateTime PlayedAt { get; set; } = DateTime.UtcNow;
}
