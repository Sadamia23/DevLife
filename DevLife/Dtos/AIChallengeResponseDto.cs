using DevLife.Enums;

namespace DevLife.Dtos;

public class AIChallengeResponseDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CorrectCode { get; set; } = string.Empty;
    public string BuggyCode { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public TechnologyStack TechStack { get; set; }
    public ExperienceLevel DifficultyLevel { get; set; }
    public string Topic { get; set; } = string.Empty;
}
