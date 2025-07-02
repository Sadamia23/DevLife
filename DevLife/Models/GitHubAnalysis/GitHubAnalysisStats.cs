using System.ComponentModel.DataAnnotations;

namespace DevLife.Models.GitHubAnalysis;

public class GitHubAnalysisStats
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int TotalAnalyses { get; set; } = 0;
    public int TotalRepositoriesAnalyzed { get; set; } = 0;
    public int TotalCommitsAnalyzed { get; set; } = 0;

    public double AverageOverallScore { get; set; } = 0.0;
    public double AverageCommitQuality { get; set; } = 0.0;
    public double AverageCommentingScore { get; set; } = 0.0;
    public double AverageNamingScore { get; set; } = 0.0;
    public double AverageStructureScore { get; set; } = 0.0;

    [StringLength(200)]
    public string MostCommonPersonalityType { get; set; } = string.Empty;

    [StringLength(100)]
    public string FavoriteLanguage { get; set; } = string.Empty;

    public string RecentAnalysesJson { get; set; } = string.Empty; // Last 10 analyses summary
    public string UnlockedAchievementsJson { get; set; } = string.Empty; // Achievement system

    public DateTime? LastAnalysis { get; set; }
    public DateTime? FirstAnalysis { get; set; }

    public int CurrentStreak { get; set; } = 0; // Days with analyses
    public int LongestStreak { get; set; } = 0;

    public int ShareCount { get; set; } = 0;
    public int FavoriteCount { get; set; } = 0;
}
