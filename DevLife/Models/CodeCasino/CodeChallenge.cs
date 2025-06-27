using DevLife.Enums;
using System.ComponentModel.DataAnnotations;

namespace DevLife.Models.CodeCasino;

public class CodeChallenge
{
    public int Id { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public TechnologyStack TechStack { get; set; }

    [Required]
    public ExperienceLevel DifficultyLevel { get; set; }

    [Required]
    public string CorrectCode { get; set; } = string.Empty;

    [Required]
    public string BuggyCode { get; set; } = string.Empty;

    [Required]
    public string Explanation { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;
}
