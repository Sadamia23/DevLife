using DevLife.Enums;

namespace DevLife.Dtos;

public class AIMeetingExcuseRequestDto
{
    public MeetingCategory Category { get; set; }
    public ExcuseType Type { get; set; }
    public int? TargetBelievability { get; set; } // 1-10 scale
    public string? UserTechStack { get; set; }
    public string? UserExperience { get; set; }
    public string? Context { get; set; } // Optional context like "urgent client demo"
    public string? Mood { get; set; } // "funny", "desperate", "professional"
    public List<string>? AvoidKeywords { get; set; } // Words to avoid in the excuse
}

public class AIMeetingExcuseResponseDto
{
    public string ExcuseText { get; set; } = string.Empty;
    public MeetingCategory Category { get; set; }
    public ExcuseType Type { get; set; }
    public int BelievabilityScore { get; set; }
    public string Reasoning { get; set; } = string.Empty; // Why this excuse works
    public List<string> Tags { get; set; } = new();
    public string TechStackUsed { get; set; } = string.Empty;
    public int HumorLevel { get; set; } // 1-10 how funny it is
    public string Usage { get; set; } = string.Empty; // Best way to use this excuse
    public bool IsAIGenerated { get; set; } = true;
}
