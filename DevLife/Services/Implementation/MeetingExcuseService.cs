using DevLife.Dtos;
using DevLife.Enums;
using DevLife.Services.Interfaces;
using System.Text;
using System.Text.Json;

namespace DevLife.Services.Implementation;

public class MeetingExcuseService : IMeetingExcuseService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _apiUrl = "https://api.openai.com/v1/chat/completions";
    private readonly ILogger<MeetingExcuseService> _logger;

    public MeetingExcuseService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<MeetingExcuseService> logger)
    {
        _httpClient = httpClient;
        _apiKey = configuration["OpenAI:ApiKey"] ?? throw new ArgumentNullException("OpenAI:ApiKey not configured");
        _logger = logger;

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }

    public async Task<AIMeetingExcuseResponseDto?> GenerateAIExcuseAsync(AIMeetingExcuseRequestDto request)
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

    public async Task<List<AIMeetingExcuseResponseDto>> GenerateBulkAIExcusesAsync(AIMeetingExcuseRequestDto request, int count = 3)
    {
        var excuses = new List<AIMeetingExcuseResponseDto>();

        for (int i = 0; i < count; i++)
        {
            var modifiedRequest = new AIMeetingExcuseRequestDto
            {
                Category = request.Category,
                Type = request.Type,
                TargetBelievability = request.TargetBelievability,
                Mood = request.Mood,
                Context = $"{request.Context} (variation {i + 1})",
                UserTechStack = request.UserTechStack,
                UserExperience = request.UserExperience
            };

            var excuse = await GenerateAIExcuseAsync(modifiedRequest);
            if (excuse != null)
            {
                excuses.Add(excuse);
            }

            // Small delay for variety
            await Task.Delay(100);
        }

        return excuses;
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
            [MeetingCategory.Retrospective] = "sprint retrospective with team reflection",
            [MeetingCategory.Planning] = "project planning session",
            [MeetingCategory.OneOnOne] = "one-on-one meeting with manager",
            [MeetingCategory.AllHands] = "company all-hands meeting",
            [MeetingCategory.Training] = "training or learning session"
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

        return $@"
You are the world's most creative developer excuse generator. Create hilarious, developer-focused excuses for avoiding meetings.

**Requirements:**
- **Meeting Type:** {request.Category} - {categoryDescriptions.GetValueOrDefault(request.Category, "general meeting")}
- **Excuse Style:** {request.Type} - {typeDescriptions.GetValueOrDefault(request.Type, "general excuse")}
- **Believability:** {request.TargetBelievability}/10 - {believabilityGuidance}
- **Mood:** {request.Mood ?? "funny"}
- **Tech Stack:** {request.UserTechStack ?? "General Developer"}
- **Experience:** {request.UserExperience ?? "Mid-level"}
- **Context:** {request.Context}

Use programming concepts, tech terminology, development tools, coding patterns, debugging scenarios, deployment issues, or developer lifestyle quirks.

**Response Format (JSON):**
{{
  ""excuseText"": ""The actual excuse text here"",
  ""category"": ""{request.Category}"",
  ""type"": ""{request.Type}"",
  ""believabilityScore"": {request.TargetBelievability ?? 7},
  ""reasoning"": ""Why this excuse works and when to use it"",
  ""tags"": [""relevant"", ""tags"", ""here""],
  ""humorLevel"": 8,
  ""usage"": ""Best delivery method and timing""
}}

Generate the perfect developer meeting excuse now:";
    }

    private async Task<string?> CallOpenAIAsync(string prompt)
    {
        var requestBody = new
        {
            model = "gpt-4",
            messages = new[]
            {
                new {
                    role = "system",
                    content = "You are a legendary developer excuse generator. Always respond with valid JSON."
                },
                new { role = "user", content = prompt }
            },
            max_tokens = 800,
            temperature = 0.9
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

            _logger.LogWarning("OpenAI API failed: {StatusCode}", response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling OpenAI API");
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
                    TechStackUsed = request.UserTechStack ?? "General",
                    HumorLevel = Math.Max(1, Math.Min(10, excuseData.HumorLevel)),
                    Usage = excuseData.Usage ?? "Use with confidence",
                    IsAIGenerated = true
                };
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse AI response");

            // Fallback excuse
            return new AIMeetingExcuseResponseDto
            {
                ExcuseText = "My AI excuse generator just threw a NullPointerException - how meta is that?",
                Category = request.Category,
                Type = request.Type,
                BelievabilityScore = request.TargetBelievability ?? 7,
                Reasoning = "Fallback excuse due to parsing error",
                Tags = new List<string> { "AI", "meta", "error" },
                TechStackUsed = "General",
                HumorLevel = 8,
                Usage = "Use when technology fails you",
                IsAIGenerated = true
            };
        }

        return null;
    }

    private string CleanJsonResponse(string response)
    {
        var clean = response.Trim();
        if (clean.StartsWith("```json")) clean = clean.Substring(7);
        if (clean.StartsWith("```")) clean = clean.Substring(3);
        if (clean.EndsWith("```")) clean = clean.Substring(0, clean.Length - 3);
        return clean.Trim();
    }

    // Helper classes
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
        public int BelievabilityScore { get; set; }
        public string? Reasoning { get; set; }
        public List<string>? Tags { get; set; }
        public int HumorLevel { get; set; }
        public string? Usage { get; set; }
    }
}
