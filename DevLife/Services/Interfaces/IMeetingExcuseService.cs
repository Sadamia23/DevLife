using DevLife.Dtos;

namespace DevLife.Services.Interfaces;

public interface IMeetingExcuseService
{
    // Core excuse generation (existing methods)
    Task<MeetingExcuseDto> GenerateRandomExcuseAsync(string username, GenerateExcuseRequestDto? criteria = null);
    Task<List<MeetingExcuseDto>> GenerateBulkExcusesAsync(string username, BulkExcuseGenerationDto request);
    Task<MeetingExcuseDto?> GetExcuseOfTheDayAsync();

    // NEW: AI-powered excuse generation
    Task<MeetingExcuseDto> GenerateAIExcuseAsync(string username, GenerateExcuseRequestDto? criteria = null);
    Task<List<MeetingExcuseDto>> GenerateAIBulkExcusesAsync(string username, BulkExcuseGenerationDto request);
    Task<MeetingExcuseDto> GeneratePersonalizedAIExcuseAsync(string username, GenerateExcuseRequestDto? criteria = null);

    // Favorites management (existing)
    Task<MeetingExcuseFavoriteDto> SaveFavoriteAsync(string username, SaveFavoriteRequestDto request);
    Task<bool> RemoveFavoriteAsync(string username, int favoriteId);
    Task<List<MeetingExcuseFavoriteDto>> GetUserFavoritesAsync(string username, int limit = 20);

    // Usage tracking (existing)
    Task<MeetingExcuseUsageDto> SubmitUsageAsync(string username, SubmitUsageRequestDto request);
    Task<List<MeetingExcuseUsageDto>> GetUserUsageHistoryAsync(string username, int limit = 20);

    // Statistics and dashboard (existing)
    Task<MeetingExcuseDashboardDto> GetDashboardAsync(string username);
    Task<MeetingExcuseStatsDto> GetUserStatsAsync(string username);
    Task<List<MeetingExcuseLeaderboardEntryDto>> GetLeaderboardAsync(int limit = 10);

    // Rating system (existing)
    Task<bool> RateExcuseAsync(string username, RateExcuseRequestDto request);
    Task<List<MeetingExcuseDto>> GetTrendingExcusesAsync(int limit = 10);
    Task<List<MeetingExcuseDto>> GetTopRatedExcusesAsync(int limit = 10);

    // Analytics (existing)
    Task<ExcuseAnalyticsDto> GetAnalyticsAsync();

    // Utility methods (existing)
    Task InitializeUserStatsAsync(int userId);
    Task<bool> CheckAndUpdateStreakAsync(int userId);
    Task<List<string>> GetAvailableTagsAsync();
}