using DevLife.Services.Interfaces;
using System.Text;
using System.Text.Json;

namespace DevLife.Services.Implementation;

public class OpenAIDatingService : IAIDatingService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OpenAIDatingService> _logger;

    private readonly Dictionary<string, string> _personalities = new()
    {
        { "Enthusiastic", "You are an enthusiastic and energetic developer who loves coding and is always excited about new technologies. You use lots of emojis and exclamation marks!" },
        { "Sarcastic", "You are a witty and slightly sarcastic developer who makes clever jokes about programming and tech culture. You're charming but have a dry sense of humor." },
        { "Helpful", "You are a kind and helpful developer who enjoys mentoring others. You're patient, understanding, and always willing to share knowledge." },
        { "Ambitious", "You are a driven and ambitious developer focused on career growth and technical excellence. You're passionate about solving complex problems." },
        { "Casual", "You are a laid-back developer who enjoys coding but also values work-life balance. You're friendly and easy-going in conversations." }
    };

    public OpenAIDatingService(HttpClient httpClient, IConfiguration configuration, ILogger<OpenAIDatingService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;

        var apiKey = _configuration["OpenAI:ApiKey"];
        if (!string.IsNullOrEmpty(apiKey))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
        }
    }

    public async Task<string> GenerateResponseAsync(string userMessage, string context, string personality)
    {
        try
        {
            // If OpenAI is not configured, use fallback responses
            var apiKey = _configuration["OpenAI:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                return GenerateFallbackResponse(userMessage, personality);
            }

            var systemPrompt = $@"
{_personalities.GetValueOrDefault(personality, _personalities["Casual"])}

Context about you: {context}

You are chatting with someone on a developer dating app. Keep responses:
- Under 100 words
- Flirty but professional
- Related to coding/tech when appropriate
- Natural and conversational
- Don't mention you're AI

Respond to their message naturally as if you're a real person interested in getting to know them.";

            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userMessage }
                },
                max_tokens = 150,
                temperature = 0.8
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

                if (jsonResponse.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                {
                    var firstChoice = choices[0];
                    if (firstChoice.TryGetProperty("message", out var message) &&
                        message.TryGetProperty("content", out var messageContent))
                    {
                        return messageContent.GetString() ?? GenerateFallbackResponse(userMessage, personality);
                    }
                }
            }

            _logger.LogWarning("OpenAI API call failed, using fallback response");
            return GenerateFallbackResponse(userMessage, personality);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating AI response");
            return GenerateFallbackResponse(userMessage, personality);
        }
    }

    public async Task<string> GetPersonalityForUserAsync(int userId)
    {
        // Simple personality assignment based on user ID
        // In a real implementation, you might store this in the database
        var personalities = _personalities.Keys.ToArray();
        var personalityIndex = userId % personalities.Length;
        return personalities[personalityIndex];
    }

    private string GenerateFallbackResponse(string userMessage, string personality)
    {
        var random = new Random();

        var responses = personality switch
        {
            "Enthusiastic" => new[]
            {
                "That's awesome! 🚀 I love talking about that stuff too!",
                "OMG yes! 🔥 Tell me more about your experience with that!",
                "So cool! 😄 I'm really excited to learn more about you!",
                "Amazing! 🌟 I think we have a lot in common!"
            },
            "Sarcastic" => new[]
            {
                "Oh, another developer who thinks they're special? 😏 Just kidding, tell me more!",
                "Well, that's refreshingly honest. Most people lie in their bios 😉",
                "Interesting... and here I thought all developers were exactly the same 🙃",
                "Bold choice sharing that! I respect the honesty 😏"
            },
            "Helpful" => new[]
            {
                "That sounds really interesting! I'd love to help if you ever want to discuss it further.",
                "I appreciate you sharing that. I'm here if you ever want to chat about tech or anything else!",
                "That's great! I'm always happy to share knowledge and learn from others too.",
                "Thanks for telling me that! I love connecting with fellow developers."
            },
            "Ambitious" => new[]
            {
                "That's exactly the kind of drive I respect! What's your next big goal?",
                "I love meeting people who are passionate about growth. Tell me about your projects!",
                "That sounds like an exciting challenge! I'm working on some ambitious projects too.",
                "Your passion for development really shows! I'd love to hear about your achievements."
            },
            _ => new[]
            {
                "That's pretty cool! I'm enjoying getting to know you better.",
                "Nice! I can tell we might have some good conversations ahead.",
                "Sounds good to me! I like your style.",
                "That's interesting! I'm curious to hear more about your perspective."
            }
        };

        return responses[random.Next(responses.Length)];
    }
}
