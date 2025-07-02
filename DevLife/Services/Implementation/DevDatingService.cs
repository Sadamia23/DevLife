using DevLife.Database;
using DevLife.Dtos;
using DevLife.Models.DevDating;
using DevLife.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileSystemGlobbing;

namespace DevLife.Services.Implementation;

public class DevDatingService : IDevDatingService
{
    private readonly ApplicationDbContext _context;
    private readonly IAIDatingService _aiDatingService;
    private readonly ILogger<DevDatingService> _logger;

    public DevDatingService(
        ApplicationDbContext context,
        IAIDatingService aiDatingService,
        ILogger<DevDatingService> logger)
    {
        _context = context;
        _aiDatingService = aiDatingService;
        _logger = logger;
    }

    public async Task<bool> SetupDatingProfileAsync(int userId, DatingSetupRequest request)
    {
        try
        {
            var existingProfile = await _context.DatingProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (existingProfile != null)
            {
                existingProfile.Gender = request.Gender;
                existingProfile.Preference = request.Preference;
                existingProfile.Bio = request.Bio;
                existingProfile.UpdatedAt = DateTime.UtcNow;
                existingProfile.IsActive = true;
            }
            else
            {
                var newProfile = new DatingProfile
                {
                    UserId = userId,
                    Gender = request.Gender,
                    Preference = request.Preference,
                    Bio = request.Bio,
                    IsActive = true
                };
                _context.DatingProfiles.Add(newProfile);
            }

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting up dating profile for user {UserId}", userId);
            return false;
        }
    }

