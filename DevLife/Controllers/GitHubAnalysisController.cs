using DevLife.Dtos;
using DevLife.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace DevLife.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GitHubAnalysisController : ControllerBase
{
    private readonly IGitHubAuthService _gitHubAuthService;
    private readonly IGitHubAnalysisService _gitHubAnalysisService;
    private readonly ILogger<GitHubAnalysisController> _logger;

    public GitHubAnalysisController(
        IGitHubAuthService gitHubAuthService,
        IGitHubAnalysisService gitHubAnalysisService,
        ILogger<GitHubAnalysisController> logger)
    {
        _gitHubAuthService = gitHubAuthService;
        _gitHubAnalysisService = gitHubAnalysisService;
        _logger = logger;
    }

    [HttpGet("auth/login")]
    public IActionResult Login([FromQuery] int userId)
    {
        try
        {
            _logger.LogInformation("Initiating GitHub login for user {UserId}", userId);
            var state = GenerateState(userId);
            var authUrl = _gitHubAuthService.GetAuthorizationUrl(state);

            return Ok(new { authUrl, state });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initiate GitHub authentication for user {UserId}", userId);
            return BadRequest(new { error = "Failed to initiate GitHub authentication", details = ex.Message });
        }
    }

    [HttpGet("auth/callback")]
    public async Task<IActionResult> Callback([FromQuery] string code, [FromQuery] string state)
    {
        try
        {
            _logger.LogInformation("Processing GitHub callback with state: {State}", state);

            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
            {
                return BadRequest(new { error = "Missing code or state parameter" });
            }

            var userId = ValidateAndExtractUserId(state);
            if (userId == 0)
            {
                return BadRequest(new { error = "Invalid state parameter" });
            }

            var accessToken = await _gitHubAuthService.ExchangeCodeForTokenAsync(code, state);

            if (string.IsNullOrEmpty(accessToken))
            {
                return BadRequest(new { error = "Failed to obtain access token" });
            }

            HttpContext.Session.SetString($"github_token_{userId}", accessToken);

            var userInfoJson = await _gitHubAuthService.GetUserInfoAsync(accessToken);
            var userInfo = JsonSerializer.Deserialize<GitHubUserInfo>(userInfoJson);

            _logger.LogInformation("GitHub authentication successful for user {UserId}, GitHub user: {GitHubUsername}", userId, userInfo?.Login);

            return Ok(new
            {
                success = true,
                message = "GitHub authentication successful",
                githubUsername = userInfo?.Login,
                githubId = userInfo?.Id
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GitHub authentication callback failed");
            return BadRequest(new { error = "Authentication failed", details = ex.Message });
        }
    }

    [HttpPost("analyze")]
    public async Task<IActionResult> AnalyzeRepositories([FromBody] GitHubAnalysisRequestWithUser request)
    {
        try
        {
            _logger.LogInformation("Starting repository analysis for user {UserId}, GitHub username: {GitHubUsername}",
                request.UserId, request.GitHubUsername);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var accessToken = HttpContext.Session.GetString($"github_token_{request.UserId}");
            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogWarning("GitHub authentication required for user {UserId}", request.UserId);
                return Unauthorized(new { error = "GitHub authentication required. Please authenticate first." });
            }

            var isValidToken = await _gitHubAuthService.ValidateTokenAsync(accessToken);
            if (!isValidToken)
            {
                _logger.LogWarning("GitHub token expired for user {UserId}", request.UserId);
                HttpContext.Session.Remove($"github_token_{request.UserId}");
                return Unauthorized(new { error = "GitHub token expired. Please re-authenticate." });
            }

            var analysisRequest = new GitHubAnalysisRequest
            {
                GitHubUsername = request.GitHubUsername,
                MaxRepositories = request.MaxRepositories,
                IncludeForkedRepos = request.IncludeForkedRepos,
                AnalyzePrivateRepos = request.AnalyzePrivateRepos
            };

            var result = await _gitHubAnalysisService.AnalyzeUserRepositoriesAsync(
                request.UserId,
                analysisRequest,
                accessToken);

            _logger.LogInformation("Repository analysis completed successfully for user {UserId}, analysis ID: {AnalysisId}",
                request.UserId, result.Id);

            // Create response without calling GenerateShareableCardAsync during analysis
            // The shareable card URL can be generated later when needed
            var response = new
            {
                success = true,
                data = result,
                message = "Analysis completed successfully! 🎉"
            };

            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Insufficient GitHub permissions for user {UserId}: {Error}", request.UserId, ex.Message);
            return Unauthorized(new { error = "Insufficient GitHub permissions for the requested analysis." });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "GitHub API error during analysis for user {UserId}", request.UserId);
            return StatusCode(502, new
            {
                error = "GitHub API error",
                details = ex.Message,
                message = "Unable to communicate with GitHub. Please try again later."
            });
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON serialization error during analysis for user {UserId}", request.UserId);
            return StatusCode(500, new
            {
                error = "Data processing error",
                details = "Failed to process analysis data",
                message = "Sorry! There was an issue processing your analysis data. Please try again."
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Invalid operation during analysis for user {UserId}: {Error}", request.UserId, ex.Message);
            return BadRequest(new
            {
                error = "Analysis configuration error",
                details = ex.Message,
                message = "Please check your analysis settings and try again."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Analysis failed for user {UserId}: {Error}", request.UserId, ex.Message);
            return StatusCode(500, new
            {
                error = "Analysis failed",
                details = ex.Message,
                message = "Sorry! Something went wrong during the analysis. Please try again."
            });
        }
    }

    [HttpGet("{analysisId}")]
    public async Task<IActionResult> GetAnalysisResult(int analysisId, [FromQuery] int userId)
    {
        try
        {
            _logger.LogDebug("Getting analysis result {AnalysisId} for user {UserId}", analysisId, userId);
            var result = await _gitHubAnalysisService.GetAnalysisResultAsync(analysisId, userId);

            if (result == null)
            {
                return NotFound(new { error = "Analysis not found" });
            }

            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve analysis {AnalysisId}", analysisId);
            return StatusCode(500, new { error = "Failed to retrieve analysis", details = ex.Message });
        }
    }

    [HttpGet("user/{userId}/history")]
    public async Task<IActionResult> GetUserAnalysisHistory(int userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            if (pageSize > 50) pageSize = 50;

            _logger.LogDebug("Getting analysis history for user {UserId}, page {Page}, size {PageSize}", userId, page, pageSize);
            var results = await _gitHubAnalysisService.GetUserAnalysisHistoryAsync(userId, page, pageSize);

            return Ok(new
            {
                success = true,
                data = results,
                page,
                pageSize,
                hasMore = results.Count == pageSize
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve analysis history for user {UserId}", userId);
            return StatusCode(500, new { error = "Failed to retrieve analysis history", details = ex.Message });
        }
    }

    [HttpDelete("{analysisId}")]
    public async Task<IActionResult> DeleteAnalysis(int analysisId, [FromQuery] int userId)
    {
        try
        {
            _logger.LogInformation("Deleting analysis {AnalysisId} for user {UserId}", analysisId, userId);
            var success = await _gitHubAnalysisService.DeleteAnalysisAsync(analysisId, userId);

            if (!success)
            {
                return NotFound(new { error = "Analysis not found or access denied" });
            }

            return Ok(new { success = true, message = "Analysis deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete analysis {AnalysisId}", analysisId);
            return StatusCode(500, new { error = "Failed to delete analysis", details = ex.Message });
        }
    }

    [HttpPost("{analysisId}/favorite")]
    public async Task<IActionResult> ToggleFavorite(int analysisId, [FromQuery] int userId)
    {
        try
        {
            _logger.LogDebug("Toggling favorite for analysis {AnalysisId}, user {UserId}", analysisId, userId);
            var isFavorited = await _gitHubAnalysisService.ToggleFavoriteAsync(analysisId, userId);

            return Ok(new
            {
                success = true,
                isFavorited,
                message = isFavorited ? "Added to favorites" : "Removed from favorites"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to toggle favorite for analysis {AnalysisId}", analysisId);
            return StatusCode(500, new { error = "Failed to toggle favorite", details = ex.Message });
        }
    }

    [HttpGet("{analysisId}/share-card")]
    public async Task<IActionResult> GetShareableCard(int analysisId)
    {
        try
        {
            _logger.LogDebug("Generating shareable card for analysis {AnalysisId}", analysisId);
            var cardUrl = await _gitHubAnalysisService.GenerateShareableCardAsync(analysisId);

            return Ok(new
            {
                success = true,
                shareableCardUrl = cardUrl,
                socialShareUrls = new
                {
                    twitter = $"https://twitter.com/intent/tweet?text=Check out my developer personality analysis!&url={Uri.EscapeDataString(cardUrl)}",
                    linkedin = $"https://www.linkedin.com/sharing/share-offsite/?url={Uri.EscapeDataString(cardUrl)}",
                    facebook = $"https://www.facebook.com/sharer/sharer.php?u={Uri.EscapeDataString(cardUrl)}"
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate shareable card for analysis {AnalysisId}", analysisId);
            return StatusCode(500, new { error = "Failed to generate shareable card", details = ex.Message });
        }
    }

    [HttpGet("auth/status")]
    public async Task<IActionResult> GetAuthStatus([FromQuery] int userId)
    {
        try
        {
            var accessToken = HttpContext.Session.GetString($"github_token_{userId}");

            if (string.IsNullOrEmpty(accessToken))
            {
                return Ok(new { isAuthenticated = false, message = "Not authenticated with GitHub" });
            }

            var isValid = await _gitHubAuthService.ValidateTokenAsync(accessToken);

            if (!isValid)
            {
                HttpContext.Session.Remove($"github_token_{userId}");
                return Ok(new { isAuthenticated = false, message = "GitHub token expired" });
            }

            var userInfoJson = await _gitHubAuthService.GetUserInfoAsync(accessToken);
            var userInfo = JsonSerializer.Deserialize<GitHubUserInfo>(userInfoJson);

            return Ok(new
            {
                isAuthenticated = true,
                githubUsername = userInfo?.Login,
                avatarUrl = userInfo?.AvatarUrl,
                message = "Authenticated with GitHub"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check authentication status for user {UserId}", userId);
            return StatusCode(500, new { error = "Failed to check authentication status", details = ex.Message });
        }
    }

    [HttpPost("auth/logout")]
    public IActionResult Logout([FromQuery] int userId)
    {
        try
        {
            _logger.LogInformation("Logging out user {UserId} from GitHub", userId);
            HttpContext.Session.Remove($"github_token_{userId}");
            return Ok(new { success = true, message = "Logged out from GitHub successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to logout user {UserId}", userId);
            return StatusCode(500, new { error = "Failed to logout", details = ex.Message });
        }
    }

    #region Private Methods

    private string GenerateState(int userId)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var random = Guid.NewGuid().ToString("N")[..8];
        return $"{userId}_{timestamp}_{random}";
    }

    private int ValidateAndExtractUserId(string state)
    {
        try
        {
            var parts = state.Split('_');
            if (parts.Length >= 3 && int.TryParse(parts[0], out var userId))
            {
                return userId;
            }
        }
        catch
        {
            // Log the error but don't expose details
        }

        return 0;
    }

    #endregion
}

public class GitHubAnalysisRequestWithUser : GitHubAnalysisRequest
{
    public int UserId { get; set; }
}

public class GitHubUserInfo
{
    public int Id { get; set; }
    public string Login { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public int PublicRepos { get; set; }
    public int Followers { get; set; }
    public int Following { get; set; }
}