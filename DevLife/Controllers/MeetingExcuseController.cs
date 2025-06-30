using DevLife.Dtos;
using DevLife.Enums;
using DevLife.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using static DevLife.Services.Implementation.MeetingExcuseService;

namespace DevLife.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MeetingExcuseController : ControllerBase
{
    private readonly IMeetingExcuseService _meetingExcuseService;
    private readonly ILogger<MeetingExcuseController> _logger;

    public MeetingExcuseController(IMeetingExcuseService meetingExcuseService, ILogger<MeetingExcuseController> logger)
    {
        _meetingExcuseService = meetingExcuseService;
        _logger = logger;
    }

    private (bool IsValid, string Username, string ErrorMessage) ValidateSession()
    {
        try
        {
            if (!HttpContext.Session.IsAvailable)
            {
                _logger.LogError("Session is not available for request");
                return (false, "", "Session not configured properly");
            }

            var username = HttpContext.Session.GetString("Username");

            _logger.LogDebug("Session check - SessionId: {SessionId}, Username: {Username}",
                HttpContext.Session.Id, username);

            if (string.IsNullOrEmpty(username))
            {
                _logger.LogWarning("No username found in session. SessionId: {SessionId}",
                    HttpContext.Session.Id);
                return (false, "", "Authentication required - please login");
            }

            return (true, username, "");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating session");
            return (false, "", $"Session validation failed: {ex.Message}");
        }
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<MeetingExcuseDashboardDto>> GetDashboard()
    {
        try
        {
            _logger.LogInformation("Getting meeting excuse dashboard");

            var (isValid, username, errorMessage) = ValidateSession();
            if (!isValid)
            {
                return Unauthorized(new { message = errorMessage });
            }

            _logger.LogInformation("Getting dashboard for user: {Username}", username);
            var dashboard = await _meetingExcuseService.GetDashboardAsync(username);

            if (dashboard == null)
            {
                _logger.LogWarning("No dashboard data found for user: {Username}", username);
                return NotFound(new { message = "User data not found" });
            }

            _logger.LogInformation("Dashboard retrieved successfully for user: {Username}", username);
            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting meeting excuse dashboard");
            return StatusCode(500, new
            {
                message = "Meeting Excuse Server Error: " + ex.Message,
                error = ex.Message,
                type = ex.GetType().Name
            });
        }
    }

    [HttpPost("generate")]
    public async Task<ActionResult<MeetingExcuseDto>> GenerateExcuse([FromBody] GenerateExcuseRequestDto? criteria = null)
    {
        try
        {
            _logger.LogInformation("Generating meeting excuse with criteria: {@Criteria}", criteria);

            var (isValid, username, errorMessage) = ValidateSession();
            if (!isValid)
            {
                return Unauthorized(new { message = errorMessage });
            }

            _logger.LogInformation("Generating excuse for user: {Username}", username);
            var excuse = await _meetingExcuseService.GenerateRandomExcuseAsync(username, criteria);

            if (excuse == null)
            {
                _logger.LogWarning("No excuse generated for user: {Username}", username);
                return NotFound(new { message = "No excuses available matching your criteria" });
            }

            _logger.LogInformation("Excuse generated successfully for user: {Username}", username);
            return Ok(excuse);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation during excuse generation");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating meeting excuse");
            return StatusCode(500, new
            {
                message = "Meeting Excuse Server Error: " + ex.Message,
                error = ex.Message,
                type = ex.GetType().Name
            });
        }
    }

    [HttpPost("generate/bulk")]
    public async Task<ActionResult<List<MeetingExcuseDto>>> GenerateBulkExcuses([FromBody] BulkExcuseGenerationDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid request data" });
            }

            var (isValid, username, errorMessage) = ValidateSession();
            if (!isValid)
            {
                return Unauthorized(new { message = errorMessage });
            }

            if (request.Count < 1 || request.Count > 10)
            {
                return BadRequest(new { message = "Count must be between 1 and 10" });
            }