    public async Task<IEnumerable<PotentialMatchDto>> GetPotentialMatchesAsync(int userId)
{
    try
    {
        _logger.LogInformation("🔍 Getting potential matches for user {UserId}", userId);

        var userProfile = await _context.DatingProfiles
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.UserId == userId && p.IsActive);

        if (userProfile == null)
        {
            _logger.LogWarning("❌ No active dating profile found for user {UserId}", userId);
            return Enumerable.Empty<PotentialMatchDto>();
        }

        _logger.LogInformation("✅ Found user profile: UserId={UserId}, Gender={Gender}, Preference={Preference}", 
            userProfile.UserId, userProfile.Gender, userProfile.Preference);

        // Get users already swiped
        var swipedUserIds = await _context.SwipeActions
            .Where(s => s.SwiperId == userId)
            .Select(s => s.SwipedUserId)
            .ToListAsync();

        _logger.LogInformation("📊 User has already swiped on {SwipeCount} profiles: [{SwipedIds}]", 
            swipedUserIds.Count, string.Join(", ", swipedUserIds));

        // Debug: Check all active dating profiles
        var allActiveProfiles = await _context.DatingProfiles
            .Include(p => p.User)
            .Where(p => p.IsActive && p.UserId != userId)
            .ToListAsync();

        _logger.LogInformation("👥 Total active profiles (excluding current user): {Count}", allActiveProfiles.Count);
        
        foreach (var profile in allActiveProfiles)
        {
            _logger.LogInformation("   Profile: UserId={UserId}, Username={Username}, Gender={Gender}, Preference={Preference}", 
                profile.UserId, profile.User.Username, profile.Gender, profile.Preference);
        }

        // Check gender matching
        var genderMatches = allActiveProfiles
            .Where(p => p.Gender == userProfile.Preference)
            .ToList();
        
        _logger.LogInformation("💕 Profiles matching user's preference ({Preference}): {Count}", 
            userProfile.Preference, genderMatches.Count);

        // Check mutual preference matching
        var mutualMatches = genderMatches
            .Where(p => p.Preference == userProfile.Gender)
            .ToList();
            
        _logger.LogInformation("💖 Profiles with mutual interest: {Count}", mutualMatches.Count);

        // Check not swiped
        var availableMatches = mutualMatches
            .Where(p => !swipedUserIds.Contains(p.UserId))
            .ToList();
            
        _logger.LogInformation("🎯 Available matches (not swiped): {Count}", availableMatches.Count);

        var potentialMatches = availableMatches
            .Take(10)
            .Select(p => new PotentialMatchDto
            {
                UserId = p.UserId,
                Username = p.User.Username,
                FirstName = p.User.FirstName,
                Age = DateTime.Now.Year - p.User.DateOfBirth.Year,
                TechStack = p.User.TechStack.ToString(),
                Experience = p.User.Experience.ToString(),
                Bio = p.Bio,
                ZodiacSign = p.User.ZodiacSign.ToString()
            })
            .ToList();

        _logger.LogInformation("✨ Returning {Count} potential matches", potentialMatches.Count);

        return potentialMatches;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "❌ Error getting potential matches for user {UserId}", userId);
        return Enumerable.Empty<PotentialMatchDto>();
    }
}

    public async Task<SwipeResponse> ProcessSwipeAsync(int userId, SwipeRequest request)
    {
        try
        {
            // Record the swipe
            var swipeAction = new SwipeAction
            {
                SwiperId = userId,
                SwipedUserId = request.TargetUserId,
                IsLike = request.IsLike
            };

            _context.SwipeActions.Add(swipeAction);

            // Update stats
            await UpdateDatingStatsAsync(userId, isSwipe: true, isLike: request.IsLike);

            var response = new SwipeResponse { IsMatch = false };

            if (request.IsLike)
            {
                // Check if there's a mutual like
                var mutualLike = await _context.SwipeActions
                    .FirstOrDefaultAsync(s => s.SwiperId == request.TargetUserId &&
                                            s.SwipedUserId == userId &&
                                            s.IsLike);

                if (mutualLike != null)
                {
                    // Create a match
                    var match = new Match
                    {
                        User1Id = Math.Min(userId, request.TargetUserId),
                        User2Id = Math.Max(userId, request.TargetUserId)
                    };

                    _context.Matches.Add(match);
                    await _context.SaveChangesAsync();

                    // Update match stats for both users
                    await UpdateDatingStatsAsync(userId, isMatch: true);
                    await UpdateDatingStatsAsync(request.TargetUserId, isMatch: true);

                    response.IsMatch = true;
                    response.MatchId = match.Id;
                    response.Message = "It's a match! 🎉";
                }
                else
                {
                    response.Message = "Like sent! 👍";
                }
            }
            else
            {
                response.Message = "Not a match 👎";
            }

            await _context.SaveChangesAsync();
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing swipe for user {UserId}", userId);
            return new SwipeResponse { Message = "Error processing swipe" };
        }
    }

    public async Task<IEnumerable<ChatMessageDto>> GetChatHistoryAsync(int userId, int matchId)
    {
        try
        {
            var match = await _context.Matches
                .FirstOrDefaultAsync(m => m.Id == matchId &&
                                        (m.User1Id == userId || m.User2Id == userId));

            if (match == null)
                return Enumerable.Empty<ChatMessageDto>();

            var messages = await _context.ChatMessages
                .Include(m => m.Sender)
                .Where(m => m.MatchId == matchId)
                .OrderBy(m => m.SentAt)
                .Select(m => new ChatMessageDto
                {
                    Id = m.Id,
                    SenderId = m.SenderId,
                    SenderUsername = m.Sender.Username,
                    Message = m.Message,
                    IsAIGenerated = m.IsAIGenerated,
                    SentAt = m.SentAt
                })
                .ToListAsync();

            return messages;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting chat history for match {MatchId}", matchId);
            return Enumerable.Empty<ChatMessageDto>();
        }
    }

    public async Task<SendMessageResponse> SendMessageAsync(int userId, int matchId, SendMessageRequest request)
    {
        try
        {
            var match = await _context.Matches
                .Include(m => m.User1)
                .Include(m => m.User2)
                .FirstOrDefaultAsync(m => m.Id == matchId &&
                                        (m.User1Id == userId || m.User2Id == userId));

            if (match == null)
                throw new InvalidOperationException("Match not found");

            // Save user message
            var userMessage = new ChatMessage
            {
                MatchId = matchId,
                SenderId = userId,
                Message = request.Message,
                IsAIGenerated = false
            };

            _context.ChatMessages.Add(userMessage);
            await _context.SaveChangesAsync();

            // Update stats
            await UpdateDatingStatsAsync(userId, isMessage: true);

            var userMessageDto = new ChatMessageDto
            {
                Id = userMessage.Id,
                SenderId = userMessage.SenderId,
                SenderUsername = userId == match.User1Id ? match.User1.Username : match.User2.Username,
                Message = userMessage.Message,
                IsAIGenerated = false,
                SentAt = userMessage.SentAt
            };

            // Generate AI response (simulate the other user)
            var otherUserId = userId == match.User1Id ? match.User2Id : match.User1Id;
            var otherUser = userId == match.User1Id ? match.User2 : match.User1;

            var personality = await _aiDatingService.GetPersonalityForUserAsync(otherUserId);
            var context = $"Tech Stack: {otherUser.TechStack}, Experience: {otherUser.Experience}";
            var aiResponse = await _aiDatingService.GenerateResponseAsync(request.Message, context, personality);

            ChatMessageDto? aiMessageDto = null;
            if (!string.IsNullOrEmpty(aiResponse))
            {
                var aiMessage = new ChatMessage
                {
                    MatchId = matchId,
                    SenderId = otherUserId,
                    Message = aiResponse,
                    IsAIGenerated = true
                };

                _context.ChatMessages.Add(aiMessage);
                await _context.SaveChangesAsync();

                aiMessageDto = new ChatMessageDto
                {
                    Id = aiMessage.Id,
                    SenderId = aiMessage.SenderId,
                    SenderUsername = otherUser.Username,
                    Message = aiMessage.Message,
                    IsAIGenerated = true,
                    SentAt = aiMessage.SentAt
                };
            }

            return new SendMessageResponse
            {
                UserMessage = userMessageDto,
                AIResponse = aiMessageDto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message for user {UserId} in match {MatchId}", userId, matchId);
            throw;
        }
    }

    public async Task<bool> HasDatingProfileAsync(int userId)
    {
        return await _context.DatingProfiles
            .AnyAsync(p => p.UserId == userId && p.IsActive);
    }

    public async Task<bool> IsValidMatchAsync(int userId, int matchId)
    {
        return await _context.Matches
            .AnyAsync(m => m.Id == matchId &&
                          (m.User1Id == userId || m.User2Id == userId) &&
                          m.IsActive);
    }

    private async Task UpdateDatingStatsAsync(int userId, bool isSwipe = false, bool isLike = false, bool isMatch = false, bool isMessage = false)
    {
        var stats = await _context.DatingStats
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (stats == null)
        {
            stats = new DatingStats { UserId = userId };
            _context.DatingStats.Add(stats);
        }

        if (isSwipe)
        {
            stats.TotalSwipes++;
            if (isLike) stats.TotalLikes++;
        }

        if (isMatch) stats.TotalMatches++;
        if (isMessage) stats.TotalMessages++;

        stats.LastActiveDate = DateTime.UtcNow;
        stats.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }
    public async Task<IEnumerable<MatchDto>> GetUserMatchesAsync(int userId)
{
    try
    {
        var matches = await _context.Matches
            .Include(m => m.User1)
            .Include(m => m.User2)
            .Where(m => (m.User1Id == userId || m.User2Id == userId) && m.IsActive)
            .OrderByDescending(m => m.MatchedAt)
            .ToListAsync();

        return matches.Select(m => new MatchDto
        {
            Id = m.Id,
            User1Id = m.User1Id,
            User2Id = m.User2Id,
            MatchedAt = m.MatchedAt,
            IsActive = m.IsActive,
            OtherUser = new MatchUser
            {
                Id = userId == m.User1Id ? m.User2.Id : m.User1.Id,
                Username = userId == m.User1Id ? m.User2.Username : m.User1.Username,
                FirstName = userId == m.User1Id ? m.User2.FirstName : m.User1.FirstName,
                LastName = userId == m.User1Id ? m.User2.LastName : m.User1.LastName,
                TechStack = userId == m.User1Id ? m.User2.TechStack : m.User1.TechStack,
                Experience = userId == m.User1Id ? m.User2.Experience : m.User1.Experience,
                ZodiacSign = userId == m.User1Id ? m.User2.ZodiacSign : m.User1.ZodiacSign
            }
        });
    }
    catch (Exception ex)
    {
        throw new Exception($"Error getting user matches: {ex.Message}");
    }
}

}
