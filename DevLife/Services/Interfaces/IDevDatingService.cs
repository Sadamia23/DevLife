using DevLife.Dtos;
using DevLife.Models.DevDating;

namespace DevLife.Services.Interfaces;

public interface IDevDatingService
{
    Task<bool> SetupDatingProfileAsync(int userId, DatingSetupRequest request);
    Task<IEnumerable<PotentialMatchDto>> GetPotentialMatchesAsync(int userId);
    Task<SwipeResponse> ProcessSwipeAsync(int userId, SwipeRequest request);
    Task<IEnumerable<ChatMessageDto>> GetChatHistoryAsync(int userId, int matchId);
    Task<SendMessageResponse> SendMessageAsync(int userId, int matchId, SendMessageRequest request);
    Task<bool> HasDatingProfileAsync(int userId);
    Task<bool> IsValidMatchAsync(int userId, int matchId);
    Task<IEnumerable<MatchDto>> GetUserMatchesAsync(int userId);
}
