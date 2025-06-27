using DevLife.Dtos;

namespace DevLife.Services.Interfaces;

public interface IAICodeRoastService
{
    Task<AICodeTaskResponseDto?> GenerateCodeTaskAsync(AICodeTaskRequestDto request);
    Task<AICodeEvaluationResponseDto?> EvaluateCodeAsync(AICodeEvaluationRequestDto request);
}
