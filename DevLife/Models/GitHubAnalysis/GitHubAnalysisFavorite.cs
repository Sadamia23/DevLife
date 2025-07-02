using System.ComponentModel.DataAnnotations;

namespace DevLife.Models.GitHubAnalysis;

public class GitHubAnalysisFavorite
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    [Required]
    public int GitHubAnalysisResultId { get; set; }
    public GitHubAnalysisResult GitHubAnalysisResult { get; set; } = null!;

    [StringLength(200)]
    public string? CustomName { get; set; } = string.Empty; // Made nullable to match DB config

    public DateTime SavedAt { get; set; } = DateTime.UtcNow;
}
