using DevLife.Dtos;
using DevLife.Enums;
using DevLife.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DevLife.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CodeRoastController : ControllerBase
    {
        private readonly ICodeRoastService _codeRoastService;
        private readonly ILogger<CodeRoastController> _logger;

        public CodeRoastController(ICodeRoastService codeRoastService, ILogger<CodeRoastController> logger)
        {
            _codeRoastService = codeRoastService;
            _logger = logger;
        }

        private (bool IsValid, string Username, string UserId, string ErrorMessage) ValidateSession()
        {
            try
            {
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

        [HttpGet("dashboard")]
        public async Task<ActionResult<CodeRoastDashboardDto>> GetDashboard()
        {
            try
            {
                _logger.LogInformation("Getting code roast dashboard");

                var (isValid, username, userId, errorMessage) = ValidateSession();
                if (!isValid)
                {
                    return Unauthorized(new { message = errorMessage });
                }

                _logger.LogInformation("Getting dashboard for user: {Username}", username);
                var dashboard = await _codeRoastService.GetDashboardAsync(username);

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
                _logger.LogError(ex, "Error getting code roast dashboard");
                return StatusCode(500, new
                {
                    message = "Code Roast Server Error: " + ex.Message,
                    error = ex.Message,
                    type = ex.GetType().Name
                });
            }
        }

        [HttpGet("task")]
        public async Task<ActionResult<CodeRoastTaskDto>> GetTask([FromQuery] string difficulty = "junior")
        {
            try
            {
                _logger.LogInformation("Getting coding task with difficulty: {Difficulty}", difficulty);

                var (isValid, username, userId, errorMessage) = ValidateSession();
                if (!isValid)
                {
                    return Unauthorized(new { message = errorMessage });
                }

                if (!Enum.TryParse<ExperienceLevel>(difficulty, true, out var difficultyLevel))
                {
                    difficultyLevel = ExperienceLevel.Junior;
                }

                _logger.LogInformation("Getting task for user: {Username} with difficulty: {Difficulty}", username, difficultyLevel);
                var task = await _codeRoastService.GetCodingTaskAsync(username, difficultyLevel);

                if (task == null)
                {
                    _logger.LogWarning("No task available for user: {Username}", username);
                    return NotFound(new { message = "No coding task available for your tech stack" });
                }

                _logger.LogInformation("Task retrieved successfully for user: {Username}", username);
                return Ok(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting coding task");
                return StatusCode(500, new
                {
                    message = "Code Roast Server Error: " + ex.Message,
                    error = ex.Message,
                    type = ex.GetType().Name
                });
            }
        }

        [HttpPost("submit")]
        public async Task<ActionResult<CodeRoastResultDto>> SubmitCode([FromBody] CodeRoastSubmissionDto submissionDto)
        {
            try
            {
                _logger.LogInformation("Submitting code for roasting: {@SubmissionDto}", submissionDto);

                var (isValid, username, userId, errorMessage) = ValidateSession();
                if (!isValid)
                {
                    return Unauthorized(new { message = errorMessage });
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid submission data: {@ModelState}", ModelState);
                    return BadRequest(new { message = "Invalid submission data" });
                }

                if (string.IsNullOrWhiteSpace(submissionDto.Code))
                {
                    _logger.LogWarning("Empty code submission from user: {Username}", username);
                    return BadRequest(new { message = "Code cannot be empty" });
                }

                _logger.LogInformation("Processing code submission for user: {Username}", username);
                var result = await _codeRoastService.SubmitCodeAsync(username, submissionDto);

                _logger.LogInformation("Code submission processed successfully for user: {Username}", username);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation during code submission");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting code for roasting");
                return StatusCode(500, new
                {
                    message = "Code Roast Server Error: " + ex.Message,
                    error = ex.Message,
                    type = ex.GetType().Name
                });
            }
        }

        [HttpGet("stats")]
        public async Task<ActionResult<CodeRoastStatsDto>> GetUserStats()
        {
            try
            {
                _logger.LogInformation("Getting user code roast stats");

                var (isValid, username, userId, errorMessage) = ValidateSession();
                if (!isValid)
                {
                    return Unauthorized(new { message = errorMessage });
                }

                _logger.LogInformation("Getting stats for user: {Username}", username);
                var stats = await _codeRoastService.GetUserStatsAsync(username);

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
                _logger.LogError(ex, "Error getting user code roast stats");
                return StatusCode(500, new
                {
                    message = "Code Roast Server Error: " + ex.Message,
                    error = ex.Message,
                    type = ex.GetType().Name
                });
            }
        }

        [HttpGet("history")]
        public async Task<ActionResult<List<CodeRoastResultDto>>> GetRoastHistory([FromQuery] int limit = 10)
        {
            try
            {
                _logger.LogInformation("Getting roast history with limit: {Limit}", limit);

                var (isValid, username, userId, errorMessage) = ValidateSession();
                if (!isValid)
                {
                    return Unauthorized(new { message = errorMessage });
                }

                if (limit < 1 || limit > 50)
                {
                    _logger.LogWarning("Invalid limit: {Limit}", limit);
                    return BadRequest(new { message = "Limit must be between 1 and 50" });
                }

                _logger.LogInformation("Getting roast history for user: {Username}", username);
                var history = await _codeRoastService.GetRoastHistoryAsync(username, limit);

                _logger.LogInformation("Roast history retrieved successfully for user: {Username}", username);
                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting roast history");
                return StatusCode(500, new
                {
                    message = "Code Roast Server Error: " + ex.Message,
                    error = ex.Message,
                    type = ex.GetType().Name
                });
            }
        }

        [HttpGet("hall-of-fame")]
        public async Task<ActionResult<CodeRoastHallOfFameDto>> GetHallOfFame()
        {
            try
            {
                _logger.LogInformation("Getting hall of fame");

                var (isValid, username, userId, errorMessage) = ValidateSession();
                if (!isValid)
                {
                    return Unauthorized(new { message = errorMessage });
                }

                _logger.LogInformation("Getting hall of fame for user: {Username}", username);
                var hallOfFame = await _codeRoastService.GetHallOfFameAsync();

                _logger.LogInformation("Hall of fame retrieved successfully");
                return Ok(hallOfFame);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting hall of fame");
                return StatusCode(500, new
                {
                    message = "Code Roast Server Error: " + ex.Message,
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
                _logger.LogInformation("Initializing user code roast stats");

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
                await _codeRoastService.InitializeUserStatsAsync(userId);

                _logger.LogInformation("Stats initialized successfully for user: {Username}", username);
                return Ok(new { message = "Code roast stats initialized successfully", username, userId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing user code roast stats");
                return StatusCode(500, new
                {
                    message = "Code Roast Server Error: " + ex.Message,
                    error = ex.Message,
                    type = ex.GetType().Name
                });
            }
        }
    }
}
