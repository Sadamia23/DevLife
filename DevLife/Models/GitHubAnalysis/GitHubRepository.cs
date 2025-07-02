using System.ComponentModel.DataAnnotations;

namespace DevLife.Models.GitHubAnalysis;

public class GitHubRepository
{
    public int Id { get; set; }

    [Required]
    public int AnalysisResultId { get; set; }
    public GitHubAnalysisResult AnalysisResult { get; set; } = null!;

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string FullName { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string PrimaryLanguage { get; set; } = string.Empty;

    public int StarsCount { get; set; }
    public int ForksCount { get; set; }
    public int CommitsAnalyzed { get; set; }
    public int FilesAnalyzed { get; set; }

    // Repository-specific scores
    public int RepoCommitQuality { get; set; }
    public int RepoCommentingScore { get; set; }
    public int RepoNamingScore { get; set; }
    public int RepoStructureScore { get; set; }

    [Required]
    public string RepositoryDetailsJson { get; set; } = string.Empty; // Additional repository data

    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdated { get; set; }
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
}