            _logger.LogInformation("Generating {Count} excuses for user: {Username}", request.Count, username);
            var excuses = await _meetingExcuseService.GenerateBulkExcusesAsync(username, request);

            _logger.LogInformation("Bulk excuses generated successfully for user: {Username}", username);
            return Ok(excuses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating bulk excuses");
            return StatusCode(500, new
            {
                message = "Meeting Excuse Server Error: " + ex.Message,
                error = ex.Message,
                type = ex.GetType().Name
            });
        }
    }

    [HttpGet("excuse-of-the-day")]
    public async Task<ActionResult<MeetingExcuseDto>> GetExcuseOfTheDay()
    {
        try
        {
            _logger.LogInformation("Getting excuse of the day");

            var excuse = await _meetingExcuseService.GetExcuseOfTheDayAsync();

            if (excuse == null)
            {
                _logger.LogWarning("No excuse of the day available");
                return NotFound(new { message = "No excuse of the day available" });
            }

            _logger.LogInformation("Excuse of the day retrieved successfully");
            return Ok(excuse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting excuse of the day");
            return StatusCode(500, new
            {
                message = "Meeting Excuse Server Error: " + ex.Message,
                error = ex.Message,
                type = ex.GetType().Name
            });
        }
    }

    [HttpPost("favorites")]
    public async Task<ActionResult<MeetingExcuseFavoriteDto>> SaveFavorite([FromBody] SaveFavoriteRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid request data" });
            }

            var (isValid, username, errorMessage) = ValidateSession();
            if (!isValid)
            {
                return Unauthorized(new { message = errorMessage });
            }

            _logger.LogInformation("Saving favorite excuse {ExcuseId} for user: {Username}", request.MeetingExcuseId, username);
            var favorite = await _meetingExcuseService.SaveFavoriteAsync(username, request);

            _logger.LogInformation("Favorite saved successfully for user: {Username}", username);
            return Ok(favorite);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation during favorite save");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving favorite excuse");
            return StatusCode(500, new
            {
                message = "Meeting Excuse Server Error: " + ex.Message,
                error = ex.Message,
                type = ex.GetType().Name
            });
        }
    }

    [HttpDelete("favorites/{favoriteId}")]
    public async Task<IActionResult> RemoveFavorite(int favoriteId)
    {
        try
        {
            var (isValid, username, errorMessage) = ValidateSession();
            if (!isValid)
            {
                return Unauthorized(new { message = errorMessage });
            }

            _logger.LogInformation("Removing favorite {FavoriteId} for user: {Username}", favoriteId, username);
            var success = await _meetingExcuseService.RemoveFavoriteAsync(username, favoriteId);

            if (!success)
            {
                _logger.LogWarning("Favorite {FavoriteId} not found for user: {Username}", favoriteId, username);
                return NotFound(new { message = "Favorite not found" });
            }

            _logger.LogInformation("Favorite removed successfully for user: {Username}", username);
            return Ok(new { message = "Favorite removed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing favorite excuse");
            return StatusCode(500, new
            {
                message = "Meeting Excuse Server Error: " + ex.Message,
                error = ex.Message,
                type = ex.GetType().Name
            });
        }
    }

    [HttpGet("favorites")]
    public async Task<ActionResult<List<MeetingExcuseFavoriteDto>>> GetFavorites([FromQuery] int limit = 20)
    {
        try
        {
            var (isValid, username, errorMessage) = ValidateSession();
            if (!isValid)
            {
                return Unauthorized(new { message = errorMessage });
            }

            if (limit < 1 || limit > 100)
            {
                return BadRequest(new { message = "Limit must be between 1 and 100" });
            }

            _logger.LogInformation("Getting favorites for user: {Username}", username);
            var favorites = await _meetingExcuseService.GetUserFavoritesAsync(username, limit);

            _logger.LogInformation("Favorites retrieved successfully for user: {Username}", username);
            return Ok(favorites);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user favorites");
            return StatusCode(500, new
            {
                message = "Meeting Excuse Server Error: " + ex.Message,
                error = ex.Message,
                type = ex.GetType().Name
            });
        }
    }

    [HttpPost("usage")]
    public async Task<ActionResult<MeetingExcuseUsageDto>> SubmitUsage([FromBody] SubmitUsageRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid request data" });
            }

            var (isValid, username, errorMessage) = ValidateSession();
            if (!isValid)
            {
                return Unauthorized(new { message = errorMessage });
            }

            _logger.LogInformation("Submitting usage for excuse {ExcuseId} by user: {Username}", request.MeetingExcuseId, username);
            var usage = await _meetingExcuseService.SubmitUsageAsync(username, request);

            _logger.LogInformation("Usage submitted successfully for user: {Username}", username);
            return Ok(usage);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation during usage submission");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting usage");
            return StatusCode(500, new
            {
                message = "Meeting Excuse Server Error: " + ex.Message,
                error = ex.Message,
                type = ex.GetType().Name
            });
        }
    }

    [HttpGet("usage")]
    public async Task<ActionResult<List<MeetingExcuseUsageDto>>> GetUsageHistory([FromQuery] int limit = 20)
    {
        try
        {
            var (isValid, username, errorMessage) = ValidateSession();
            if (!isValid)
            {
                return Unauthorized(new { message = errorMessage });
            }

            if (limit < 1 || limit > 100)
            {
                return BadRequest(new { message = "Limit must be between 1 and 100" });
            }

            _logger.LogInformation("Getting usage history for user: {Username}", username);
            var history = await _meetingExcuseService.GetUserUsageHistoryAsync(username, limit);

            _logger.LogInformation("Usage history retrieved successfully for user: {Username}", username);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting usage history");
            return StatusCode(500, new
            {
                message = "Meeting Excuse Server Error: " + ex.Message,
                error = ex.Message,
                type = ex.GetType().Name
            });
        }
    }

    [HttpGet("stats")]
    public async Task<ActionResult<MeetingExcuseStatsDto>> GetUserStats()
    {
        try
        {
            var (isValid, username, errorMessage) = ValidateSession();
            if (!isValid)
            {
                return Unauthorized(new { message = errorMessage });
            }

            _logger.LogInformation("Getting stats for user: {Username}", username);
            var stats = await _meetingExcuseService.GetUserStatsAsync(username);

            if (stats == null)
            {
                _logger.LogWarning("No stats found for user: {Username}", username);
                return NotFound(new { message = "User stats not found" });
            }

            _logger.LogInformation("Stats retrieved successfully for user: {Username}", username);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user stats");
            return StatusCode(500, new
            {
                message = "Meeting Excuse Server Error: " + ex.Message,
                error = ex.Message,
                type = ex.GetType().Name
            });
        }
    }

    [HttpGet("leaderboard")]
    public async Task<ActionResult<List<MeetingExcuseLeaderboardEntryDto>>> GetLeaderboard([FromQuery] int limit = 10)
    {
        try
        {
            var (isValid, username, errorMessage) = ValidateSession();
            if (!isValid)
            {
                return Unauthorized(new { message = errorMessage });
            }

            if (limit < 1 || limit > 50)
            {
                return BadRequest(new { message = "Limit must be between 1 and 50" });
            }

            _logger.LogInformation("Getting leaderboard for user: {Username}", username);
            var leaderboard = await _meetingExcuseService.GetLeaderboardAsync(limit);

            _logger.LogInformation("Leaderboard retrieved successfully");
            return Ok(leaderboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting leaderboard");
            return StatusCode(500, new
            {
                message = "Meeting Excuse Server Error: " + ex.Message,
                error = ex.Message,
                type = ex.GetType().Name
            });
        }
    }

    [HttpPost("rate")]
    public async Task<IActionResult> RateExcuse([FromBody] RateExcuseRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid request data" });
            }

            var (isValid, username, errorMessage) = ValidateSession();
            if (!isValid)
            {
                return Unauthorized(new { message = errorMessage });
            }

            _logger.LogInformation("Rating excuse {ExcuseId} with {Rating} stars by user: {Username}",
                request.MeetingExcuseId, request.Rating, username);

            var success = await _meetingExcuseService.RateExcuseAsync(username, request);

            if (!success)
            {
                _logger.LogWarning("Failed to rate excuse {ExcuseId} for user: {Username}", request.MeetingExcuseId, username);
                return BadRequest(new { message = "Failed to rate excuse" });
            }

            _logger.LogInformation("Excuse rated successfully by user: {Username}", username);
            return Ok(new { message = "Excuse rated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rating excuse");
            return StatusCode(500, new
            {
                message = "Meeting Excuse Server Error: " + ex.Message,
                error = ex.Message,
                type = ex.GetType().Name
            });
        }
    }

    [HttpGet("trending")]
    public async Task<ActionResult<List<MeetingExcuseDto>>> GetTrendingExcuses([FromQuery] int limit = 10)
    {
        try
        {
            if (limit < 1 || limit > 50)
            {
                return BadRequest(new { message = "Limit must be between 1 and 50" });
            }

            _logger.LogInformation("Getting trending excuses");
            var trending = await _meetingExcuseService.GetTrendingExcusesAsync(limit);

            _logger.LogInformation("Trending excuses retrieved successfully");
            return Ok(trending);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting trending excuses");
            return StatusCode(500, new
            {
                message = "Meeting Excuse Server Error: " + ex.Message,
                error = ex.Message,
                type = ex.GetType().Name
            });
        }
    }

    [HttpGet("top-rated")]
    public async Task<ActionResult<List<MeetingExcuseDto>>> GetTopRatedExcuses([FromQuery] int limit = 10)
    {
        try
        {
            if (limit < 1 || limit > 50)
            {
                return BadRequest(new { message = "Limit must be between 1 and 50" });
            }

            _logger.LogInformation("Getting top rated excuses");
            var topRated = await _meetingExcuseService.GetTopRatedExcusesAsync(limit);

            _logger.LogInformation("Top rated excuses retrieved successfully");
            return Ok(topRated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top rated excuses");
            return StatusCode(500, new
            {
                message = "Meeting Excuse Server Error: " + ex.Message,
                error = ex.Message,
                type = ex.GetType().Name
            });
        }
    }

    [HttpGet("analytics")]
    public async Task<ActionResult<ExcuseAnalyticsDto>> GetAnalytics()
    {
        try
        {
            var (isValid, username, errorMessage) = ValidateSession();
            if (!isValid)
            {
                return Unauthorized(new { message = errorMessage });
            }

            _logger.LogInformation("Getting excuse analytics");
            var analytics = await _meetingExcuseService.GetAnalyticsAsync();

            _logger.LogInformation("Analytics retrieved successfully");
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting analytics");
            return StatusCode(500, new
            {
                message = "Meeting Excuse Server Error: " + ex.Message,
                error = ex.Message,
                type = ex.GetType().Name
            });
        }
    }

    [HttpGet("tags")]
    public async Task<ActionResult<List<string>>> GetAvailableTags()
    {
        try
        {
            _logger.LogInformation("Getting available tags");
            var tags = await _meetingExcuseService.GetAvailableTagsAsync();

            _logger.LogInformation("Available tags retrieved successfully");
            return Ok(tags);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available tags");
            return StatusCode(500, new
            {
                message = "Meeting Excuse Server Error: " + ex.Message,
                error = ex.Message,
                type = ex.GetType().Name
            });
        }
    }

    [HttpPost("initialize")]
    public async Task<IActionResult> InitializeUserStats()
    {
        try
        {
            var (isValid, username, errorMessage) = ValidateSession();
            if (!isValid)
            {
                return Unauthorized(new { message = errorMessage });
            }

            var userIdString = HttpContext.Session.GetString("UserId");
            if (!int.TryParse(userIdString, out int userId))
            {
                return BadRequest(new { message = "Invalid user session - cannot parse userId" });
            }

            _logger.LogInformation("Initializing meeting excuse stats for user: {Username} (ID: {UserId})", username, userId);
            await _meetingExcuseService.InitializeUserStatsAsync(userId);

            _logger.LogInformation("Stats initialized successfully for user: {Username}", username);
            return Ok(new { message = "Meeting excuse stats initialized successfully", username, userId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing user stats");
            return StatusCode(500, new
            {
                message = "Meeting Excuse Server Error: " + ex.Message,
                error = ex.Message,
                type = ex.GetType().Name
            });
        }
    }

    [HttpPost("generate/ai")]
    public async Task<ActionResult<MeetingExcuseDto>> GenerateAIExcuse([FromBody] GenerateExcuseRequestDto? criteria = null)
    {
        try
        {
            _logger.LogInformation("Generating AI-powered meeting excuse with criteria: {@Criteria}", criteria);

            var (isValid, username, errorMessage) = ValidateSession();
            if (!isValid)
            {
                return Unauthorized(new { message = errorMessage });
            }

            _logger.LogInformation("Generating AI excuse for user: {Username}", username);
            var excuse = await _meetingExcuseService.GenerateAIExcuseAsync(username, criteria);

            if (excuse == null)
            {
                _logger.LogWarning("No AI excuse generated for user: {Username}", username);
                return NotFound(new { message = "Unable to generate AI excuse at this time" });
            }

            _logger.LogInformation("AI excuse generated successfully for user: {Username}", username);
            return Ok(excuse);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation during AI excuse generation");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating AI meeting excuse");
            return StatusCode(500, new
            {
                message = "AI Excuse Generation Error: " + ex.Message,
                error = ex.Message,
                type = ex.GetType().Name
            });
        }
    }

    [HttpPost("generate/ai/bulk")]
    public async Task<ActionResult<List<MeetingExcuseDto>>> GenerateAIBulkExcuses([FromBody] BulkExcuseGenerationDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid request data" });
            }

            var (isValid, username, errorMessage) = ValidateSession();
            if (!isValid)
            {
                return Unauthorized(new { message = errorMessage });
            }

            if (request.Count < 1 || request.Count > 5) // Limit AI calls
            {
                return BadRequest(new { message = "AI bulk generation limited to 1-5 excuses" });
            }

            _logger.LogInformation("Generating {Count} AI excuses for user: {Username}", request.Count, username);
            var excuses = await _meetingExcuseService.GenerateAIBulkExcusesAsync(username, request);

            _logger.LogInformation("AI bulk excuses generated successfully for user: {Username}", username);
            return Ok(excuses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating AI bulk excuses");
            return StatusCode(500, new
            {
                message = "AI Bulk Generation Error: " + ex.Message,
                error = ex.Message,
                type = ex.GetType().Name
            });
        }
    }

    [HttpPost("generate/ai/personalized")]
    public async Task<ActionResult<MeetingExcuseDto>> GeneratePersonalizedAIExcuse([FromBody] GenerateExcuseRequestDto? criteria = null)
    {
        try
        {
            _logger.LogInformation("Generating personalized AI excuse with criteria: {@Criteria}", criteria);

            var (isValid, username, errorMessage) = ValidateSession();
            if (!isValid)
            {
                return Unauthorized(new { message = errorMessage });
            }

            _logger.LogInformation("Generating personalized AI excuse for user: {Username}", username);
            var excuse = await _meetingExcuseService.GeneratePersonalizedAIExcuseAsync(username, criteria);

            if (excuse == null)
            {
                _logger.LogWarning("No personalized AI excuse generated for user: {Username}", username);
                return NotFound(new { message = "Unable to generate personalized AI excuse at this time" });
            }

            _logger.LogInformation("Personalized AI excuse generated successfully for user: {Username}", username);
            return Ok(excuse);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation during personalized AI excuse generation");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating personalized AI meeting excuse");
            return StatusCode(500, new
            {
                message = "Personalized AI Excuse Generation Error: " + ex.Message,
                error = ex.Message,
                type = ex.GetType().Name
            });
        }
    }

    [HttpGet("ai/status")]
    public async Task<ActionResult<object>> GetAIStatus()
    {
        try
        {
            var (isValid, username, errorMessage) = ValidateSession();
            if (!isValid)
            {
                return Unauthorized(new { message = errorMessage });
            }

            var stats = await _meetingExcuseService.GetUserStatsAsync(username);

            return Ok(new
            {
                aiAvailable = true, // You can check if AI service is configured
                userStats = new
                {
                    totalExcusesGenerated = stats.TotalExcusesGenerated,
                },
                features = new
                {
                    personalizedExcuses = true,
                    bulkGeneration = true,
                    contextAware = true,
                    techStackAware = true
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting AI status");
            return StatusCode(500, new
            {
                message = "AI Status Error: " + ex.Message,
                error = ex.Message,
                type = ex.GetType().Name
            });
        }
    }

    [HttpPost("generate/smart")]
    public async Task<ActionResult<MeetingExcuseDto>> GenerateSmartExcuse([FromBody] AIExcuseGenerationDto request)
    {
        try
        {
            _logger.LogInformation("Generating smart excuse with AI flag: {UseAI}", request.UseAI);

            var (isValid, username, errorMessage) = ValidateSession();
            if (!isValid)
            {
                return Unauthorized(new { message = errorMessage });
            }

            MeetingExcuseDto excuse;

            if (request.UseAI)
            {
                excuse = await _meetingExcuseService.GeneratePersonalizedAIExcuseAsync(username, request.Criteria);
            }
            else
            {
                excuse = await _meetingExcuseService.GenerateRandomExcuseAsync(username, request.Criteria);
            }

            if (excuse == null)
            {
                _logger.LogWarning("No smart excuse generated for user: {Username}", username);
                return NotFound(new { message = "Unable to generate excuse at this time" });
            }

            _logger.LogInformation("Smart excuse generated successfully for user: {Username}", username);
            return Ok(excuse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating smart excuse");
            return StatusCode(500, new
            {
                message = "Smart Excuse Generation Error: " + ex.Message,
                error = ex.Message,
                type = ex.GetType().Name
            });
        }
    }

    [HttpGet("ai/samples")]
    public async Task<ActionResult<List<object>>> GetAISamples()
    {
        try
        {
            var (isValid, username, errorMessage) = ValidateSession();
            if (!isValid)
            {
                return Unauthorized(new { message = errorMessage });
            }

            var samples = new List<object>();
            var categories = new[] { MeetingCategory.DailyStandup, MeetingCategory.ClientMeeting, MeetingCategory.TeamBuilding };
            var types = new[] { ExcuseType.Technical, ExcuseType.Personal, ExcuseType.Creative };

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    var criteria = new GenerateExcuseRequestDto
                    {
                        Category = categories[i],
                        Type = types[i],
                        MinBelievability = 7
                    };

                    var excuse = await _meetingExcuseService.GenerateAIExcuseAsync(username, criteria);
                    if (excuse != null)
                    {
                        samples.Add(new
                        {
                            category = excuse.Category.ToString(),
                            type = excuse.Type.ToString(),
                            text = excuse.ExcuseText,
                            believability = excuse.BelievabilityScore,
                            tags = excuse.Tags
                        });
                    }
                }
                catch
                {
                    continue;
                }
            }

            return Ok(samples);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting AI samples");
            return StatusCode(500, new
            {
                message = "AI Samples Error: " + ex.Message,
                error = ex.Message,
                type = ex.GetType().Name
            });
        }
    }
}
