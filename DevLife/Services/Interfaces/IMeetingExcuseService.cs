using DevLife.Dtos;

namespace DevLife.Services.Interfaces;

public interface IMeetingExcuseService
{
    // AI-only excuse generation methods
    Task<AIMeetingExcuseResponseDto?> GenerateAIExcuseAsync(AIMeetingExcuseRequestDto request);
    Task<List<AIMeetingExcuseResponseDto>> GenerateBulkAIExcusesAsync(AIMeetingExcuseRequestDto request, int count = 3);
}
