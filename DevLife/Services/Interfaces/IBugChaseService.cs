using DevLife.Dtos;

namespace DevLife.Services.Interfaces;

public interface IBugChaseService
{
    /// <summary>
    /// Submit a new bug chase score for the user
    /// </summary>
    Task<BugChaseGameResultDto> SubmitScoreAsync(string username, BugChaseScoreDto scoreDto);

    /// <summary>
    /// Get user's bug chase statistics
    /// </summary>
    Task<BugChaseStatsDto?> GetUserStatsAsync(string username);

    /// <summary>
    /// Get bug chase leaderboard (top scores)
    /// </summary>
    Task<List<BugChaseLeaderboardEntryDto>> GetLeaderboardAsync(int limit = 5);

    /// <summary>
    /// Get user's recent bug chase games
    /// </summary>
    Task<List<BugChaseGameResultDto>> GetRecentGamesAsync(string username, int limit = 10);

    /// <summary>
    /// Get bug chase dashboard data (stats + leaderboard + recent games)
    /// </summary>
    Task<BugChaseDashboardDto?> GetDashboardAsync(string username);

    /// <summary>
    /// Initialize bug chase stats for a new user
    /// </summary>
    Task InitializeUserStatsAsync(int userId);
}
