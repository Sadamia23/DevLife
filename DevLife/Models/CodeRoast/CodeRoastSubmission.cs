using DevLife.Dtos;
using DevLife.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DevLife.Models.CodeRoast;

public class CodeRoastSubmission
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }
    public virtual User User { get; set; } = null!;

    [Required]
    public int TaskId { get; set; }
    public virtual CodeRoastTask Task { get; set; } = null!;

    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string SubmittedCode { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? UserNotes { get; set; }

    // AI Evaluation Results
    [Range(0, 100)]
    public int OverallScore { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string RoastMessage { get; set; } = string.Empty;

    [Column(TypeName = "nvarchar(max)")]
    public string TechnicalFeedback { get; set; } = string.Empty;

    // Quality Assessment Scores
    [Range(0, 100)]
    public int ReadabilityScore { get; set; }

    [Range(0, 100)]
    public int PerformanceScore { get; set; }

    [Range(0, 100)]
    public int CorrectnessScore { get; set; }

    [Range(0, 100)]
    public int BestPracticesScore { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string PositivePointsJson { get; set; } = "[]"; // JSON array

    [Column(TypeName = "nvarchar(max)")]
    public string ImprovementPointsJson { get; set; } = "[]"; // JSON array

    [Column(TypeName = "nvarchar(max)")]
    public string RedFlagsJson { get; set; } = "[]"; // JSON array

    [MaxLength(50)]
    public string CodeStyle { get; set; } = "Unknown"; // "Clean", "Messy", "Chaotic", etc.

    [Column(TypeName = "nvarchar(max)")]
    public string DetectedPatternsJson { get; set; } = "[]"; // JSON array

    [Column(TypeName = "nvarchar(max)")]
    public string CodeSmellsJson { get; set; } = "[]"; // JSON array

    // Metadata
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime EvaluatedAt { get; set; } = DateTime.UtcNow;

    [Range(0, 1440)] // 0 to 24 hours in minutes
    public int TimeSpentMinutes { get; set; } = 0;

    public RoastSeverity RoastSeverity { get; set; } = RoastSeverity.Medium;

    // Computed properties
    public bool IsRoasted => OverallScore < 70;
    public bool IsPraised => OverallScore >= 70;
    public bool IsPerfect => OverallScore >= 95;
}