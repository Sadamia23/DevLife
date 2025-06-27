using DevLife.Enums;
using System.ComponentModel.DataAnnotations;

namespace DevLife.Dtos;

public class MeetingExcuseDto
{
    public int Id { get; set; }
    public string ExcuseText { get; set; } = string.Empty;
    public MeetingCategory Category { get; set; }
    public ExcuseType Type { get; set; }
    public int BelievabilityScore { get; set; }
    public List<string> Tags { get; set; } = new();
    public int UsageCount { get; set; }
    public double AverageRating { get; set; }
    public int RatingCount { get; set; }
    public bool IsFavorite { get; set; } = false;
    public DateTime CreatedAt { get; set; }
}

public class GenerateExcuseRequestDto
{
    public MeetingCategory? Category { get; set; }
    public ExcuseType? Type { get; set; }
    public int? MinBelievability { get; set; }
    public int? MaxBelievability { get; set; }
    public List<string>? Tags { get; set; }
    public bool ExcludeUsed { get; set; } = false;
}

public class MeetingExcuseFavoriteDto
{
    public int Id { get; set; }
    public int MeetingExcuseId { get; set; }
    public string? CustomName { get; set; }
    public int? UserRating { get; set; }
    public DateTime SavedAt { get; set; }
    public MeetingExcuseDto Excuse { get; set; } = null!;
}

public class SaveFavoriteRequestDto
{
    [Required]
    public int MeetingExcuseId { get; set; }

    [StringLength(200)]
    public string? CustomName { get; set; }

    [Range(1, 5)]
    public int? UserRating { get; set; }
}

public class MeetingExcuseUsageDto
{
    public int Id { get; set; }
    public int MeetingExcuseId { get; set; }
    public string? Context { get; set; }
    public bool? WasSuccessful { get; set; }
    public DateTime UsedAt { get; set; }
    public MeetingExcuseDto Excuse { get; set; } = null!;
}

public class SubmitUsageRequestDto
{
    [Required]
    public int MeetingExcuseId { get; set; }

    [StringLength(500)]
    public string? Context { get; set; }

    public bool? WasSuccessful { get; set; }
}

public class MeetingExcuseStatsDto
{
    public int UserId { get; set; }
    public int TotalExcusesGenerated { get; set; }
    public int TotalFavorites { get; set; }
    public MeetingCategory? FavoriteCategory { get; set; }
    public ExcuseType? FavoriteType { get; set; }
    public double AverageBelievability { get; set; }
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public List<string> UnlockedAchievements { get; set; } = new();
    public DateTime LastExcuseGenerated { get; set; }
}

public class MeetingExcuseDashboardDto
{
    public MeetingExcuseStatsDto UserStats { get; set; } = null!;
    public MeetingExcuseDto? ExcuseOfTheDay { get; set; }
    public List<MeetingExcuseFavoriteDto> RecentFavorites { get; set; } = new();
    public List<MeetingExcuseUsageDto> RecentUsage { get; set; } = new();
    public List<MeetingExcuseLeaderboardEntryDto> TopUsersThisWeek { get; set; } = new();
    public List<MeetingExcuseDto> TrendingExcuses { get; set; } = new();
}

public class MeetingExcuseLeaderboardEntryDto
{
    public string Username { get; set; } = string.Empty;
    public int TotalExcusesGenerated { get; set; }
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public double AverageBelievability { get; set; }
    public int Position { get; set; }
}

public class RateExcuseRequestDto
{
    [Required]
    public int MeetingExcuseId { get; set; }

    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }
}

public class MeetingExcuseResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public MeetingExcuseDto? Excuse { get; set; }
}

public class BulkExcuseGenerationDto
{
    public GenerateExcuseRequestDto Criteria { get; set; } = new();
    [Range(1, 10)]
    public int Count { get; set; } = 1;
}

public class ExcuseAnalyticsDto
{
    public Dictionary<MeetingCategory, int> CategoryUsage { get; set; } = new();
    public Dictionary<ExcuseType, int> TypeUsage { get; set; } = new();
    public Dictionary<int, int> BelievabilityDistribution { get; set; } = new();
    public List<MeetingExcuseDto> MostPopularExcuses { get; set; } = new();
    public List<MeetingExcuseDto> HighestRatedExcuses { get; set; } = new();
    public double AverageRatingAcrossAllExcuses { get; set; }
    public int TotalExcusesInDatabase { get; set; }
    public int TotalUsageCount { get; set; }
}
