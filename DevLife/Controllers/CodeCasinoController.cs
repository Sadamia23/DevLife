using DevLife.Dtos;
using DevLife.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DevLife.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CodeCasinoController : ControllerBase
    {
        private readonly ICodeCasinoService _casinoService;
        private readonly ILogger<CodeCasinoController> _logger;

        public CodeCasinoController(ICodeCasinoService casinoService, ILogger<CodeCasinoController> logger)
        {
            _casinoService = casinoService;
            _logger = logger;
        }

        /// <summary>
        /// Helper method to safely check session and authentication
        /// </summary>
        private (bool IsValid, string Username, string UserId, string ErrorMessage) ValidateSession()
        {
            try
            {
                // Check if session is available
                if (!HttpContext.Session.IsAvailable)
                {
                    _logger.LogError("Session is not available for request");
                    return (false, "", "", "Session not configured properly");
                }

                var username = HttpContext.Session.GetString("Username");
                var userId = HttpContext.Session.GetString("UserId");

                _logger.LogDebug("Session check - SessionId: {SessionId}, Username: {Username}, UserId: {UserId}",
                    HttpContext.Session.Id, username, userId);

                if (string.IsNullOrEmpty(username))
                {
                    _logger.LogWarning("No username found in session. SessionId: {SessionId}",
                        HttpContext.Session.Id);
                    return (false, "", "", "Authentication required - please login");
                }

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("No userId found in session. Username: {Username}",
                        username);
                    return (false, username, "", "Session incomplete - please login again");
                }

                return (true, username, userId, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating session");
                return (false, "", "", $"Session validation failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Debug endpoint to check authentication and session state
        /// </summary>
        [HttpGet("debug/auth")]
        public IActionResult DebugAuth()
        {
            try
            {
                var sessionInfo = new
                {
                    HasHttpContext = HttpContext != null,
                    HasSession = HttpContext?.Session != null,
                    SessionAvailable = HttpContext?.Session?.IsAvailable ?? false,
                    SessionId = HttpContext?.Session?.IsAvailable == true ? HttpContext.Session.Id : "N/A",
                    RequestPath = HttpContext?.Request?.Path.ToString(),
                    RequestMethod = HttpContext?.Request?.Method
                };

                _logger.LogInformation("Debug auth called - Session info: {@SessionInfo}", sessionInfo);

                if (!HttpContext.Session.IsAvailable)
                {
                    return StatusCode(500, new
                    {
                        error = "Session not available",
                        message = "Session middleware is not properly configured",
                        sessionInfo
                    });
                }

                var username = HttpContext.Session.GetString("Username");
                var userId = HttpContext.Session.GetString("UserId");

                return Ok(new
                {
                    message = "Debug auth successful",
                    sessionInfo,
                    username,
                    userId,
                    isAuthenticated = !string.IsNullOrEmpty(username),
                    sessionKeys = HttpContext.Session.Keys.ToList(),
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in debug auth endpoint");
                return StatusCode(500, new
                {
                    error = ex.Message,
                    stackTrace = ex.StackTrace,
                    type = ex.GetType().Name
                });
            }
        }

        /// <summary>
        /// Get casino dashboard with user stats, leaderboard, and daily challenge
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<ActionResult<CasinoStatsResponse>> GetDashboard()
        {
            try
            {
                _logger.LogInformation("Getting casino dashboard");

                var (isValid, username, userId, errorMessage) = ValidateSession();
                if (!isValid)
                {
                    return Unauthorized(new { message = errorMessage });
                }

                _logger.LogInformation("Getting dashboard for user: {Username}", username);
                var stats = await _casinoService.GetCasinoStatsAsync(username);

                if (stats == null)
                {
                    _logger.LogWarning("No stats found for user: {Username}", username);
                    return NotFound(new { message = "User stats not found" });
                }

                _logger.LogInformation("Dashboard retrieved successfully for user: {Username}", username);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting casino dashboard");
                return StatusCode(500, new
                {
                    message = "Casino Server Error: " + ex.Message,
                    error = ex.Message,
                    type = ex.GetType().Name
                });
            }
        }

        /// <summary>
        /// Get a random code challenge for the user
        /// </summary>
        [HttpGet("challenge")]
        public async Task<ActionResult<CodeChallengeDto>> GetRandomChallenge()
        {
            try
            {
                _logger.LogInformation("Getting random challenge");

                var (isValid, username, userId, errorMessage) = ValidateSession();
                if (!isValid)
                {
                    return Unauthorized(new { message = errorMessage });
                }

                _logger.LogInformation("Getting challenge for user: {Username}", username);
                var challenge = await _casinoService.GetRandomChallengeAsync(username);

                if (challenge == null)
                {
                    _logger.LogWarning("No challenges available for user: {Username}", username);
                    return NotFound(new { message = "No challenges available for your tech stack" });
                }

                _logger.LogInformation("Challenge retrieved successfully for user: {Username}", username);
                return Ok(challenge);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting random challenge");
                return StatusCode(500, new
                {
                    message = "Casino Server Error: " + ex.Message,
                    error = ex.Message,
                    type = ex.GetType().Name
                });
            }
        }

        /// <summary>
        /// Get today's daily challenge
        /// </summary>
        [HttpGet("daily-challenge")]
        public async Task<ActionResult<DailyChallengeDto>> GetDailyChallenge()
        {
            try
            {
                _logger.LogInformation("Getting daily challenge");

                var (isValid, username, userId, errorMessage) = ValidateSession();
                if (!isValid)
                {
                    return Unauthorized(new { message = errorMessage });
                }

                _logger.LogInformation("Getting daily challenge for user: {Username}", username);
                var dailyChallenge = await _casinoService.GetDailyChallengeAsync(username);

                if (dailyChallenge == null)
                {
                    _logger.LogWarning("No daily challenge available for user: {Username}", username);
                    return NotFound(new { message = "No daily challenge available" });
                }

                _logger.LogInformation("Daily challenge retrieved successfully for user: {Username}", username);
                return Ok(dailyChallenge);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting daily challenge");
                return StatusCode(500, new
                {
                    message = "Casino Server Error: " + ex.Message,
                    error = ex.Message,
                    type = ex.GetType().Name
                });
            }
        }

        /// <summary>
        /// Place a bet on a code challenge
        /// </summary>
        [HttpPost("bet")]
        public async Task<ActionResult<GameResultDto>> PlaceBet([FromBody] PlaceBetDto betDto)
        {
            try
            {
                _logger.LogInformation("Placing bet: {@BetDto}", betDto);

                var (isValid, username, userId, errorMessage) = ValidateSession();
                if (!isValid)
                {
                    return Unauthorized(new { message = errorMessage });
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid bet data: {@ModelState}", ModelState);
                    return BadRequest(new { message = "Invalid bet data" });
                }

                if (betDto.ChosenOption != 1 && betDto.ChosenOption != 2)
                {
                    _logger.LogWarning("Invalid chosen option: {ChosenOption}", betDto.ChosenOption);
                    return BadRequest(new { message = "Chosen option must be 1 or 2" });
                }

                _logger.LogInformation("Processing bet for user: {Username}", username);
                var result = await _casinoService.PlaceBetAsync(username, betDto);

                _logger.LogInformation("Bet placed successfully for user: {Username}", username);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation during bet placement");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error placing bet");
                return StatusCode(500, new
                {
                    message = "Casino Server Error: " + ex.Message,
                    error = ex.Message,
                    type = ex.GetType().Name
                });
            }
        }

        /// <summary>
        /// Get current user's casino statistics
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<UserStatsDto>> GetUserStats()
        {
            try
            {
                _logger.LogInformation("Getting user stats");

                var (isValid, username, userId, errorMessage) = ValidateSession();
                if (!isValid)
                {
                    return Unauthorized(new { message = errorMessage });
                }

                _logger.LogInformation("Getting stats for user: {Username}", username);
                var stats = await _casinoService.GetUserStatsAsync(username);

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
                    message = "Casino Server Error: " + ex.Message,
                    error = ex.Message,
                    type = ex.GetType().Name
                });
            }
        }

        /// <summary>
        /// Get casino leaderboard
        /// </summary>
        [HttpGet("leaderboard")]
        public async Task<ActionResult<List<LeaderboardEntryDto>>> GetLeaderboard([FromQuery] int limit = 10)
        {
            try
            {
                _logger.LogInformation("Getting leaderboard with limit: {Limit}", limit);

                var (isValid, username, userId, errorMessage) = ValidateSession();
                if (!isValid)
                {
                    return Unauthorized(new { message = errorMessage });
                }

                if (limit < 1 || limit > 100)
                {
                    _logger.LogWarning("Invalid limit: {Limit}", limit);
                    return BadRequest(new { message = "Limit must be between 1 and 100" });
                }

                _logger.LogInformation("Getting leaderboard for user: {Username}", username);
                var leaderboard = await _casinoService.GetLeaderboardAsync(limit);

                _logger.LogInformation("Leaderboard retrieved successfully");
                return Ok(leaderboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting leaderboard");
                return StatusCode(500, new
                {
                    message = "Casino Server Error: " + ex.Message,
                    error = ex.Message,
                    type = ex.GetType().Name
                });
            }
        }

        /// <summary>
        /// Initialize casino stats for the current user (if not exists)
        /// </summary>
        [HttpPost("initialize")]
        public async Task<IActionResult> InitializeUserStats()
        {
            try
            {
                _logger.LogInformation("Initializing user stats");

                var (isValid, username, userIdString, errorMessage) = ValidateSession();
                if (!isValid)
                {
                    return Unauthorized(new { message = errorMessage });
                }

                if (!int.TryParse(userIdString, out int userId))
                {
                    _logger.LogWarning("Invalid userId in session: {UserIdString}", userIdString);
                    return BadRequest(new { message = "Invalid user session - cannot parse userId" });
                }

                _logger.LogInformation("Initializing stats for user: {Username} (ID: {UserId})", username, userId);
                await _casinoService.InitializeUserStatsAsync(userId);

                _logger.LogInformation("Stats initialized successfully for user: {Username}", username);
                return Ok(new { message = "Casino stats initialized successfully", username, userId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing user stats");
                return StatusCode(500, new
                {
                    message = "Casino Server Error: " + ex.Message,
                    error = ex.Message,
                    type = ex.GetType().Name
                });
            }
        }
    }
}