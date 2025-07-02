using System.ComponentModel.DataAnnotations;

namespace DevLife.Dtos;

public class GitHubAnalysisRequest
{
    [Required]
    [StringLength(100)]
    public string GitHubUsername { get; set; } = string.Empty;

    public int MaxRepositories { get; set; } = 5; // Limit repositories to analyze
    public bool IncludeForkedRepos { get; set; } = false;
    public bool AnalyzePrivateRepos { get; set; } = false;
}

public class GitHubAnalysisResponse
{
    public int Id { get; set; }
    public string PersonalityType { get; set; } = string.Empty;
    public string PersonalityDescription { get; set; } = string.Empty;
    public List<string> Strengths { get; set; } = new();
    public List<string> Weaknesses { get; set; } = new();
    public List<CelebrityDeveloper> CelebrityDevelopers { get; set; } = new();
    public AnalysisScores Scores { get; set; } = new();
    public List<RepositorySummary> RepositoriesAnalyzed { get; set; } = new();
    public string ShareableCardUrl { get; set; } = string.Empty;
    public DateTime AnalyzedAt { get; set; }
}

public class CelebrityDeveloper
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string GitHubUsername { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public int SimilarityScore { get; set; } // 0-100
}

public class AnalysisScores
{
    public int CommitMessageQuality { get; set; }
    public int CodeCommentingScore { get; set; }
    public int VariableNamingScore { get; set; }
    public int ProjectStructureScore { get; set; }
    public int OverallScore { get; set; }
}

public class RepositorySummary
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PrimaryLanguage { get; set; } = string.Empty;
    public int StarsCount { get; set; }
    public int CommitsAnalyzed { get; set; }
    public AnalysisScores Scores { get; set; } = new();
}
