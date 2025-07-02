using DevLife.Dtos;

namespace DevLife.Services.Interfaces;

public interface IGitHubAnalysisService
{
    Task<GitHubAnalysisResponse> AnalyzeUserRepositoriesAsync(int userId, GitHubAnalysisRequest request, string accessToken);
    Task<GitHubAnalysisResponse?> GetAnalysisResultAsync(int analysisId, int userId);
    Task<List<GitHubAnalysisResponse>> GetUserAnalysisHistoryAsync(int userId, int page = 1, int pageSize = 10);
    Task<bool> DeleteAnalysisAsync(int analysisId, int userId);
    Task<string> GenerateShareableCardAsync(int analysisId);
    Task<bool> ToggleFavoriteAsync(int analysisId, int userId);
}