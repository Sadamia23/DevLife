using DevLife.Enums;
using System.ComponentModel.DataAnnotations;

public class AIMeetingExcuseRequestDto
{
    [Required]
    public MeetingCategory Category { get; set; }

    [Required]
    public ExcuseType Type { get; set; }

    [Range(1, 10)]
    public int? TargetBelievability { get; set; }

    [StringLength(100)]
    public string? UserTechStack { get; set; }

    [StringLength(50)]
    public string? UserExperience { get; set; }

    [StringLength(1000)]
    public string? Context { get; set; }

    [StringLength(50)]
    public string? Mood { get; set; }

    public List<string>? AvoidKeywords { get; set; }
}

public class AIMeetingExcuseResponseDto
{
    public string ExcuseText { get; set; } = string.Empty;
    public MeetingCategory Category { get; set; }
    public ExcuseType Type { get; set; }
    public int BelievabilityScore { get; set; }
    public string Reasoning { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public string TechStackUsed { get; set; } = string.Empty;
    public int HumorLevel { get; set; }
    public string Usage { get; set; } = string.Empty;
    public bool IsAIGenerated { get; set; } = true;
}