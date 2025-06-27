using DevLife.Dtos;

namespace DevLife.Services.Interfaces;

public interface IAIMeetingExcuseService
{
    Task<AIMeetingExcuseResponseDto?> GenerateExcuseAsync(AIMeetingExcuseRequestDto request);

    Task<List<AIMeetingExcuseResponseDto>> GenerateBulkExcusesAsync(AIMeetingExcuseRequestDto request, int count = 3);

    Task<AIMeetingExcuseResponseDto?> GeneratePersonalizedExcuseAsync(string username, AIMeetingExcuseRequestDto request);
}
