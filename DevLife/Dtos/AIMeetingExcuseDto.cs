using DevLife.Enums;

namespace DevLife.Dtos;

public class AIMeetingExcuseRequestDto
{
    public MeetingCategory Category { get; set; }
    public ExcuseType Type { get; set; }
    public int? TargetBelievability { get; set; }
    public string? UserTechStack { get; set; }
    public string? UserExperience { get; set; }
    public string? Context { get; set; }
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
