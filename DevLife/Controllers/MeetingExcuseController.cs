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
    private readonly IAIMeetingExcuseService _aiService;
    private readonly ILogger<MeetingExcuseController> _logger;

    public MeetingExcuseController(
        IAIMeetingExcuseService aiService,
        ILogger<MeetingExcuseController> logger)
    {
        _aiService = aiService;
        _logger = logger;
    }

    [HttpPost("generate")]
    public async Task<ActionResult<AIMeetingExcuseResponseDto>> GenerateExcuse([FromBody] AIMeetingExcuseRequestDto request)
    {
        try
        {
            // Validate the request
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for generate excuse request");
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Generating AI meeting excuse for {Category} {Type}",
                request.Category, request.Type);

            // Set defaults if not provided
            request.TargetBelievability ??= 7;
            request.Mood ??= "funny";
            request.UserTechStack ??= "Full Stack Developer";
            request.UserExperience ??= "Mid-level";

            var excuse = await _aiService.GenerateExcuseAsync(request);

            if (excuse == null)
            {
                _logger.LogWarning("AI service returned null excuse");
                return BadRequest(new { message = "Failed to generate excuse. Please try again." });
            }

            _logger.LogInformation("Successfully generated AI excuse");
            return Ok(excuse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating AI meeting excuse");
            return StatusCode(500, new
            {
                message = "Internal server error while generating excuse",
                error = ex.Message
            });
        }
    }

    [HttpPost("generate/bulk")]
    public async Task<ActionResult<List<AIMeetingExcuseResponseDto>>> GenerateBulkExcuses(
        [FromBody] AIMeetingExcuseRequestDto request,
        [FromQuery] int count = 3)
    {
        try
        {
            // Validate the request
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for bulk excuse request");
                return BadRequest(ModelState);
            }

            if (count < 1 || count > 5)
            {
                return BadRequest(new { message = "Count must be between 1 and 5" });
            }

            _logger.LogInformation("Generating {Count} AI meeting excuses", count);

            // Set defaults
            request.TargetBelievability ??= 7;
            request.Mood ??= "funny";
            request.UserTechStack ??= "Full Stack Developer";
            request.UserExperience ??= "Mid-level";

            var excuses = await _aiService.GenerateBulkExcusesAsync(request, count);

            if (!excuses.Any())
            {
                _logger.LogWarning("AI service returned no excuses");
                return BadRequest(new { message = "Failed to generate any excuses. Please try again." });
            }

            _logger.LogInformation("Successfully generated {Count} AI excuses", excuses.Count);
            return Ok(excuses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating bulk AI excuses");
            return StatusCode(500, new
            {
                message = "Internal server error while generating bulk excuses",
                error = ex.Message
            });
        }
    }

    [HttpGet("categories")]
    public ActionResult<object[]> GetCategories()
    {
        var categories = Enum.GetValues<MeetingCategory>()
            .Select(c => new { value = (int)c, name = c.ToString(), label = GetCategoryLabel(c) })
            .ToArray();

        return Ok(categories);
    }

    [HttpGet("types")]
    public ActionResult<object[]> GetExcuseTypes()
    {
        var types = Enum.GetValues<ExcuseType>()
            .Select(t => new { value = (int)t, name = t.ToString(), label = GetTypeLabel(t) })
            .ToArray();

        return Ok(types);
    }

    [HttpGet("health")]
    public ActionResult<object> GetHealth()
    {
        return Ok(new
        {
            status = "healthy",
            service = "AI Meeting Excuse Generator",
            timestamp = DateTime.UtcNow,
            features = new[] { "AI Generation", "Bulk Generation", "Developer Humor" }
        });
    }

    private string GetCategoryLabel(MeetingCategory category) => category switch
    {
        MeetingCategory.DailyStandup => "🌅 Daily Standup",
        MeetingCategory.SprintPlanning => "📋 Sprint Planning",
        MeetingCategory.ClientMeeting => "👔 Client Meeting",
        MeetingCategory.TeamBuilding => "🤝 Team Building",
        MeetingCategory.CodeReview => "🔍 Code Review",
        MeetingCategory.Retrospective => "🔄 Retrospective",
        MeetingCategory.Planning => "📝 Planning",
        MeetingCategory.OneOnOne => "👥 One-on-One",
        MeetingCategory.AllHands => "👐 All Hands",
        MeetingCategory.Training => "📚 Training",
        _ => category.ToString()
    };

    private string GetTypeLabel(ExcuseType type) => type switch
    {
        ExcuseType.Technical => "💻 Technical Issues",
        ExcuseType.Personal => "👤 Personal Matters",
        ExcuseType.Creative => "🎨 Creative Block",
        ExcuseType.Health => "🏥 Health Related",
        ExcuseType.Emergency => "🚨 Emergency",
        ExcuseType.Mysterious => "❓ Mysterious",
        _ => type.ToString()
    };
}
