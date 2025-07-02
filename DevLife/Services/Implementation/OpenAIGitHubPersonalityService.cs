using DevLife.Dtos;
using DevLife.Services.Interfaces;
using System.Text;
using System.Text.Json;

namespace DevLife.Services.Implementation;

public class OpenAIGitHubPersonalityService : IAIGitHubPersonalityService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly string _apiKey;

    public OpenAIGitHubPersonalityService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _apiKey = _configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI API key not configured");

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }

    public async Task<PersonalityAnalysisResult> AnalyzePersonalityAsync(GitHubAnalysisData analysisData)
    {
        var prompt = BuildAnalysisPrompt(analysisData);

        var requestBody = new
        {
            model = "gpt-4o-mini",
            messages = new[]
            {
                new { role = "system", content = GetSystemPrompt() },
                new { role = "user", content = prompt }
            },
            temperature = 0.7,
            max_tokens = 2000
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var openAiResponse = JsonSerializer.Deserialize<OpenAIResponse>(responseContent);

            if (openAiResponse?.Choices?.Any() == true)
            {
                var analysisText = openAiResponse.Choices[0].Message.Content;
                return ParseAnalysisResult(analysisText, analysisData);
            }

            return GetFallbackResult(analysisData);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calling OpenAI API: {ex.Message}");
            return GetFallbackResult(analysisData);
        }
    }

    private string GetSystemPrompt()
    {
        return @"You are an expert developer personality analyst who studies GitHub repositories to determine coding personalities.

Analyze the provided GitHub data and respond with a JSON object containing:

1. **personalityType**: A fun, creative developer personality type in English (e.g., ""You Are Chaotic Debugger"", ""You Are Perfectionist Architect"", ""You Are Creative Hacker"")

2. **description**: A detailed explanation of this personality type (in English, 100-200 words)

3. **strengths**: Array of 3-5 key strengths based on the coding patterns

4. **weaknesses**: Array of 3-5 areas for improvement

5. **celebrityDevelopers**: Array of 3 celebrity developers with similar traits, each containing:
   - name: Developer's name
   - description: Brief description
   - gitHubUsername: Their GitHub username
   - reason: Why they're similar (50-100 words)
   - similarityScore: 1-100

6. **scores**: Object with detailed scores (0-100):
   - commitMessageQuality
   - codeCommentingScore  
   - variableNamingScore
   - projectStructureScore
   - overallScore

Base your analysis on commit patterns, repository structure, naming conventions, and project organization. Be creative but accurate. Make personality types fun and memorable while being constructive with feedback.

Respond ONLY with valid JSON, no additional text.";
    }

    private string BuildAnalysisPrompt(GitHubAnalysisData analysisData)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"GitHub Username: {analysisData.Username}");
        sb.AppendLine($"Total Repositories Analyzed: {analysisData.Repositories.Count}");
        sb.AppendLine();

        foreach (var repo in analysisData.Repositories.Take(5)) // Limit to avoid token limits
        {
            sb.AppendLine($"Repository: {repo.Name}");
            sb.AppendLine($"Language: {repo.PrimaryLanguage}");
            sb.AppendLine($"Stars: {repo.StarsCount}, Commits Analyzed: {repo.CommitCount}");
            sb.AppendLine($"Scores - Commit Quality: {repo.CommitQuality}, Naming: {repo.NamingScore}, Structure: {repo.StructureScore}");

            if (repo.Commits.Any())
            {
                sb.AppendLine("Sample Commit Messages:");
                foreach (var commit in repo.Commits.Take(5))
                {
                    sb.AppendLine($"- {commit.Commit?.Message ?? ""}");
                }
            }
            sb.AppendLine();
        }

        // Add aggregate statistics
        var avgCommitQuality = analysisData.Repositories.Any() ?
            (int)analysisData.Repositories.Average(r => r.CommitQuality) : 50;
        var avgNamingScore = analysisData.Repositories.Any() ?
            (int)analysisData.Repositories.Average(r => r.NamingScore) : 50;
        var avgStructureScore = analysisData.Repositories.Any() ?
            (int)analysisData.Repositories.Average(r => r.StructureScore) : 50;
        var avgCommentingScore = analysisData.Repositories.Any() ?
            (int)analysisData.Repositories.Average(r => r.CommentingScore) : 50;

        sb.AppendLine("Overall Statistics:");
        sb.AppendLine($"Average Commit Quality: {avgCommitQuality}");
        sb.AppendLine($"Average Naming Score: {avgNamingScore}");
        sb.AppendLine($"Average Structure Score: {avgStructureScore}");
        sb.AppendLine($"Average Commenting Score: {avgCommentingScore}");

        var primaryLanguages = analysisData.Repositories
            .GroupBy(r => r.PrimaryLanguage)
            .OrderByDescending(g => g.Count())
            .Take(3)
            .Select(g => $"{g.Key} ({g.Count()} repos)")
            .ToList();

        sb.AppendLine($"Primary Languages: {string.Join(", ", primaryLanguages)}");

        return sb.ToString();
    }

    private PersonalityAnalysisResult ParseAnalysisResult(string analysisText, GitHubAnalysisData analysisData)
    {
        try
        {
            // Try to extract JSON from the response
            var jsonStart = analysisText.IndexOf('{');
            var jsonEnd = analysisText.LastIndexOf('}');

            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var jsonText = analysisText.Substring(jsonStart, jsonEnd - jsonStart + 1);
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true
                };

                var result = JsonSerializer.Deserialize<AIPersonalityResponse>(jsonText, options);

                if (result != null)
                {
                    return new PersonalityAnalysisResult
                    {
                        PersonalityType = result.PersonalityType ?? "You Are Mysterious Coder",
                        Description = result.Description ?? "A developer with unique coding patterns.",
                        Strengths = result.Strengths ?? new List<string> { "Consistent coding", "Good problem solving" },
                        Weaknesses = result.Weaknesses ?? new List<string> { "Could improve documentation" },
                        CelebrityDevelopers = result.CelebrityDevelopers ?? GetDefaultCelebrityDevelopers(),
                        Scores = result.Scores ?? CalculateDefaultScores(analysisData)
                    };
                }
            }
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Error parsing AI response: {ex.Message}");
        }

        return GetFallbackResult(analysisData);
    }

    private PersonalityAnalysisResult GetFallbackResult(GitHubAnalysisData analysisData)
    {
        var personalityTypes = new[]
        {
            "You Are Creative Problem Solver",
            "You Are Methodical Architect",
            "You Are Chaotic Genius",
            "You Are Perfectionist Craftsman",
            "You Are Pragmatic Builder"
        };

        var random = new Random();
        var selectedType = personalityTypes[random.Next(personalityTypes.Length)];

        return new PersonalityAnalysisResult
        {
            PersonalityType = selectedType,
            Description = "You have a unique coding style that reflects your approach to problem-solving and software development.",
            Strengths = new List<string>
            {
                "Strong technical skills",
                "Consistent coding practices",
                "Good problem-solving approach"
            },
            Weaknesses = new List<string>
            {
                "Could improve code documentation",
                "Consider more consistent commit messages"
            },
            CelebrityDevelopers = GetDefaultCelebrityDevelopers(),
            Scores = CalculateDefaultScores(analysisData)
        };
    }

    private List<CelebrityDeveloper> GetDefaultCelebrityDevelopers()
    {
        return new List<CelebrityDeveloper>
        {
            new CelebrityDeveloper
            {
                Name = "John Resig",
                Description = "Creator of jQuery",
                GitHubUsername = "jeresig",
                Reason = "Both show innovative approaches to solving complex problems with elegant solutions.",
                SimilarityScore = 75
            },
            new CelebrityDeveloper
            {
                Name = "Evan You",
                Description = "Creator of Vue.js",
                GitHubUsername = "yyx990803",
                Reason = "Similar attention to developer experience and clean, maintainable code.",
                SimilarityScore = 70
            },
            new CelebrityDeveloper
            {
                Name = "Dan Abramov",
                Description = "React core team member",
                GitHubUsername = "gaearon",
                Reason = "Shares a thoughtful approach to software architecture and clear communication.",
                SimilarityScore = 68
            }
        };
    }

    private AnalysisScores CalculateDefaultScores(GitHubAnalysisData analysisData)
    {
        if (!analysisData.Repositories.Any())
        {
            return new AnalysisScores
            {
                CommitMessageQuality = 60,
                CodeCommentingScore = 55,
                VariableNamingScore = 70,
                ProjectStructureScore = 65,
                OverallScore = 62
            };
        }

        var commitQuality = (int)analysisData.Repositories.Average(r => r.CommitQuality);
        var commentingScore = (int)analysisData.Repositories.Average(r => r.CommentingScore);
        var namingScore = (int)analysisData.Repositories.Average(r => r.NamingScore);
        var structureScore = (int)analysisData.Repositories.Average(r => r.StructureScore);
        var overallScore = (commitQuality + commentingScore + namingScore + structureScore) / 4;

        return new AnalysisScores
        {
            CommitMessageQuality = commitQuality,
            CodeCommentingScore = commentingScore,
            VariableNamingScore = namingScore,
            ProjectStructureScore = structureScore,
            OverallScore = overallScore
        };
    }
}

// Response models for OpenAI API
public class OpenAIResponse
{
    public Choice[]? Choices { get; set; }
}

public class Choice
{
    public Message Message { get; set; } = new();
}

public class Message
{
    public string Content { get; set; } = string.Empty;
}

public class AIPersonalityResponse
{
    public string? PersonalityType { get; set; }
    public string? Description { get; set; }
    public List<string>? Strengths { get; set; }
    public List<string>? Weaknesses { get; set; }
    public List<CelebrityDeveloper>? CelebrityDevelopers { get; set; }
    public AnalysisScores? Scores { get; set; }
}
