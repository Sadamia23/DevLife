using DevLife.Enums;
using System.ComponentModel.DataAnnotations;

namespace DevLife.Dtos;

public class CodeChallengeDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TechnologyStack TechStack { get; set; }
    public ExperienceLevel DifficultyLevel { get; set; }
    public string CodeOption1 { get; set; } = string.Empty;
    public string CodeOption2 { get; set; } = string.Empty;
    public bool IsDailyChallenge { get; set; }
    public int BonusMultiplier { get; set; } = 2;
}

public class PlaceBetDto
{
    [Required]
    [Range(1, 10000)]
    public int PointsBet { get; set; }

    [Required]
    public int ChallengeId { get; set; }

    [Required]
    public int ChosenOption { get; set; } // 1 or 2
}

public class GameResultDto
{
    public bool IsCorrect { get; set; }
    public int PointsBet { get; set; }
    public int PointsWon { get; set; }
    public int PointsLost { get; set; }
    public int NewTotalPoints { get; set; }
    public int CurrentStreak { get; set; }
    public bool StreakBroken { get; set; }
    public double LuckMultiplier { get; set; }
    public string Explanation { get; set; } = string.Empty;
    public string CorrectCode { get; set; } = string.Empty;
    public string BuggyCode { get; set; } = string.Empty;
}

public class UserStatsDto
{
    public int TotalPoints { get; set; }
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public int TotalGamesPlayed { get; set; }
    public int TotalGamesWon { get; set; }
    public double WinRate { get; set; }
    public bool CanPlayDailyChallenge { get; set; }
    public DateTime? LastDailyChallenge { get; set; }
}

public class LeaderboardEntryDto
{
    public int Rank { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public TechnologyStack TechStack { get; set; }
    public ZodiacSign ZodiacSign { get; set; }
    public int TotalPoints { get; set; }
    public int CurrentStreak { get; set; }
    public int TotalGamesWon { get; set; }
    public double WinRate { get; set; }
}

public class DailyChallengeDto
{
    public CodeChallengeDto Challenge { get; set; } = null!;
    public DateTime ChallengeDate { get; set; }
    public int BonusMultiplier { get; set; }
    public bool HasPlayed { get; set; }
}

public class CasinoStatsResponse
{
    public UserStatsDto UserStats { get; set; } = null!;
    public List<LeaderboardEntryDto> TopPlayers { get; set; } = new();
    public DailyChallengeDto? DailyChallenge { get; set; }
}
