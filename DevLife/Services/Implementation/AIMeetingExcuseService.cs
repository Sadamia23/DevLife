using DevLife.Database;
using DevLife.Dtos;
using DevLife.Enums;
using DevLife.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace DevLife.Services.Implementation;

public class OpenAIMeetingExcuseService : IAIMeetingExcuseService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _apiUrl = "https://api.openai.com/v1/chat/completions";
    private readonly ILogger<OpenAIMeetingExcuseService> _logger;
    private readonly ApplicationDbContext _context;

    public OpenAIMeetingExcuseService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<OpenAIMeetingExcuseService> logger,
        ApplicationDbContext context)
    {
        _httpClient = httpClient;
        _apiKey = configuration["OpenAI:ApiKey"] ?? throw new ArgumentNullException("OpenAI:ApiKey not configured");
        _logger = logger;
        _context = context;

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }

    public async Task<AIMeetingExcuseResponseDto?> GenerateExcuseAsync(AIMeetingExcuseRequestDto request)
    {
        try
        {
            var prompt = BuildExcuseGenerationPrompt(request);
            var response = await CallOpenAIAsync(prompt);

            if (response != null)
            {
                return ParseExcuseResponse(response, request);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating AI meeting excuse for {Category} {Type}",
                request.Category, request.Type);
            return null;
        }
    }

    public async Task<List<AIMeetingExcuseResponseDto>> GenerateBulkExcusesAsync(AIMeetingExcuseRequestDto request, int count = 3)
    {
        var excuses = new List<AIMeetingExcuseResponseDto>();

        // Generate multiple excuses with slight variations
        for (int i = 0; i < count; i++)
        {
            var modifiedRequest = request;

            // Add variation to get different results
            if (i > 0)
            {
                modifiedRequest.Context = $"{request.Context} (variation {i + 1})";
            }

            var excuse = await GenerateExcuseAsync(modifiedRequest);
            if (excuse != null)
            {
                excuses.Add(excuse);
            }

            // Small delay to get varied responses
            await Task.Delay(100);
        }

        return excuses;
    }

    public async Task<AIMeetingExcuseResponseDto?> GeneratePersonalizedExcuseAsync(string username, AIMeetingExcuseRequestDto request)
    {
        try
        {
            // Get user info for personalization
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user != null)
            {
                request.UserTechStack = user.TechStack.ToString();
                request.UserExperience = user.Experience.ToString();
            }

            // Get user's recent excuse usage patterns
            var recentUsage = await _context.MeetingExcuseUsages
                .Include(u => u.MeetingExcuse)
                .Where(u => u.User.Username == username)
                .OrderByDescending(u => u.UsedAt)
                .Take(5)
                .Select(u => u.MeetingExcuse.Type.ToString())
                .ToListAsync();

            // Avoid recently used types
            request.Context = $"{request.Context} (User recently used: {string.Join(", ", recentUsage)})";

            return await GenerateExcuseAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating personalized excuse for user {Username}", username);
            return await GenerateExcuseAsync(request); // Fallback to non-personalized
        }
    }

    private string BuildExcuseGenerationPrompt(AIMeetingExcuseRequestDto request)
    {
        var categoryDescriptions = new Dictionary<MeetingCategory, string>
        {
            [MeetingCategory.DailyStandup] = "daily standup/scrum meeting with quick status updates",
            [MeetingCategory.SprintPlanning] = "sprint planning session with story estimation and task breakdown",
            [MeetingCategory.ClientMeeting] = "client meeting or presentation with external stakeholders",
            [MeetingCategory.TeamBuilding] = "team building activity or social work event",
            [MeetingCategory.CodeReview] = "code review session with peer feedback",
            [MeetingCategory.Retrospective] = "sprint retrospective with team reflection"
        };

        var typeDescriptions = new Dictionary<ExcuseType, string>
        {
            [ExcuseType.Technical] = "technical issue or system problem (servers, code, tools)",
            [ExcuseType.Personal] = "personal situation but professional sounding",
            [ExcuseType.Creative] = "wildly imaginative and absurd but developer-themed",
            [ExcuseType.Health] = "developer-specific health issues (RSI, eye strain, etc.)",
            [ExcuseType.Emergency] = "urgent technical emergency requiring immediate attention",
            [ExcuseType.Mysterious] = "vague and mysterious but technically plausible"
        };

        var believabilityGuidance = request.TargetBelievability switch
        {
            >= 8 => "Make it very believable - something that could actually happen to a developer",
            >= 6 => "Make it somewhat believable - stretches reality but plausible in tech world",
            >= 4 => "Make it obviously fake but amusing - clearly an excuse but entertaining",
            _ => "Make it absurdly unbelievable but hilarious - pure comedy gold"
        };

        var moodGuidance = request.Mood?.ToLower() switch
        {
            "desperate" => "Sound genuinely desperate and pleading",
            "professional" => "Maintain professional tone while being creative",
            "confident" => "Sound completely confident and matter-of-fact",
            _ => "Be naturally funny and developer-relatable"
        };

        return $@"
You are the world's most creative developer excuse generator. You specialize in creating hilarious, developer-focused excuses for avoiding meetings. Your excuses are legendary in the programming community.

**Excuse Requirements:**
- **Meeting Type:** {request.Category} - {categoryDescriptions.GetValueOrDefault(request.Category, "general meeting")}
- **Excuse Style:** {request.Type} - {typeDescriptions.GetValueOrDefault(request.Type, "general excuse")}
- **Believability Target:** {request.TargetBelievability}/10 - {believabilityGuidance}
- **Developer Context:** {request.UserTechStack} developer with {request.UserExperience} experience
- **Mood:** {moodGuidance}
- **Additional Context:** {request.Context}

**Developer Culture References:**
Use programming concepts, tech stack terminology, development tools, coding patterns, software engineering principles, debugging scenarios, deployment issues, version control problems, infrastructure challenges, or developer lifestyle quirks.

**Creativity Guidelines:**
- Reference specific technologies, frameworks, or tools
- Use programming terminology naturally
- Include developer pain points (merge conflicts, production bugs, deadlines)
- Make it relatable to real developer experiences
- Add technical details that make it sound authentic
- Include humor that only developers would truly appreciate

**Avoid these if specified:** {string.Join(", ", request.AvoidKeywords ?? new List<string>())}

**Response Format (JSON):**
{{
  ""excuseText"": ""The actual excuse text here"",
  ""category"": ""{request.Category}"",
  ""type"": ""{request.Type}"",
  ""believabilityScore"": {request.TargetBelievability ?? 7},
  ""reasoning"": ""Why this excuse works and when to use it"",
  ""tags"": [""relevant"", ""tags"", ""for"", ""filtering""],
  ""techStackUsed"": ""{request.UserTechStack}"",
  ""humorLevel"": 8,
  ""usage"": ""Best delivery method and timing for this excuse""
}}

**Examples of Great Developer Excuses:**
- ""My rubber duck debugging session has evolved into a philosophical debate about the nature of existence""
- ""I'm experiencing a stack overflow in my social interaction buffer""
- ""Git blame shows this meeting was my fault, but I'm currently in a detached HEAD state""
- ""My work-life balance microservice is down for emergency maintenance""

**Important:** 
- Keep it developer-focused and tech-savvy
- Make it genuinely funny, not just absurd
- Ensure it fits the meeting type and excuse style
- Balance humor with the requested believability level
- Make developers reading it think ""I wish I thought of that!""

Generate the perfect developer meeting excuse now:";
    }

    private async Task<string?> CallOpenAIAsync(string prompt)
    {
        var requestBody = new
        {
            model = "gpt-4", // Using GPT-4 for better creativity and humor
            messages = new[]
            {
                new {
                    role = "system",
                    content = "You are a legendary developer excuse generator with years of experience in tech humor. You understand developer culture deeply and create excuses that make developers laugh while still being practical. Always respond with valid JSON."
                },
                new { role = "user", content = prompt }
            },
            max_tokens = 800,
            temperature = 0.9, // High temperature for maximum creativity
            presence_penalty = 0.3, // Encourage variety
            frequency_penalty = 0.3 // Avoid repetition
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync(_apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var openAIResponse = JsonSerializer.Deserialize<OpenAIResponse>(responseContent);

                return openAIResponse?.choices?.FirstOrDefault()?.message?.content;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("OpenAI API call failed with status: {StatusCode}, Error: {Error}",
                    response.StatusCode, errorContent);
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error calling OpenAI API");
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout calling OpenAI API");
        }

        return null;
    }

    private AIMeetingExcuseResponseDto? ParseExcuseResponse(string response, AIMeetingExcuseRequestDto request)
    {
        try
        {
            var cleanResponse = CleanJsonResponse(response);
            var excuseData = JsonSerializer.Deserialize<AIExcuseData>(cleanResponse, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (excuseData != null && !string.IsNullOrEmpty(excuseData.ExcuseText))
            {
                return new AIMeetingExcuseResponseDto
                {
                    ExcuseText = excuseData.ExcuseText,
                    Category = request.Category,
                    Type = request.Type,
                    BelievabilityScore = Math.Max(1, Math.Min(10, excuseData.BelievabilityScore)),
                    Reasoning = excuseData.Reasoning ?? "AI-generated excuse",
                    Tags = excuseData.Tags ?? new List<string>(),
                    TechStackUsed = excuseData.TechStackUsed ?? request.UserTechStack ?? "General",
                    HumorLevel = Math.Max(1, Math.Min(10, excuseData.HumorLevel)),
                    Usage = excuseData.Usage ?? "Use with confidence",
                    IsAIGenerated = true
                };
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse AI excuse response: {Response}", response);

            // Fallback: try to extract excuse text manually
            if (response.Contains("excuseText"))
            {
                return CreateFallbackExcuse(response, request);
            }
        }

        return null;
    }

    private AIMeetingExcuseResponseDto CreateFallbackExcuse(string response, AIMeetingExcuseRequestDto request)
    {
        // Extract excuse text manually if JSON parsing fails
        var excuseText = "My AI excuse generator experienced a parsing error - how meta is that?";

        // Try to extract the excuse text between quotes
        var startIndex = response.IndexOf("\"excuseText\":");
        if (startIndex != -1)
        {
            var textStart = response.IndexOf("\"", startIndex + 13);
            if (textStart != -1)
            {
                var textEnd = response.IndexOf("\"", textStart + 1);
                if (textEnd != -1)
                {
                    excuseText = response.Substring(textStart + 1, textEnd - textStart - 1);
                }
            }
        }

        return new AIMeetingExcuseResponseDto
        {
            ExcuseText = excuseText,
            Category = request.Category,
            Type = request.Type,
            BelievabilityScore = request.TargetBelievability ?? 7,
            Reasoning = "Backup excuse due to AI parsing issues",
            Tags = new List<string> { "AI", "fallback", "meta" },
            TechStackUsed = request.UserTechStack ?? "General",
            HumorLevel = 6,
            Usage = "Use when all else fails",
            IsAIGenerated = true
        };
    }

    private string CleanJsonResponse(string response)
    {
        var cleanResponse = response.Trim();

        // Remove markdown code blocks
        if (cleanResponse.StartsWith("```json"))
        {
            cleanResponse = cleanResponse.Substring(7);
        }
        else if (cleanResponse.StartsWith("```"))
        {
            cleanResponse = cleanResponse.Substring(3);
        }

        if (cleanResponse.EndsWith("```"))
        {
            cleanResponse = cleanResponse.Substring(0, cleanResponse.Length - 3);
        }

        return cleanResponse.Trim();
    }

    // Helper classes for JSON deserialization
    private class OpenAIResponse
    {
        public Choice[]? choices { get; set; }
    }

    private class Choice
    {
        public Message? message { get; set; }
    }

    private class Message
    {
        public string? content { get; set; }
    }

    private class AIExcuseData
    {
        public string ExcuseText { get; set; } = string.Empty;
        public string? Category { get; set; }
        public string? Type { get; set; }
        public int BelievabilityScore { get; set; }
        public string? Reasoning { get; set; }
        public List<string>? Tags { get; set; }
        public string? TechStackUsed { get; set; }
        public int HumorLevel { get; set; }
        public string? Usage { get; set; }
    }
}
