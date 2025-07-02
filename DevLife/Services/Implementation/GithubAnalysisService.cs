using DevLife.Database;
using DevLife.Dtos;
using DevLife.Models.GitHubAnalysis;
using DevLife.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace DevLife.Services.Implementation;

public class GitHubAnalysisService : IGitHubAnalysisService
{
    private readonly ApplicationDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly IAIGitHubPersonalityService _aiPersonalityService;
    private readonly ILogger<GitHubAnalysisService> _logger;

    public GitHubAnalysisService(
        ApplicationDbContext context,
        HttpClient httpClient,
        IAIGitHubPersonalityService aiPersonalityService,
        ILogger<GitHubAnalysisService> logger)
    {
        _context = context;
        _httpClient = httpClient;
        _aiPersonalityService = aiPersonalityService;
        _logger = logger;
    }

    public async Task<GitHubAnalysisResponse> AnalyzeUserRepositoriesAsync(int userId, GitHubAnalysisRequest request, string accessToken)
    {
        try
        {
            _logger.LogInformation("Starting GitHub analysis for user {UserId}, username {GitHubUsername}", userId, request.GitHubUsername);

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "DevLife-App");

            var repositories = await FetchUserRepositoriesAsync(request.GitHubUsername, request.MaxRepositories, request.IncludeForkedRepos);
            _logger.LogInformation("Fetched {Count} repositories for analysis", repositories.Count);

            var analysisData = new GitHubAnalysisData
            {
                Username = request.GitHubUsername,
                Repositories = new List<RepositoryAnalysis>()
            };

            foreach (var repo in repositories)
            {
                try
                {
                    var repoAnalysis = await AnalyzeRepositoryAsync(repo);
                    analysisData.Repositories.Add(repoAnalysis);
                    _logger.LogDebug("Analyzed repository: {RepoName}", repo.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Failed to analyze repository {RepoName}: {Error}", repo.Name, ex.Message);
                    // Continue with other repositories
                }
            }

            if (!analysisData.Repositories.Any())
            {
                throw new InvalidOperationException("No repositories could be analyzed");
            }

            _logger.LogInformation("Starting AI personality analysis...");
            var personalityResult = await _aiPersonalityService.AnalyzePersonalityAsync(analysisData);
            _logger.LogInformation("AI analysis completed with personality type: {PersonalityType}", personalityResult.PersonalityType);

            // Create analysis result
            var analysisResult = new GitHubAnalysisResult
            {
                UserId = userId,
                PersonalityType = personalityResult.PersonalityType,
                PersonalityDescription = personalityResult.Description,
                StrengthsJson = JsonSerializer.Serialize(personalityResult.Strengths),
                WeaknessesJson = JsonSerializer.Serialize(personalityResult.Weaknesses),
                CelebrityDevelopersJson = JsonSerializer.Serialize(personalityResult.CelebrityDevelopers),
                RepositoriesAnalyzed = repositories.Count,
                TotalCommits = analysisData.Repositories.Sum(r => r.CommitCount),
                TotalFiles = analysisData.Repositories.Sum(r => r.FileCount),
                CommitMessageQuality = personalityResult.Scores.CommitMessageQuality,
                CodeCommentingScore = personalityResult.Scores.CodeCommentingScore,
                VariableNamingScore = personalityResult.Scores.VariableNamingScore,
                ProjectStructureScore = personalityResult.Scores.ProjectStructureScore,
                OverallScore = personalityResult.Scores.OverallScore,
                AnalysisDetailsJson = JsonSerializer.Serialize(analysisData),
                GitHubUsername = request.GitHubUsername
            };

            // Use a transaction to ensure data consistency
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _logger.LogInformation("Saving analysis result to database...");

                // Add analysis result
                _context.GitHubAnalysisResults.Add(analysisResult);
                await _context.SaveChangesAsync(); // Save to get the ID

                _logger.LogInformation("Analysis result saved with ID: {AnalysisId}", analysisResult.Id);

                // Add repositories
                foreach (var repoAnalysis in analysisData.Repositories)
                {
                    var repoEntity = new GitHubRepository
                    {
                        AnalysisResultId = analysisResult.Id, // Use the saved ID
                        Name = repoAnalysis.Name,
                        FullName = repoAnalysis.FullName,
                        Description = repoAnalysis.Description ?? string.Empty,
                        PrimaryLanguage = repoAnalysis.PrimaryLanguage,
                        StarsCount = repoAnalysis.StarsCount,
                        ForksCount = repoAnalysis.ForksCount,
                        CommitsAnalyzed = repoAnalysis.CommitCount,
                        FilesAnalyzed = repoAnalysis.FileCount,
                        RepoCommitQuality = repoAnalysis.CommitQuality,
                        RepoCommentingScore = repoAnalysis.CommentingScore,
                        RepoNamingScore = repoAnalysis.NamingScore,
                        RepoStructureScore = repoAnalysis.StructureScore,
                        RepositoryDetailsJson = JsonSerializer.Serialize(repoAnalysis),
                        CreatedAt = repoAnalysis.CreatedAt,
                        LastUpdated = repoAnalysis.UpdatedAt
                    };

                    _context.GitHubRepositories.Add(repoEntity);
                }

                // Update user stats
                await UpdateUserStatsAsync(userId, analysisResult);

                // Save all changes
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("All database operations completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database transaction failed, rolling back...");
                await transaction.RollbackAsync();
                throw;
            }

            // Create response object (without navigation properties)
            var response = new GitHubAnalysisResponse
            {
                Id = analysisResult.Id,
                PersonalityType = personalityResult.PersonalityType,
                PersonalityDescription = personalityResult.Description,
                Strengths = personalityResult.Strengths,
                Weaknesses = personalityResult.Weaknesses,
                CelebrityDevelopers = personalityResult.CelebrityDevelopers,
                Scores = new AnalysisScores
                {
                    CommitMessageQuality = personalityResult.Scores.CommitMessageQuality,
                    CodeCommentingScore = personalityResult.Scores.CodeCommentingScore,
                    VariableNamingScore = personalityResult.Scores.VariableNamingScore,
                    ProjectStructureScore = personalityResult.Scores.ProjectStructureScore,
                    OverallScore = personalityResult.Scores.OverallScore
                },
                RepositoriesAnalyzed = analysisData.Repositories.Select(r => new RepositorySummary
                {
                    Name = r.Name,
                    Description = r.Description,
                    PrimaryLanguage = r.PrimaryLanguage,
                    StarsCount = r.StarsCount,
                    CommitsAnalyzed = r.CommitCount,
                    Scores = new AnalysisScores
                    {
                        CommitMessageQuality = r.CommitQuality,
                        CodeCommentingScore = r.CommentingScore,
                        VariableNamingScore = r.NamingScore,
                        ProjectStructureScore = r.StructureScore,
                        OverallScore = (r.CommitQuality + r.CommentingScore + r.NamingScore + r.StructureScore) / 4
                    }
                }).ToList(),
                AnalyzedAt = analysisResult.AnalyzedAt
            };

            _logger.LogInformation("GitHub analysis completed successfully for user {UserId}", userId);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GitHub analysis failed for user {UserId}: {Error}", userId, ex.Message);
            throw;
        }
    }

    private async Task<List<GitHubRepositoryData>> FetchUserRepositoriesAsync(string username, int maxRepos, bool includeForked)
    {
        try
        {
            var repos = new List<GitHubRepositoryData>();
            var url = $"https://api.github.com/users/{username}/repos?sort=updated&per_page={maxRepos}";

            if (!includeForked)
            {
                url += "&type=owner";
            }

            _logger.LogDebug("Fetching repositories from: {Url}", url);
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"GitHub API error: {response.StatusCode} - {errorContent}");
            }

            var content = await response.Content.ReadAsStringAsync();
            var repoData = JsonSerializer.Deserialize<List<GitHubRepositoryData>>(content, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });

            return repoData?.Take(maxRepos).ToList() ?? new List<GitHubRepositoryData>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch repositories for user {Username}", username);
            throw;
        }
    }

    private async Task<RepositoryAnalysis> AnalyzeRepositoryAsync(GitHubRepositoryData repo)
    {
        try
        {
            var commits = new List<CommitData>();
            var contents = new List<ContentData>();

            // Fetch commits with error handling
            try
            {
                var commitsUrl = $"https://api.github.com/repos/{repo.FullName}/commits?per_page=50";
                var commitsResponse = await _httpClient.GetAsync(commitsUrl);

                if (commitsResponse.IsSuccessStatusCode)
                {
                    var commitsContent = await commitsResponse.Content.ReadAsStringAsync();
                    commits = JsonSerializer.Deserialize<List<CommitData>>(commitsContent, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                    }) ?? new List<CommitData>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to fetch commits for {RepoName}: {Error}", repo.FullName, ex.Message);
            }

            // Fetch contents with error handling
            try
            {
                var contentsUrl = $"https://api.github.com/repos/{repo.FullName}/contents";
                var contentsResponse = await _httpClient.GetAsync(contentsUrl);

                if (contentsResponse.IsSuccessStatusCode)
                {
                    var contentsContent = await contentsResponse.Content.ReadAsStringAsync();
                    contents = JsonSerializer.Deserialize<List<ContentData>>(contentsContent, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                    }) ?? new List<ContentData>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to fetch contents for {RepoName}: {Error}", repo.FullName, ex.Message);
            }

            var commitQuality = AnalyzeCommitMessages(commits);
            var namingScore = AnalyzeVariableNaming(contents);
            var structureScore = AnalyzeProjectStructure(contents);
            var commentingScore = await AnalyzeCodeCommenting(repo.FullName, contents);

            return new RepositoryAnalysis
            {
                Name = repo.Name,
                FullName = repo.FullName,
                Description = repo.Description ?? "",
                PrimaryLanguage = repo.Language ?? "Unknown",
                StarsCount = repo.StargazersCount,
                ForksCount = repo.ForksCount,
                CommitCount = commits.Count,
                FileCount = contents.Count,
                CommitQuality = commitQuality,
                CommentingScore = commentingScore,
                NamingScore = namingScore,
                StructureScore = structureScore,
                CreatedAt = repo.CreatedAt,
                UpdatedAt = repo.UpdatedAt,
                Commits = commits.Take(20).ToList() // Store sample commits
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to analyze repository {RepoName}", repo.FullName);
            throw;
        }
    }

    private int AnalyzeCommitMessages(List<CommitData> commits)
    {
        if (!commits.Any()) return 50; // Default score

        var score = 0;
        var totalCommits = commits.Count;

        foreach (var commit in commits)
        {
            var message = commit.Commit?.Message ?? "";
            var commitScore = 0;

            var firstLine = message.Split('\n')[0];
            if (firstLine.Length >= 10 && firstLine.Length <= 72) commitScore += 25;

            if (char.IsUpper(firstLine.FirstOrDefault())) commitScore += 15;

            if (!firstLine.EndsWith('.')) commitScore += 10;

            var meaningfulWords = new[] { "implement", "add", "refactor", "optimize", "enhance", "resolve", "create" };
            if (meaningfulWords.Any(word => firstLine.ToLower().Contains(word))) commitScore += 25;

            if (IsConventionalCommit(firstLine)) commitScore += 25;

            score += Math.Min(commitScore, 100);
        }

        return score / totalCommits;
    }

    private bool IsConventionalCommit(string message)
    {
        var types = new[] { "feat", "fix", "docs", "style", "refactor", "test", "chore", "perf", "ci", "build", "revert" };
        return types.Any(type => message.ToLower().StartsWith($"{type}:") || message.ToLower().StartsWith($"{type}("));
    }

    private int AnalyzeVariableNaming(List<ContentData> contents)
    {
        return 75; // Placeholder score
    }

    private int AnalyzeProjectStructure(List<ContentData> contents)
    {
        var score = 50; // Base score

        var hasReadme = contents.Any(c => c.Name.ToLower().Contains("readme"));
        var hasGitignore = contents.Any(c => c.Name == ".gitignore");
        var hasLicense = contents.Any(c => c.Name.ToLower().Contains("license"));
        var hasDockerfile = contents.Any(c => c.Name.ToLower() == "dockerfile");
        var hasCI = contents.Any(c => c.Name == ".github" || c.Name == ".gitlab-ci.yml");

        if (hasReadme) score += 15;
        if (hasGitignore) score += 10;
        if (hasLicense) score += 10;
        if (hasDockerfile) score += 10;
        if (hasCI) score += 5;

        return Math.Min(score, 100);
    }

    private async Task<int> AnalyzeCodeCommenting(string repoFullName, List<ContentData> contents)
    {
        return 65; // Placeholder score
    }

    private async Task UpdateUserStatsAsync(int userId, GitHubAnalysisResult result)
    {
        try
        {
            var stats = await _context.GitHubAnalysisStats.FirstOrDefaultAsync(s => s.UserId == userId);

            if (stats == null)
            {
                stats = new GitHubAnalysisStats { UserId = userId, FirstAnalysis = result.AnalyzedAt };
                _context.GitHubAnalysisStats.Add(stats);
            }

            stats.TotalAnalyses++;
            stats.TotalRepositoriesAnalyzed += result.RepositoriesAnalyzed;
            stats.TotalCommitsAnalyzed += result.TotalCommits;
            stats.LastAnalysis = result.AnalyzedAt;

            var allUserAnalyses = await _context.GitHubAnalysisResults
                .Where(r => r.UserId == userId)
                .ToListAsync();

            // Add current analysis to the list for calculations
            allUserAnalyses.Add(result);

            stats.AverageOverallScore = allUserAnalyses.Average(a => a.OverallScore);
            stats.AverageCommitQuality = allUserAnalyses.Average(a => a.CommitMessageQuality);
            stats.AverageCommentingScore = allUserAnalyses.Average(a => a.CodeCommentingScore);
            stats.AverageNamingScore = allUserAnalyses.Average(a => a.VariableNamingScore);
            stats.AverageStructureScore = allUserAnalyses.Average(a => a.ProjectStructureScore);

            // Update streak
            var yesterday = DateTime.UtcNow.Date.AddDays(-1);
            var hasAnalysisYesterday = allUserAnalyses.Any(a => a.AnalyzedAt.Date == yesterday);

            if (hasAnalysisYesterday || stats.LastAnalysis?.Date == DateTime.UtcNow.Date)
            {
                stats.CurrentStreak++;
            }
            else
            {
                stats.CurrentStreak = 1;
            }

            if (stats.CurrentStreak > stats.LongestStreak)
            {
                stats.LongestStreak = stats.CurrentStreak;
            }

            _logger.LogDebug("Updated user stats for user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update user stats for user {UserId}", userId);
            throw;
        }
    }

    public async Task<GitHubAnalysisResponse?> GetAnalysisResultAsync(int analysisId, int userId)
    {
        try
        {
            var result = await _context.GitHubAnalysisResults
                .Where(r => r.Id == analysisId && r.UserId == userId)
                .FirstOrDefaultAsync();

            if (result == null) return null;

            return new GitHubAnalysisResponse
            {
                Id = result.Id,
                PersonalityType = result.PersonalityType,
                PersonalityDescription = result.PersonalityDescription,
                Strengths = JsonSerializer.Deserialize<List<string>>(result.StrengthsJson) ?? new(),
                Weaknesses = JsonSerializer.Deserialize<List<string>>(result.WeaknessesJson) ?? new(),
                CelebrityDevelopers = JsonSerializer.Deserialize<List<CelebrityDeveloper>>(result.CelebrityDevelopersJson) ?? new(),
                Scores = new AnalysisScores
                {
                    CommitMessageQuality = result.CommitMessageQuality,
                    CodeCommentingScore = result.CodeCommentingScore,
                    VariableNamingScore = result.VariableNamingScore,
                    ProjectStructureScore = result.ProjectStructureScore,
                    OverallScore = result.OverallScore
                },
                AnalyzedAt = result.AnalyzedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get analysis result {AnalysisId}", analysisId);
            throw;
        }
    }

    public async Task<List<GitHubAnalysisResponse>> GetUserAnalysisHistoryAsync(int userId, int page = 1, int pageSize = 10)
    {
        try
        {
            var results = await _context.GitHubAnalysisResults
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.AnalyzedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new GitHubAnalysisResponse
                {
                    Id = r.Id,
                    PersonalityType = r.PersonalityType,
                    PersonalityDescription = r.PersonalityDescription,
                    Scores = new AnalysisScores
                    {
                        CommitMessageQuality = r.CommitMessageQuality,
                        CodeCommentingScore = r.CodeCommentingScore,
                        VariableNamingScore = r.VariableNamingScore,
                        ProjectStructureScore = r.ProjectStructureScore,
                        OverallScore = r.OverallScore
                    },
                    AnalyzedAt = r.AnalyzedAt
                })
                .ToListAsync();

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get analysis history for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> DeleteAnalysisAsync(int analysisId, int userId)
    {
        try
        {
            var result = await _context.GitHubAnalysisResults
                .FirstOrDefaultAsync(r => r.Id == analysisId && r.UserId == userId);

            if (result == null) return false;

            _context.GitHubAnalysisResults.Remove(result);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete analysis {AnalysisId}", analysisId);
            throw;
        }
    }

    public async Task<string> GenerateShareableCardAsync(int analysisId)
    {
        try
        {
            return $"https://devlife-app.com/share/github-analysis/{analysisId}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate shareable card for analysis {AnalysisId}", analysisId);
            throw;
        }
    }

    public async Task<bool> ToggleFavoriteAsync(int analysisId, int userId)
    {
        try
        {
            var existingFavorite = await _context.GitHubAnalysisFavorites
                .FirstOrDefaultAsync(f => f.GitHubAnalysisResultId == analysisId && f.UserId == userId);

            if (existingFavorite != null)
            {
                _context.GitHubAnalysisFavorites.Remove(existingFavorite);
            }
            else
            {
                _context.GitHubAnalysisFavorites.Add(new GitHubAnalysisFavorite
                {
                    UserId = userId,
                    GitHubAnalysisResultId = analysisId
                });
            }

            await _context.SaveChangesAsync();
            return existingFavorite == null; // Return true if added, false if removed
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to toggle favorite for analysis {AnalysisId}", analysisId);
            throw;
        }
    }
}
public class GitHubRepositoryData
{
    public string Name { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Language { get; set; }
    public int StargazersCount { get; set; }
    public int ForksCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CommitData
{
    public CommitDetails? Commit { get; set; }
}

public class CommitDetails
{
    public string Message { get; set; } = string.Empty;
}

public class ContentData
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}

public class GitHubAnalysisData
{
    public string Username { get; set; } = string.Empty;
    public List<RepositoryAnalysis> Repositories { get; set; } = new();
}

public class RepositoryAnalysis
{
    public string Name { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PrimaryLanguage { get; set; } = string.Empty;
    public int StarsCount { get; set; }
    public int ForksCount { get; set; }
    public int CommitCount { get; set; }
    public int FileCount { get; set; }
    public int CommitQuality { get; set; }
    public int CommentingScore { get; set; }
    public int NamingScore { get; set; }
    public int StructureScore { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<CommitData> Commits { get; set; } = new();
}

public class PersonalityAnalysisResult
{
    public string PersonalityType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Strengths { get; set; } = new();
    public List<string> Weaknesses { get; set; } = new();
    public List<CelebrityDeveloper> CelebrityDevelopers { get; set; } = new();
    public AnalysisScores Scores { get; set; } = new();
}
