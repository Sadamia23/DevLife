using DevLife.Database;
using DevLife.Dtos;
using DevLife.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevLife.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DevDatingController : ControllerBase
{
    private readonly IDevDatingService _datingService;
    private readonly ILogger<DevDatingController> _logger;
    private readonly ApplicationDbContext _context;

    public DevDatingController(IDevDatingService datingService, ILogger<DevDatingController> logger, ApplicationDbContext context)
    {
        _datingService = datingService;
        _logger = logger;
        _context = context;
    }

    [HttpPost("setup")]
    public async Task<IActionResult> SetupDatingProfile([FromBody] DatingSetupRequest request)
    {
        try
        {
            var userIdClaim = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var success = await _datingService.SetupDatingProfileAsync(userId, request);

            if (success)
            {
                return Ok(new { message = "Dating profile setup successfully!" });
            }

            return BadRequest(new { message = "Failed to setup dating profile" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting up dating profile");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("profiles")]
    public async Task<IActionResult> GetPotentialMatches()
    {
        try
        {
            var userIdClaim = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var hasProfile = await _datingService.HasDatingProfileAsync(userId);
            if (!hasProfile)
            {
                return BadRequest(new { message = "Please setup your dating profile first" });
            }

            var matches = await _datingService.GetPotentialMatchesAsync(userId);
            return Ok(matches);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting potential matches");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpPost("swipe")]
    public async Task<IActionResult> ProcessSwipe([FromBody] SwipeRequest request)
    {
        try
        {
            var userIdClaim = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (request.TargetUserId == userId)
            {
                return BadRequest(new { message = "Cannot swipe on yourself" });
            }

            var response = await _datingService.ProcessSwipeAsync(userId, request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing swipe");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("chat/{matchId:int}")]
    public async Task<IActionResult> GetChatHistory(int matchId)
    {
        try
        {
            var userIdClaim = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var isValidMatch = await _datingService.IsValidMatchAsync(userId, matchId);
            if (!isValidMatch)
            {
                return NotFound(new { message = "Match not found" });
            }

            var chatHistory = await _datingService.GetChatHistoryAsync(userId, matchId);
            return Ok(chatHistory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting chat history for match {MatchId}", matchId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpPost("chat/{matchId:int}")]
    public async Task<IActionResult> SendMessage(int matchId, [FromBody] SendMessageRequest request)
    {
        try
        {
            var userIdClaim = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var isValidMatch = await _datingService.IsValidMatchAsync(userId, matchId);
            if (!isValidMatch)
            {
                return NotFound(new { message = "Match not found" });
            }

            var response = await _datingService.SendMessageAsync(userId, matchId, request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message for match {MatchId}", matchId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("profile/status")]
    public async Task<IActionResult> GetProfileStatus()
    {
        try
        {
            var userIdClaim = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var hasProfile = await _datingService.HasDatingProfileAsync(userId);
            return Ok(new { hasProfile });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting profile status");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("matches")]
    public async Task<IActionResult> GetUserMatches()
    {
        try
        {
            var userIdClaim = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var matches = await _datingService.GetUserMatchesAsync(userId);
            return Ok(matches);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user matches");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}
