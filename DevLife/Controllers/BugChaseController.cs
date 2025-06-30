using DevLife.Dtos;
using DevLife.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DevLife.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BugChaseController : ControllerBase
    {
        private readonly IBugChaseService _bugChaseService;
        private readonly ILogger<BugChaseController> _logger;

        public BugChaseController(IBugChaseService bugChaseService, ILogger<BugChaseController> logger)
        {
            _bugChaseService = bugChaseService;
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
        public async Task<ActionResult<BugChaseDashboardDto>> GetDashboard()
        {
            try
            {
                _logger.LogInformation("Getting bug chase dashboard");

                var (isValid, username, errorMessage) = ValidateSession();
                if (!isValid)
                {
                    return Unauthorized(new { message = errorMessage });
                }

                _logger.LogInformation("Getting dashboard for user: {Username}", username);
                var dashboard = await _bugChaseService.GetDashboardAsync(username);

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
                _logger.LogError(ex, "Error getting bug chase dashboard");
                return StatusCode(500, new
                {
                    message = "Bug Chase Server Error: " + ex.Message,
                    error = ex.Message,
                    type = ex.GetType().Name
                });
            }
        }

        [HttpPost("score")]
        public async Task<ActionResult<BugChaseGameResultDto>> SubmitScore([FromBody] BugChaseScoreDto scoreDto)
        {
            try
            {
                _logger.LogInformation("Submitting bug chase score: {@ScoreDto}", scoreDto);

                var (isValid, username, errorMessage) = ValidateSession();
                if (!isValid)
                {
                    return Unauthorized(new { message = errorMessage });
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid score data: {@ModelState}", ModelState);
                    return BadRequest(new { message = "Invalid score data" });
                }

                if (scoreDto.Score < 0 || scoreDto.Distance < 0)
                {
                    _logger.LogWarning("Invalid score values: Score={Score}, Distance={Distance}",
                        scoreDto.Score, scoreDto.Distance);
                    return BadRequest(new { message = "Score and distance must be non-negative" });
                }

                _logger.LogInformation("Processing score submission for user: {Username}", username);
                var result = await _bugChaseService.SubmitScoreAsync(username, scoreDto);

                _logger.LogInformation("Score submitted successfully for user: {Username}", username);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation during score submission");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting bug chase score");
                return StatusCode(500, new
                {
                    message = "Bug Chase Server Error: " + ex.Message,
                    error = ex.Message,
                    type = ex.GetType().Name
                });
            }
        }

        [HttpGet("stats")]
        public async Task<ActionResult<BugChaseStatsDto>> GetUserStats()
        {
            try
            {
                _logger.LogInformation("Getting user bug chase stats");

                var (isValid, username, errorMessage) = ValidateSession();
                if (!isValid)
                {
                    return Unauthorized(new { message = errorMessage });
                }

                _logger.LogInformation("Getting stats for user: {Username}", username);
                var stats = await _bugChaseService.GetUserStatsAsync(username);

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
                _logger.LogError(ex, "Error getting user bug chase stats");
                return StatusCode(500, new
                {
                    message = "Bug Chase Server Error: " + ex.Message,
                    error = ex.Message,
                    type = ex.GetType().Name
                });
            }
        }

        [HttpGet("leaderboard")]
        public async Task<ActionResult<List<BugChaseLeaderboardEntryDto>>> GetLeaderboard([FromQuery] int limit = 5)
        {
            try
            {
                _logger.LogInformation("Getting bug chase leaderboard with limit: {Limit}", limit);

                var (isValid, username, errorMessage) = ValidateSession();
                if (!isValid)
                {
                    return Unauthorized(new { message = errorMessage });
                }

                if (limit < 1 || limit > 50)
                {
                    _logger.LogWarning("Invalid limit: {Limit}", limit);
                    return BadRequest(new { message = "Limit must be between 1 and 50" });
                }

                _logger.LogInformation("Getting leaderboard for user: {Username}", username);
                var leaderboard = await _bugChaseService.GetLeaderboardAsync(limit);

                _logger.LogInformation("Leaderboard retrieved successfully");
                return Ok(leaderboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bug chase leaderboard");
                return StatusCode(500, new
                {
                    message = "Bug Chase Server Error: " + ex.Message,
                    error = ex.Message,
                    type = ex.GetType().Name
                });
            }
        }

        [HttpGet("recent")]
        public async Task<ActionResult<List<BugChaseGameResultDto>>> GetRecentGames([FromQuery] int limit = 10)
        {
            try
            {
                _logger.LogInformation("Getting recent bug chase games with limit: {Limit}", limit);

                var (isValid, username, errorMessage) = ValidateSession();
                if (!isValid)
                {
                    return Unauthorized(new { message = errorMessage });
                }

                if (limit < 1 || limit > 50)
                {
                    _logger.LogWarning("Invalid limit: {Limit}", limit);
                    return BadRequest(new { message = "Limit must be between 1 and 50" });
                }

                _logger.LogInformation("Getting recent games for user: {Username}", username);
                var recentGames = await _bugChaseService.GetRecentGamesAsync(username, limit);

                _logger.LogInformation("Recent games retrieved successfully for user: {Username}", username);
                return Ok(recentGames);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent bug chase games");
                return StatusCode(500, new
                {
                    message = "Bug Chase Server Error: " + ex.Message,
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
                _logger.LogInformation("Initializing user bug chase stats");

                var (isValid, username, errorMessage) = ValidateSession();
                if (!isValid)
                {
                    return Unauthorized(new { message = errorMessage });
                }

                var userIdString = HttpContext.Session.GetString("UserId");
                if (!int.TryParse(userIdString, out int userId))
                {
                    _logger.LogWarning("Invalid userId in session: {UserIdString}", userIdString);
                    return BadRequest(new { message = "Invalid user session - cannot parse userId" });
                }

                _logger.LogInformation("Initializing stats for user: {Username} (ID: {UserId})", username, userId);
                await _bugChaseService.InitializeUserStatsAsync(userId);

                _logger.LogInformation("Stats initialized successfully for user: {Username}", username);
                return Ok(new { message = "Bug chase stats initialized successfully", username, userId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing user bug chase stats");
                return StatusCode(500, new
                {
                    message = "Bug Chase Server Error: " + ex.Message,
                    error = ex.Message,
                    type = ex.GetType().Name
                });
            }
        }
    }
}
