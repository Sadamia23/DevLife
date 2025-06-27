using DevLife.Dtos;
using DevLife.Enums;

namespace DevLife.Services.Interfaces;

public interface ICodeCasinoService
{
    Task<CodeChallengeDto?> GetRandomChallengeAsync(string username);
    Task<DailyChallengeDto?> GetDailyChallengeAsync(string username);
    Task<GameResultDto> PlaceBetAsync(string username, PlaceBetDto betDto);
    Task<UserStatsDto?> GetUserStatsAsync(string username);
    Task<List<LeaderboardEntryDto>> GetLeaderboardAsync(int limit = 10);
    Task<CasinoStatsResponse?> GetCasinoStatsAsync(string username);
    double CalculateZodiacLuckMultiplier(ZodiacSign zodiacSign);
    Task InitializeUserStatsAsync(int userId);
}
