using DevLife.Services.Implementation;

namespace DevLife.Services.Interfaces;

public interface IAIGitHubPersonalityService
{
    Task<PersonalityAnalysisResult> AnalyzePersonalityAsync(GitHubAnalysisData analysisData);
}
