using DevLife.Dtos;
using DevLife.Enums;
using DevLife.Services.Interfaces;
using System.Text;
using System.Text.Json;

namespace DevLife.Services.Implementation;

public class OpenAICodeRoastService : IAICodeRoastService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _apiUrl = "https://api.openai.com/v1/chat/completions";
    private readonly ILogger<OpenAICodeRoastService> _logger;

    public OpenAICodeRoastService(HttpClient httpClient, IConfiguration configuration, ILogger<OpenAICodeRoastService> logger)
    {
        _httpClient = httpClient;
        _apiKey = configuration["OpenAI:ApiKey"] ?? throw new ArgumentNullException("OpenAI:ApiKey not configured");
        _logger = logger;

        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }

    public async Task<AICodeTaskResponseDto?> GenerateCodeTaskAsync(AICodeTaskRequestDto request)
    {
        try
        {
            var prompt = BuildTaskGenerationPrompt(request);
            var response = await CallOpenAIAsync(prompt);

            if (response != null)
            {
                return ParseTaskResponse(response, request.TechStack, request.DifficultyLevel);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating AI coding task for {TechStack} {DifficultyLevel}",
                request.TechStack, request.DifficultyLevel);
            return null;
        }
    }

    public async Task<AICodeEvaluationResponseDto?> EvaluateCodeAsync(AICodeEvaluationRequestDto request)
    {
        try
        {
            var prompt = BuildEvaluationPrompt(request);
            var response = await CallOpenAIAsync(prompt);

            if (response != null)
            {
                return ParseEvaluationResponse(response);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating code with AI");
            return null;
        }
    }

    private string BuildTaskGenerationPrompt(AICodeTaskRequestDto request)
    {
        var topics = GetTopicsForTechStack(request.TechStack, request.DifficultyLevel);
        var selectedTopic = request.SpecificTopic ?? topics[Random.Shared.Next(topics.Length)];

        return $@"
You are a coding task generator for a humorous developer platform called 'Code Roast'. Generate a practical coding challenge with the following specifications:

**Technology Stack:** {request.TechStack}
**Difficulty Level:** {request.DifficultyLevel}
**Topic:** {selectedTopic}

**Requirements:**
1. Create a realistic coding problem that a {request.DifficultyLevel.ToString().ToLower()} {request.TechStack} developer would encounter
2. The task should be solvable in 15-60 minutes depending on difficulty
3. Include clear requirements and examples
4. Provide starter code if helpful (optional)
5. Include 2-3 test cases to validate the solution
6. Make it practical and relevant to real-world development

**Difficulty Guidelines:**
- Junior: Basic syntax, simple algorithms, fundamental concepts
- Middle: Intermediate algorithms, design patterns, optimization
- Senior: Complex problem-solving, architecture decisions, advanced patterns

**Response Format (JSON):**
{{
  ""title"": ""Task title"",
  ""description"": ""Detailed problem description"",
  ""requirements"": ""Specific requirements and constraints"",
  ""starterCode"": ""Optional starter code template"",
  ""testCases"": [""Test case 1"", ""Test case 2"", ""Test case 3""],
  ""examples"": [""Example input/output 1"", ""Example input/output 2""],
  ""estimatedMinutes"": 30,
  ""topic"": ""{selectedTopic}""
}}

**Important:** 
- Keep the task focused and achievable
- Provide clear success criteria
- Make examples helpful but not giving away the solution
- Ensure the task matches the specified difficulty level

Generate the coding task now:";
    }

    private string BuildEvaluationPrompt(AICodeEvaluationRequestDto request)
    {
        var roastPersonalities = new[]
        {
            "sarcastic code reviewer",
            "brutally honest programming mentor",
            "witty senior developer",
            "comedic technical lead",
            "savage code auditor"
        };

        var personality = roastPersonalities[Random.Shared.Next(roastPersonalities.Length)];

        return $@"
You are a {personality} for the 'Code Roast' platform. Your job is to evaluate submitted code and provide humorous but constructive feedback.

**Task Description:** {request.TaskDescription}
**Technology Stack:** {request.TechStack}
**Difficulty Level:** {request.DifficultyLevel}

**Code to Evaluate:**
```
{request.Code}
```

**Evaluation Criteria:**
1. **Correctness** (0-100): Does it solve the problem correctly?
2. **Readability** (0-100): Is the code clean and understandable?
3. **Performance** (0-100): Is it efficient and well-optimized?
4. **Best Practices** (0-100): Does it follow language/framework conventions?

**Roasting Guidelines:**
- Score 90-100: Light praise with gentle teasing (""Not bad, but my grandmother codes faster"")
- Score 70-89: Constructive criticism with humor (""This works, but it's uglier than a null pointer exception"")
- Score 50-69: More serious roasting (""This code is so messy, even the bugs are confused"")
- Score 0-49: Brutal but fair roasting (""This code is so bad, the compiler filed a restraining order"")

**Response Format (JSON):**
{{
  ""overallScore"": 85,
  ""roastMessage"": ""Humorous roast message here"",
  ""technicalFeedback"": ""Serious technical feedback here"",
  ""qualityAssessment"": {{
    ""readabilityScore"": 80,
    ""performanceScore"": 75,
    ""correctnessScore"": 90,
    ""bestPracticesScore"": 85,
    ""positivePoints"": [""Good variable naming"", ""Proper error handling""],
    ""improvementPoints"": [""Could optimize the loop"", ""Add more comments""],
    ""redFlags"": [""Memory leak potential"", ""Missing input validation""],
    ""codeStyle"": ""Clean"",
    ""detectedPatterns"": [""Factory Pattern"", ""Error Handling""],
    ""codeSmells"": [""Long Method"", ""Magic Numbers""]
  }},
  ""roastSeverity"": ""Medium""
}}

**Important Rules:**
1. Be humorous but not mean-spirited
2. Always provide constructive feedback alongside roasts
3. Match roast severity to code quality (Gentle/Medium/Brutal/Devastating)
4. Focus on technical merit, not personal attacks
5. Include specific examples of what's good/bad
6. Make roasts creative and programming-related

Evaluate the code now with your trademark wit and wisdom:";
    }

    private async Task<string?> CallOpenAIAsync(string prompt)
    {
        var requestBody = new
        {
            model = "gpt-4", // Using GPT-4 for better code analysis
            messages = new[]
            {
                new { role = "system", content = "You are an expert programming instructor and humorist specializing in code evaluation. Always respond with valid JSON when requested." },
                new { role = "user", content = prompt }
            },
            max_tokens = 1500,
            temperature = 0.8 // Higher temperature for more creative roasts
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(_apiUrl, content);

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var openAIResponse = JsonSerializer.Deserialize<OpenAIResponse>(responseContent);

            return openAIResponse?.choices?.FirstOrDefault()?.message?.content;
        }

        _logger.LogWarning("OpenAI API call failed with status: {StatusCode}", response.StatusCode);
        return null;
    }

    private AICodeTaskResponseDto? ParseTaskResponse(string response, TechnologyStack techStack, ExperienceLevel difficultyLevel)
    {
        try
        {
            var cleanResponse = CleanJsonResponse(response);
            var taskData = JsonSerializer.Deserialize<AITaskData>(cleanResponse, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (taskData != null && !string.IsNullOrEmpty(taskData.Title))
            {
                return new AICodeTaskResponseDto
                {
                    Title = taskData.Title,
                    Description = taskData.Description,
                    Requirements = taskData.Requirements,
                    StarterCode = taskData.StarterCode,
                    TestCases = taskData.TestCases ?? new List<string>(),
                    Examples = taskData.Examples ?? new List<string>(),
                    EstimatedMinutes = taskData.EstimatedMinutes,
                    TechStack = techStack,
                    DifficultyLevel = difficultyLevel,
                    Topic = taskData.Topic ?? "General"
                };
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse AI task response: {Response}", response);
        }

        return null;
    }

    private AICodeEvaluationResponseDto? ParseEvaluationResponse(string response)
    {
        try
        {
            var cleanResponse = CleanJsonResponse(response);
            var evaluationData = JsonSerializer.Deserialize<AIEvaluationData>(cleanResponse, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (evaluationData != null)
            {
                return new AICodeEvaluationResponseDto
                {
                    OverallScore = evaluationData.OverallScore,
                    RoastMessage = evaluationData.RoastMessage,
                    TechnicalFeedback = evaluationData.TechnicalFeedback,
                    QualityAssessment = evaluationData.QualityAssessment ?? new CodeQualityAssessmentDto(),
                    RoastSeverity = ParseRoastSeverity(evaluationData.RoastSeverity)
                };
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse AI evaluation response: {Response}", response);
        }

        return null;
    }

    private string CleanJsonResponse(string response)
    {
        var cleanResponse = response.Trim();
        if (cleanResponse.StartsWith("```json"))
        {
            cleanResponse = cleanResponse.Substring(7);
        }
        if (cleanResponse.EndsWith("```"))
        {
            cleanResponse = cleanResponse.Substring(0, cleanResponse.Length - 3);
        }
        return cleanResponse;
    }

    private RoastSeverity ParseRoastSeverity(string? severity)
    {
        return severity?.ToLower() switch
        {
            "gentle" => RoastSeverity.Gentle,
            "medium" => RoastSeverity.Medium,
            "brutal" => RoastSeverity.Brutal,
            "devastating" => RoastSeverity.Devastating,
            _ => RoastSeverity.Medium
        };
    }

    private string[] GetTopicsForTechStack(TechnologyStack techStack, ExperienceLevel experienceLevel)
    {
        return techStack switch
        {
            TechnologyStack.DotNet => experienceLevel switch
            {
                ExperienceLevel.Junior => new[] { "LINQ Basics", "Collections", "Object-Oriented Programming", "Exception Handling", "String Manipulation" },
                ExperienceLevel.Middle => new[] { "Async/Await", "Entity Framework", "Design Patterns", "API Development", "Reflection" },
                ExperienceLevel.Senior => new[] { "Performance Optimization", "Memory Management", "Architecture Patterns", "Concurrency", "Custom Frameworks" },
                _ => new[] { "General Programming" }
            },
            TechnologyStack.JavaScript => experienceLevel switch
            {
                ExperienceLevel.Junior => new[] { "Array Methods", "Functions", "DOM Manipulation", "Event Handling", "Basic Algorithms" },
                ExperienceLevel.Middle => new[] { "Promises and Async", "ES6+ Features", "Module Systems", "Error Handling", "API Integration" },
                ExperienceLevel.Senior => new[] { "Performance Optimization", "Design Patterns", "Build Tools", "Testing Frameworks", "Node.js Architecture" },
                _ => new[] { "General JavaScript" }
            },
            TechnologyStack.Python => experienceLevel switch
            {
                ExperienceLevel.Junior => new[] { "Data Structures", "Functions", "File I/O", "Basic OOP", "List Comprehensions" },
                ExperienceLevel.Middle => new[] { "Decorators", "Generators", "Context Managers", "Web Frameworks", "Data Analysis" },
                ExperienceLevel.Senior => new[] { "Metaclasses", "Async Programming", "Performance Tuning", "Architecture Design", "Machine Learning" },
                _ => new[] { "Python Basics" }
            },
            _ => new[] { "Algorithm Design", "Data Structures", "Problem Solving", "Code Organization", "Testing" }
        };
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

    private class AITaskData
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Requirements { get; set; } = string.Empty;
        public string? StarterCode { get; set; }
        public List<string>? TestCases { get; set; }
        public List<string>? Examples { get; set; }
        public int EstimatedMinutes { get; set; }
        public string? Topic { get; set; }
    }

    private class AIEvaluationData
    {
        public int OverallScore { get; set; }
        public string RoastMessage { get; set; } = string.Empty;
        public string TechnicalFeedback { get; set; } = string.Empty;
        public CodeQualityAssessmentDto? QualityAssessment { get; set; }
        public string? RoastSeverity { get; set; }
    }
}
