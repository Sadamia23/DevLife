using DevLife.Dtos;

namespace DevLife.Services.Interfaces;

public interface IBugChaseService
{
    Task<BugChaseGameResultDto> SubmitScoreAsync(string username, BugChaseScoreDto scoreDto);

    Task<BugChaseStatsDto?> GetUserStatsAsync(string username);

    Task<List<BugChaseLeaderboardEntryDto>> GetLeaderboardAsync(int limit = 5);

    Task<List<BugChaseGameResultDto>> GetRecentGamesAsync(string username, int limit = 10);

    Task<BugChaseDashboardDto?> GetDashboardAsync(string username);

    Task InitializeUserStatsAsync(int userId);
}
