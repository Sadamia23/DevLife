using System.ComponentModel.DataAnnotations;

namespace DevLife.Models.GitHubAnalysis;

public class GitHubAnalysisResult
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    [Required]
    [StringLength(200)]
    public string PersonalityType { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string PersonalityDescription { get; set; } = string.Empty;

    [Required]
    public string StrengthsJson { get; set; } = string.Empty; // JSON array of strings

    [Required]
    public string WeaknessesJson { get; set; } = string.Empty; // JSON array of strings

    [Required]
    public string CelebrityDevelopersJson { get; set; } = string.Empty; // JSON array of celebrity developer objects

    public int RepositoriesAnalyzed { get; set; }
    public int TotalCommits { get; set; }
    public int TotalFiles { get; set; }

    // Analysis scores (0-100)
    public int CommitMessageQuality { get; set; }
    public int CodeCommentingScore { get; set; }
    public int VariableNamingScore { get; set; }
    public int ProjectStructureScore { get; set; }
    public int OverallScore { get; set; }

    [Required]
    public string AnalysisDetailsJson { get; set; } = string.Empty; // Detailed analysis data

    [Required]
    [StringLength(100)]
    public string GitHubUsername { get; set; } = string.Empty;

    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SharedAt { get; set; }
    public int ShareCount { get; set; } = 0;

    public bool IsPublic { get; set; } = false;

    // Navigation properties
    public ICollection<GitHubAnalysisFavorite> Favorites { get; set; } = new List<GitHubAnalysisFavorite>();
}
