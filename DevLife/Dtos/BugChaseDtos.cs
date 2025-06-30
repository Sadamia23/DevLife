using DevLife.Enums;
using System.ComponentModel.DataAnnotations;

namespace DevLife.Dtos;

public class BugChaseScoreDto
{
    [Required]
    [Range(0, int.MaxValue)]
    public int Score { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public int Distance { get; set; }

    [Required]
    public TimeSpan SurvivalTime { get; set; }

    public int BugsAvoided { get; set; } = 0;
    public int DeadlinesAvoided { get; set; } = 0;
    public int MeetingsAvoided { get; set; } = 0;
    public int CoffeeCollected { get; set; } = 0;
    public int WeekendsCollected { get; set; } = 0;
}

public class BugChaseGameResultDto
{
    public int Id { get; set; }
    public int Score { get; set; }
    public int Distance { get; set; }
    public TimeSpan SurvivalTime { get; set; }
    public int BugsAvoided { get; set; }
    public int DeadlinesAvoided { get; set; }
    public int MeetingsAvoided { get; set; }
    public int CoffeeCollected { get; set; }
    public int WeekendsCollected { get; set; }
    public DateTime PlayedAt { get; set; }
    public bool IsNewBestScore { get; set; }
    public int Rank { get; set; }
}

public class BugChaseLeaderboardEntryDto
{
    public int Rank { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public TechnologyStack TechStack { get; set; }
    public ZodiacSign ZodiacSign { get; set; }
    public int Score { get; set; }
    public int Distance { get; set; }
    public TimeSpan SurvivalTime { get; set; }
    public DateTime PlayedAt { get; set; }
}

public class BugChaseStatsDto
{
    public int BestScore { get; set; }
    public int TotalGamesPlayed { get; set; }
    public int TotalDistance { get; set; }
    public TimeSpan TotalSurvivalTime { get; set; }
    public int TotalBugsAvoided { get; set; }
    public int TotalDeadlinesAvoided { get; set; }
    public int TotalMeetingsAvoided { get; set; }
    public int TotalCoffeeCollected { get; set; }
    public int TotalWeekendsCollected { get; set; }
    public double AverageScore { get; set; }
    public double AverageSurvivalTime { get; set; }
}

public class BugChaseDashboardDto
{
    public BugChaseStatsDto UserStats { get; set; } = null!;
    public List<BugChaseLeaderboardEntryDto> TopScores { get; set; } = new();
    public List<BugChaseGameResultDto> RecentGames { get; set; } = new();
}

public interface GameObject
{
    int X { get; set; }
    int Y { get; set; }
    int Width { get; set; }
    int Height { get; set; }
    GameObjectType Type { get; set; }
    string? Emoji { get; set; }
    string? Color { get; set; }
    double? Speed { get; set; }
}

public enum GameObjectType
{
    Player,
    Bug,
    Deadline,
    Meeting,
    Coffee,
    Weekend
}

public class GameState
{
    public bool IsPlaying { get; set; }
    public bool IsPaused { get; set; }
    public bool IsGameOver { get; set; }
    public int Score { get; set; }
    public int Distance { get; set; }
    public double Speed { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public List<GameObject> Obstacles { get; set; } = new();
    public List<GameObject> PowerUps { get; set; } = new();
    public GameObject Player { get; set; } = null!;
    public List<GameEffect> Effects { get; set; } = new();
}

public class GameEffect
{
    public string Type { get; set; } = string.Empty; // 'speed' | 'invincible'
    public DateTime StartTime { get; set; }
    public int Duration { get; set; } // in milliseconds
    public bool Active { get; set; }
}

public class GameStats
{
    public int BugsAvoided { get; set; }
    public int DeadlinesAvoided { get; set; }
    public int MeetingsAvoided { get; set; }
    public int CoffeeCollected { get; set; }
    public int WeekendsCollected { get; set; }
}

public class GameControls
{
    public bool Up { get; set; }
    public bool Down { get; set; }
    public bool Jump { get; set; }
    public bool Duck { get; set; }
}