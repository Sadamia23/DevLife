using DevLife.Dtos;
using DevLife.Enums;

namespace DevLife.Services.Interfaces;

public interface IAIChallengeService
{
    Task<AIChallengeResponseDto?> GenerateChallengeAsync(
        TechnologyStack techStack,
        ExperienceLevel experienceLevel,
        string? specificTopic = null);

    Task<AIChallengeResponseDto?> GenerateDailyChallengeAsync();

    Task<bool> ValidateCodeAsync(string code, TechnologyStack techStack);
}
