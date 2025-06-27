using DevLife.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DevLife.Models.CodeRoast;

public class CodeRoastTask
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Requirements { get; set; } = string.Empty;

    [Required]
    public TechnologyStack TechStack { get; set; }

    [Required]
    public ExperienceLevel DifficultyLevel { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string? StarterCode { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string TestCasesJson { get; set; } = "[]"; // JSON array of test cases

    [Column(TypeName = "nvarchar(max)")]
    public string ExamplesJson { get; set; } = "[]"; // JSON array of examples

    [Range(1, 480)] // 1 minute to 8 hours
    public int EstimatedMinutes { get; set; } = 30;

    [MaxLength(100)]
    public string? Topic { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public bool IsAIGenerated { get; set; } = false;

    // Navigation properties
    public virtual ICollection<CodeRoastSubmission> Submissions { get; set; } = new List<CodeRoastSubmission>();
}