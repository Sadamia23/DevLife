using DevLife.Database;
using DevLife.Dtos;
using DevLife.Enums;
using DevLife.Models.MeetingExcuse;
using DevLife.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace DevLife.Services.Implementation;

public class MeetingExcuseService : IMeetingExcuseService
{
    private readonly IAIMeetingExcuseService? _aiService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<MeetingExcuseService> _logger;

    public MeetingExcuseService(
        ApplicationDbContext context,
        ILogger<MeetingExcuseService> logger,
        IAIMeetingExcuseService? aiService = null) // Optional dependency
    {
        _context = context;
        _logger = logger;
        _aiService = aiService;
    }
    public async Task<MeetingExcuseDto> GenerateRandomExcuseAsync(string username, GenerateExcuseRequestDto? criteria = null)
    {
        try
        {
            var user = await GetUserByUsernameAsync(username);
            if (user == null)
                throw new InvalidOperationException("User not found");

            // Ensure user stats exist
            await InitializeUserStatsAsync(user.Id);

            // Build query based on criteria
            var query = _context.MeetingExcuses.Where(e => e.IsActive);

            if (criteria != null)
            {
                if (criteria.Category.HasValue)
                    query = query.Where(e => e.Category == criteria.Category.Value);

                if (criteria.Type.HasValue)
                    query = query.Where(e => e.Type == criteria.Type.Value);

                if (criteria.MinBelievability.HasValue)
                    query = query.Where(e => e.BelievabilityScore >= criteria.MinBelievability.Value);

                if (criteria.MaxBelievability.HasValue)
                    query = query.Where(e => e.BelievabilityScore <= criteria.MaxBelievability.Value);

                if (criteria.ExcludeUsed)
                {
                    var usedExcuseIds = await _context.MeetingExcuseUsages
                        .Where(u => u.UserId == user.Id)
                        .Select(u => u.MeetingExcuseId)
                        .ToListAsync();

                    query = query.Where(e => !usedExcuseIds.Contains(e.Id));
                }

                if (criteria.Tags?.Any() == true)
                {
                    foreach (var tag in criteria.Tags)
                    {
                        query = query.Where(e => e.TagsJson.Contains($"\"{tag}\""));
                    }
                }
            }

            var excuses = await query.ToListAsync();

            if (!excuses.Any())
            {
                // Fallback to any active excuse if no matches
                excuses = await _context.MeetingExcuses.Where(e => e.IsActive).ToListAsync();
            }

            if (!excuses.Any())
                throw new InvalidOperationException("No excuses available");

            // Select random excuse
            var random = new Random();
            var selectedExcuse = excuses[random.Next(excuses.Count)];

            // Update usage count
            selectedExcuse.UsageCount++;
            await _context.SaveChangesAsync();

            // Update user stats
            await UpdateUserStatsAsync(user.Id, selectedExcuse);

            return await MapToExcuseDtoAsync(selectedExcuse, user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating random excuse for user {Username}", username);
            throw;
        }
    }

    public async Task<List<MeetingExcuseDto>> GenerateBulkExcusesAsync(string username, BulkExcuseGenerationDto request)
    {
        var result = new List<MeetingExcuseDto>();

        for (int i = 0; i < request.Count; i++)
        {
            var excuse = await GenerateRandomExcuseAsync(username, request.Criteria);
            result.Add(excuse);
        }

        return result;
    }

    public async Task<MeetingExcuseDto?> GetExcuseOfTheDayAsync()
    {
        try
        {
            // Use the date as seed for consistent daily excuse
            var today = DateTime.UtcNow.Date;
            var seed = today.GetHashCode();
            var random = new Random(seed);

            var excuses = await _context.MeetingExcuses
                .Where(e => e.IsActive)
                .ToListAsync();

            if (!excuses.Any())
                return null;

            var dailyExcuse = excuses[random.Next(excuses.Count)];
            return await MapToExcuseDtoAsync(dailyExcuse, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting excuse of the day");
            return null;
        }
    }

    public async Task<MeetingExcuseFavoriteDto> SaveFavoriteAsync(string username, SaveFavoriteRequestDto request)
    {
        try
        {
            var user = await GetUserByUsernameAsync(username);
            if (user == null)
                throw new InvalidOperationException("User not found");

            var excuse = await _context.MeetingExcuses.FindAsync(request.MeetingExcuseId);
            if (excuse == null)
                throw new InvalidOperationException("Excuse not found");

            // Check if already favorited
            var existingFavorite = await _context.MeetingExcuseFavorites
                .FirstOrDefaultAsync(f => f.UserId == user.Id && f.MeetingExcuseId == request.MeetingExcuseId);

            if (existingFavorite != null)
                throw new InvalidOperationException("Excuse already in favorites");

            var favorite = new MeetingExcuseFavorite
            {
                UserId = user.Id,
                MeetingExcuseId = request.MeetingExcuseId,
                CustomName = request.CustomName,
                UserRating = request.UserRating,
                SavedAt = DateTime.UtcNow
            };

            _context.MeetingExcuseFavorites.Add(favorite);

            // Update user stats
            var userStats = await GetOrCreateUserStatsAsync(user.Id);
            userStats.TotalFavorites++;
            userStats.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new MeetingExcuseFavoriteDto
            {
                Id = favorite.Id,
                MeetingExcuseId = favorite.MeetingExcuseId,
                CustomName = favorite.CustomName,
                UserRating = favorite.UserRating,
                SavedAt = favorite.SavedAt,
                Excuse = await MapToExcuseDtoAsync(excuse, user.Id)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving favorite for user {Username}", username);
            throw;
        }
    }

    public async Task<bool> RemoveFavoriteAsync(string username, int favoriteId)
    {
        try
        {
            var user = await GetUserByUsernameAsync(username);
            if (user == null)
                return false;

            var favorite = await _context.MeetingExcuseFavorites
                .FirstOrDefaultAsync(f => f.Id == favoriteId && f.UserId == user.Id);

            if (favorite == null)
                return false;

            _context.MeetingExcuseFavorites.Remove(favorite);

            // Update user stats
            var userStats = await GetOrCreateUserStatsAsync(user.Id);
            userStats.TotalFavorites = Math.Max(0, userStats.TotalFavorites - 1);
            userStats.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing favorite {FavoriteId} for user {Username}", favoriteId, username);
            return false;
        }
    }

    public async Task<List<MeetingExcuseFavoriteDto>> GetUserFavoritesAsync(string username, int limit = 20)
    {
        try
        {
            var user = await GetUserByUsernameAsync(username);
            if (user == null)
                return new List<MeetingExcuseFavoriteDto>();

            var favorites = await _context.MeetingExcuseFavorites
                .Include(f => f.MeetingExcuse)
                .Where(f => f.UserId == user.Id)
                .OrderByDescending(f => f.SavedAt)
                .Take(limit)
                .ToListAsync();

            var result = new List<MeetingExcuseFavoriteDto>();
            foreach (var favorite in favorites)
            {
                result.Add(new MeetingExcuseFavoriteDto
                {
                    Id = favorite.Id,
                    MeetingExcuseId = favorite.MeetingExcuseId,
                    CustomName = favorite.CustomName,
                    UserRating = favorite.UserRating,
                    SavedAt = favorite.SavedAt,
                    Excuse = await MapToExcuseDtoAsync(favorite.MeetingExcuse, user.Id)
                });
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting favorites for user {Username}", username);
            return new List<MeetingExcuseFavoriteDto>();
        }
    }

    public async Task<MeetingExcuseUsageDto> SubmitUsageAsync(string username, SubmitUsageRequestDto request)
    {
        try
        {
            var user = await GetUserByUsernameAsync(username);
            if (user == null)
                throw new InvalidOperationException("User not found");

            var excuse = await _context.MeetingExcuses.FindAsync(request.MeetingExcuseId);
            if (excuse == null)
                throw new InvalidOperationException("Excuse not found");

            var usage = new MeetingExcuseUsage
            {
                UserId = user.Id,
                MeetingExcuseId = request.MeetingExcuseId,
                Context = request.Context,
                WasSuccessful = request.WasSuccessful,
                UsedAt = DateTime.UtcNow
            };

            _context.MeetingExcuseUsages.Add(usage);
            await _context.SaveChangesAsync();

            return new MeetingExcuseUsageDto
            {
                Id = usage.Id,
                MeetingExcuseId = usage.MeetingExcuseId,
                Context = usage.Context,
                WasSuccessful = usage.WasSuccessful,
                UsedAt = usage.UsedAt,
                Excuse = await MapToExcuseDtoAsync(excuse, user.Id)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting usage for user {Username}", username);
            throw;
        }
    }

    public async Task<List<MeetingExcuseUsageDto>> GetUserUsageHistoryAsync(string username, int limit = 20)
    {
        try
        {
            var user = await GetUserByUsernameAsync(username);
            if (user == null)
                return new List<MeetingExcuseUsageDto>();

            var usages = await _context.MeetingExcuseUsages
                .Include(u => u.MeetingExcuse)
                .Where(u => u.UserId == user.Id)
                .OrderByDescending(u => u.UsedAt)
                .Take(limit)
                .ToListAsync();

            var result = new List<MeetingExcuseUsageDto>();
            foreach (var usage in usages)
            {
                result.Add(new MeetingExcuseUsageDto
                {
                    Id = usage.Id,
                    MeetingExcuseId = usage.MeetingExcuseId,
                    Context = usage.Context,
                    WasSuccessful = usage.WasSuccessful,
                    UsedAt = usage.UsedAt,
                    Excuse = await MapToExcuseDtoAsync(usage.MeetingExcuse, user.Id)
                });
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting usage history for user {Username}", username);
            return new List<MeetingExcuseUsageDto>();
        }
    }

    public async Task<MeetingExcuseDashboardDto> GetDashboardAsync(string username)
    {
        try
        {
            var user = await GetUserByUsernameAsync(username);
            if (user == null)
                throw new InvalidOperationException("User not found");

            await InitializeUserStatsAsync(user.Id);

            var userStats = await GetUserStatsAsync(username);
            var recentFavorites = await GetUserFavoritesAsync(username, 5);
            var recentUsage = await GetUserUsageHistoryAsync(username, 5);
            var excuseOfTheDay = await GetExcuseOfTheDayAsync();
            var leaderboard = await GetLeaderboardAsync(5);
            var trending = await GetTrendingExcusesAsync(5);

            return new MeetingExcuseDashboardDto
            {
                UserStats = userStats,
                ExcuseOfTheDay = excuseOfTheDay,
                RecentFavorites = recentFavorites,
                RecentUsage = recentUsage,
                TopUsersThisWeek = leaderboard,
                TrendingExcuses = trending
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard for user {Username}", username);
            throw;
        }
    }

    public async Task<MeetingExcuseStatsDto> GetUserStatsAsync(string username)
    {
        try
        {
            var user = await GetUserByUsernameAsync(username);
            if (user == null)
                throw new InvalidOperationException("User not found");

            var stats = await GetOrCreateUserStatsAsync(user.Id);

            return new MeetingExcuseStatsDto
            {
                UserId = user.Id,
                TotalExcusesGenerated = stats.TotalExcusesGenerated,
                TotalFavorites = stats.TotalFavorites,
                FavoriteCategory = stats.FavoriteCategory,
                FavoriteType = stats.FavoriteType,
                AverageBelievability = stats.AverageBelievability,
                CurrentStreak = stats.CurrentStreak,
                LongestStreak = stats.LongestStreak,
                UnlockedAchievements = JsonSerializer.Deserialize<List<string>>(stats.UnlockedAchievementsJson) ?? new(),
                LastExcuseGenerated = stats.LastExcuseGenerated
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user stats for {Username}", username);
            throw;
        }
    }

    public async Task<List<MeetingExcuseLeaderboardEntryDto>> GetLeaderboardAsync(int limit = 10)
    {
        try
        {
            var entries = await _context.MeetingExcuseStats
                .Include(s => s.User)
                .OrderByDescending(s => s.TotalExcusesGenerated)
                .ThenByDescending(s => s.CurrentStreak)
                .Take(limit)
                .ToListAsync();

            return entries.Select((entry, index) => new MeetingExcuseLeaderboardEntryDto
            {
                Username = entry.User.Username,
                TotalExcusesGenerated = entry.TotalExcusesGenerated,
                CurrentStreak = entry.CurrentStreak,
                LongestStreak = entry.LongestStreak,
                AverageBelievability = entry.AverageBelievability,
                Position = index + 1
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting leaderboard");
            throw;
        }
    }

    public async Task<bool> RateExcuseAsync(string username, RateExcuseRequestDto request)
    {
        try
        {
            var user = await GetUserByUsernameAsync(username);
            if (user == null)
                return false;

            var excuse = await _context.MeetingExcuses.FindAsync(request.MeetingExcuseId);
            if (excuse == null)
                return false;

            // Update average rating
            var totalRating = excuse.AverageRating * excuse.RatingCount + request.Rating;
            excuse.RatingCount++;
            excuse.AverageRating = totalRating / excuse.RatingCount;

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rating excuse for user {Username}", username);
            return false;
        }
    }

    public async Task<List<MeetingExcuseDto>> GetTrendingExcusesAsync(int limit = 10)
    {
        try
        {
            var weekAgo = DateTime.UtcNow.AddDays(-7);

            var trendingIds = await _context.MeetingExcuseUsages
                .Where(u => u.UsedAt >= weekAgo)
                .GroupBy(u => u.MeetingExcuseId)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .Take(limit)
                .ToListAsync();

            var excuses = await _context.MeetingExcuses
                .Where(e => trendingIds.Contains(e.Id))
                .ToListAsync();

            var result = new List<MeetingExcuseDto>();
            foreach (var excuse in excuses)
            {
                result.Add(await MapToExcuseDtoAsync(excuse, null));
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting trending excuses");
            return new List<MeetingExcuseDto>();
        }
    }

    public async Task<List<MeetingExcuseDto>> GetTopRatedExcusesAsync(int limit = 10)
    {
        try
        {
            var excuses = await _context.MeetingExcuses
                .Where(e => e.IsActive && e.RatingCount > 0)
                .OrderByDescending(e => e.AverageRating)
                .ThenByDescending(e => e.RatingCount)
                .Take(limit)
                .ToListAsync();

            var result = new List<MeetingExcuseDto>();
            foreach (var excuse in excuses)
            {
                result.Add(await MapToExcuseDtoAsync(excuse, null));
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top rated excuses");
            return new List<MeetingExcuseDto>();
        }
    }

    public async Task<ExcuseAnalyticsDto> GetAnalyticsAsync()
    {
        try
        {
            var categoryUsage = await _context.MeetingExcuseUsages
                .GroupBy(u => u.MeetingExcuse.Category)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Category, x => x.Count);

            var typeUsage = await _context.MeetingExcuseUsages
                .GroupBy(u => u.MeetingExcuse.Type)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Type, x => x.Count);

            var believabilityDist = await _context.MeetingExcuseUsages
                .GroupBy(u => u.MeetingExcuse.BelievabilityScore)
                .Select(g => new { Score = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Score, x => x.Count);

            var mostPopular = await GetTrendingExcusesAsync(5);
            var topRated = await GetTopRatedExcusesAsync(5);

            var avgRating = await _context.MeetingExcuses
                .Where(e => e.RatingCount > 0)
                .AverageAsync(e => e.AverageRating);

            var totalExcuses = await _context.MeetingExcuses.CountAsync();
            var totalUsage = await _context.MeetingExcuseUsages.CountAsync();

            return new ExcuseAnalyticsDto
            {
                CategoryUsage = categoryUsage,
                TypeUsage = typeUsage,
                BelievabilityDistribution = believabilityDist,
                MostPopularExcuses = mostPopular,
                HighestRatedExcuses = topRated,
                AverageRatingAcrossAllExcuses = avgRating,
                TotalExcusesInDatabase = totalExcuses,
                TotalUsageCount = totalUsage
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting analytics");
            throw;
        }
    }

    public async Task InitializeUserStatsAsync(int userId)
    {
        try
        {
            var existingStats = await _context.MeetingExcuseStats
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (existingStats == null)
            {
                var stats = new MeetingExcuseStats
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.MeetingExcuseStats.Add(stats);
                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing user stats for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> CheckAndUpdateStreakAsync(int userId)
    {
        try
        {
            var stats = await GetOrCreateUserStatsAsync(userId);
            var today = DateTime.UtcNow.Date;
            var lastUsageDate = stats.LastExcuseGenerated.Date;

            if (lastUsageDate == today)
            {
                // Already used today, no change
                return false;
            }
            else if (lastUsageDate == today.AddDays(-1))
            {
                // Used yesterday, continue streak
                stats.CurrentStreak++;
                if (stats.CurrentStreak > stats.LongestStreak)
                    stats.LongestStreak = stats.CurrentStreak;
            }
            else if (lastUsageDate < today.AddDays(-1))
            {
                // Gap in usage, reset streak
                stats.CurrentStreak = 1;
            }

            stats.LastExcuseGenerated = DateTime.UtcNow;
            stats.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking and updating streak for user {UserId}", userId);
            return false;
        }
    }

    public async Task<List<string>> GetAvailableTagsAsync()
    {
        try
        {
            var allTags = await _context.MeetingExcuses
                .Where(e => e.IsActive && !string.IsNullOrEmpty(e.TagsJson))
                .Select(e => e.TagsJson)
                .ToListAsync();

            var uniqueTags = new HashSet<string>();
            foreach (var tagJson in allTags)
            {
                try
                {
                    var tags = JsonSerializer.Deserialize<List<string>>(tagJson);
                    if (tags != null)
                    {
                        foreach (var tag in tags)
                            uniqueTags.Add(tag);
                    }
                }
                catch { /* Ignore invalid JSON */ }
            }

            return uniqueTags.OrderBy(t => t).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available tags");
            return new List<string>();
        }
    }
    public async Task<MeetingExcuseDto> GenerateAIExcuseAsync(string username, GenerateExcuseRequestDto? criteria = null)
    {
        try
        {
            if (_aiService == null)
            {
                _logger.LogWarning("AI service not available, falling back to database excuses");
                return await GenerateRandomExcuseAsync(username, criteria);
            }

            var user = await GetUserByUsernameAsync(username);
            if (user == null)
                throw new InvalidOperationException("User not found");

            // Build AI request
            var aiRequest = new AIMeetingExcuseRequestDto
            {
                Category = criteria?.Category ?? GetRandomCategory(),
                Type = criteria?.Type ?? GetRandomType(),
                TargetBelievability = criteria?.MinBelievability ?? 7,
                UserTechStack = user.TechStack.ToString(),
                UserExperience = user.Experience.ToString(),
                Context = "Generate a fresh, creative excuse",
                Mood = "funny"
            };

            var aiExcuse = await _aiService.GenerateExcuseAsync(aiRequest);

            if (aiExcuse != null)
            {
                // Convert AI response to standard DTO
                var excuseDto = new MeetingExcuseDto
                {
                    Id = 0, // AI-generated excuses don't have DB IDs
                    ExcuseText = aiExcuse.ExcuseText,
                    Category = aiExcuse.Category,
                    Type = aiExcuse.Type,
                    BelievabilityScore = aiExcuse.BelievabilityScore,
                    Tags = aiExcuse.Tags,
                    UsageCount = 0,
                    AverageRating = 0,
                    RatingCount = 0,
                    IsFavorite = false,
                    CreatedAt = DateTime.UtcNow
                };

                // Update user stats
                await UpdateUserStatsForAIExcuse(user.Id, aiExcuse);

                return excuseDto;
            }

            // Fallback to database if AI fails
            _logger.LogWarning("AI excuse generation failed, falling back to database");
            return await GenerateRandomExcuseAsync(username, criteria);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating AI excuse for user {Username}", username);
            return await GenerateRandomExcuseAsync(username, criteria);
        }
    }

    public async Task<List<MeetingExcuseDto>> GenerateAIBulkExcusesAsync(string username, BulkExcuseGenerationDto request)
    {
        try
        {
            if (_aiService == null)
            {
                return await GenerateBulkExcusesAsync(username, request);
            }

            var user = await GetUserByUsernameAsync(username);
            if (user == null)
                throw new InvalidOperationException("User not found");

            var aiRequest = new AIMeetingExcuseRequestDto
            {
                Category = request.Criteria.Category ?? GetRandomCategory(),
                Type = request.Criteria.Type ?? GetRandomType(),
                TargetBelievability = request.Criteria.MinBelievability ?? 7,
                UserTechStack = user.TechStack.ToString(),
                UserExperience = user.Experience.ToString(),
                Context = "Generate multiple creative excuses with variety",
                Mood = "funny"
            };

            var aiExcuses = await _aiService.GenerateBulkExcusesAsync(aiRequest, request.Count);
            var result = new List<MeetingExcuseDto>();

            foreach (var aiExcuse in aiExcuses)
            {
                result.Add(new MeetingExcuseDto
                {
                    Id = 0,
                    ExcuseText = aiExcuse.ExcuseText,
                    Category = aiExcuse.Category,
                    Type = aiExcuse.Type,
                    BelievabilityScore = aiExcuse.BelievabilityScore,
                    Tags = aiExcuse.Tags,
                    UsageCount = 0,
                    AverageRating = 0,
                    RatingCount = 0,
                    IsFavorite = false,
                    CreatedAt = DateTime.UtcNow
                });

                await UpdateUserStatsForAIExcuse(user.Id, aiExcuse);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating AI bulk excuses for user {Username}", username);
            return await GenerateBulkExcusesAsync(username, request);
        }
    }

    public async Task<MeetingExcuseDto> GeneratePersonalizedAIExcuseAsync(string username, GenerateExcuseRequestDto? criteria = null)
    {
        try
        {
            if (_aiService == null)
            {
                return await GenerateRandomExcuseAsync(username, criteria);
            }

            var user = await GetUserByUsernameAsync(username);
            if (user == null)
                throw new InvalidOperationException("User not found");

            var aiRequest = new AIMeetingExcuseRequestDto
            {
                Category = criteria?.Category ?? GetRandomCategory(),
                Type = criteria?.Type ?? GetRandomType(),
                TargetBelievability = criteria?.MinBelievability ?? 7,
                Context = "Personalized excuse based on user profile and history",
                Mood = "funny"
            };

            var aiExcuse = await _aiService.GeneratePersonalizedExcuseAsync(username, aiRequest);

            if (aiExcuse != null)
            {
                var excuseDto = new MeetingExcuseDto
                {
                    Id = 0,
                    ExcuseText = aiExcuse.ExcuseText,
                    Category = aiExcuse.Category,
                    Type = aiExcuse.Type,
                    BelievabilityScore = aiExcuse.BelievabilityScore,
                    Tags = aiExcuse.Tags,
                    UsageCount = 0,
                    AverageRating = 0,
                    RatingCount = 0,
                    IsFavorite = false,
                    CreatedAt = DateTime.UtcNow
                };

                await UpdateUserStatsForAIExcuse(user.Id, aiExcuse);
                return excuseDto;
            }

            return await GenerateRandomExcuseAsync(username, criteria);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating personalized AI excuse for user {Username}", username);
            return await GenerateRandomExcuseAsync(username, criteria);
        }
    }

    // PRIVATE HELPER METHODS - ADD THESE TO YOUR EXISTING SERVICE

    private MeetingCategory GetRandomCategory()
    {
        var categories = Enum.GetValues<MeetingCategory>();
        return categories[Random.Shared.Next(categories.Length)];
    }

    private ExcuseType GetRandomType()
    {
        var types = Enum.GetValues<ExcuseType>();
        return types[Random.Shared.Next(types.Length)];
    }

    private async Task UpdateUserStatsForAIExcuse(int userId, AIMeetingExcuseResponseDto aiExcuse)
    {
        try
        {
            var stats = await GetOrCreateUserStatsAsync(userId);

            stats.TotalExcusesGenerated++;

            // Update average believability
            var totalBelievability = stats.AverageBelievability * (stats.TotalExcusesGenerated - 1) + aiExcuse.BelievabilityScore;
            stats.AverageBelievability = totalBelievability / stats.TotalExcusesGenerated;

            // Update favorite category/type tracking
            await UpdateFavoriteCategories(stats, aiExcuse.Category, aiExcuse.Type);

            stats.UpdatedAt = DateTime.UtcNow;

            await CheckAndUpdateStreakAsync(userId);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user stats for AI excuse");
        }
    }

    private async Task UpdateFavoriteCategories(MeetingExcuseStats stats, MeetingCategory category, ExcuseType type)
    {
        // Simple frequency tracking - could be enhanced with more sophisticated analytics
        if (stats.FavoriteCategory == null)
        {
            stats.FavoriteCategory = category;
            stats.FavoriteType = type;
        }
        // Add more sophisticated logic here if needed
    }

    // NEW DTOs FOR AI INTEGRATION - ADD THESE TO YOUR EXISTING DTOS

    public class AIExcuseGenerationDto
    {
        public GenerateExcuseRequestDto Criteria { get; set; } = new();
        public bool UseAI { get; set; } = false;
        public string? Mood { get; set; } = "funny";
        public string? Context { get; set; }
        public int? TargetBelievability { get; set; }
    }

    // Private helper methods
    private async Task<Models.User?> GetUserByUsernameAsync(string username)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
    }

    private async Task<MeetingExcuseStats> GetOrCreateUserStatsAsync(int userId)
    {
        var stats = await _context.MeetingExcuseStats.FirstOrDefaultAsync(s => s.UserId == userId);
        if (stats == null)
        {
            await InitializeUserStatsAsync(userId);
            stats = await _context.MeetingExcuseStats.FirstAsync(s => s.UserId == userId);
        }
        return stats;
    }

    private async Task UpdateUserStatsAsync(int userId, MeetingExcuse excuse)
    {
        var stats = await GetOrCreateUserStatsAsync(userId);

        stats.TotalExcusesGenerated++;

        // Update average believability
        var totalBelievability = stats.AverageBelievability * (stats.TotalExcusesGenerated - 1) + excuse.BelievabilityScore;
        stats.AverageBelievability = totalBelievability / stats.TotalExcusesGenerated;

        stats.UpdatedAt = DateTime.UtcNow;

        await CheckAndUpdateStreakAsync(userId);
    }

    private async Task<MeetingExcuseDto> MapToExcuseDtoAsync(MeetingExcuse excuse, int? userId)
    {
        var isFavorite = false;
        if (userId.HasValue)
        {
            isFavorite = await _context.MeetingExcuseFavorites
                .AnyAsync(f => f.UserId == userId.Value && f.MeetingExcuseId == excuse.Id);
        }

        List<string> tags;
        try
        {
            tags = JsonSerializer.Deserialize<List<string>>(excuse.TagsJson) ?? new List<string>();
        }
        catch
        {
            tags = new List<string>();
        }

        return new MeetingExcuseDto
        {
            Id = excuse.Id,
            ExcuseText = excuse.ExcuseText,
            Category = excuse.Category,
            Type = excuse.Type,
            BelievabilityScore = excuse.BelievabilityScore,
            Tags = tags,
            UsageCount = excuse.UsageCount,
            AverageRating = excuse.AverageRating,
            RatingCount = excuse.RatingCount,
            IsFavorite = isFavorite,
            CreatedAt = excuse.CreatedAt
        };
    }
}
