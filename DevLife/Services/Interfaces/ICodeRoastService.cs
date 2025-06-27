using DevLife.Dtos;
using DevLife.Enums;

namespace DevLife.Services.Interfaces;

public interface ICodeRoastService
{
    Task<CodeRoastDashboardDto?> GetDashboardAsync(string username);
    Task<CodeRoastTaskDto?> GetCodingTaskAsync(string username, ExperienceLevel difficulty);
    Task<CodeRoastResultDto> SubmitCodeAsync(string username, CodeRoastSubmissionDto submissionDto);
    Task<CodeRoastStatsDto?> GetUserStatsAsync(string username);
    Task<List<CodeRoastResultDto>> GetRoastHistoryAsync(string username, int limit = 10);
    Task<CodeRoastHallOfFameDto> GetHallOfFameAsync();
    Task InitializeUserStatsAsync(int userId);
    Task<List<CodeRoastTaskDto>> GetRecommendedTasksAsync(string username, int limit = 3);
}
