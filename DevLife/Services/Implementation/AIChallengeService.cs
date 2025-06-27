using DevLife.Dtos;
using DevLife.Enums;
using DevLife.Services.Interfaces;
using System.Text;
using System.Text.Json;

namespace DevLife.Services.Implementation;

public class OpenAIChallengeService : IAIChallengeService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _apiUrl = "https://api.openai.com/v1/chat/completions";
    private readonly ILogger<OpenAIChallengeService> _logger;
    private readonly ICodeFormatterService _codeFormatterService; // Add formatter service

    public OpenAIChallengeService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<OpenAIChallengeService> logger,
        ICodeFormatterService codeFormatterService) // Inject formatter service
    {
        _httpClient = httpClient;
        _apiKey = configuration["OpenAI:ApiKey"] ?? throw new ArgumentNullException("OpenAI:ApiKey not configured");
        _logger = logger;
        _codeFormatterService = codeFormatterService; // Assign formatter service

        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }

    public async Task<AIChallengeResponseDto?> GenerateChallengeAsync(
        TechnologyStack techStack,
        ExperienceLevel experienceLevel,
        string? specificTopic = null)
    {
        try
        {
            var prompt = BuildEnhancedChallengePrompt(techStack, experienceLevel, specificTopic);
            var response = await CallOpenAIAsync(prompt);

            if (response != null)
            {
                return ParseChallengeResponse(response, techStack, experienceLevel);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating AI challenge for {TechStack} {ExperienceLevel}", techStack, experienceLevel);
            return null;
        }
    }

    public async Task<AIChallengeResponseDto?> GenerateDailyChallengeAsync()
    {
        try
        {
            var prompt = BuildEnhancedDailyChallengePrompt();
            var response = await CallOpenAIAsync(prompt);

            if (response != null)
            {
                return ParseChallengeResponse(response, TechnologyStack.DotNet, ExperienceLevel.Middle);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating daily AI challenge");
            return null;
        }
    }

    public async Task<bool> ValidateCodeAsync(string code, TechnologyStack techStack)
    {
        try
        {
            var prompt = BuildValidationPrompt(code, techStack);
            var response = await CallOpenAIAsync(prompt);

            return response?.Contains("VALID", StringComparison.OrdinalIgnoreCase) == true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating code with AI");
            return false;
        }
    }

    private string BuildEnhancedChallengePrompt(TechnologyStack techStack, ExperienceLevel experienceLevel, string? specificTopic)
    {
        var topics = GetTopicsForTechStack(techStack, experienceLevel);
        var selectedTopic = specificTopic ?? topics[Random.Shared.Next(topics.Length)];
        var formattingGuidelines = GetFormattingGuidelines(techStack);

        return $@"
You are an expert coding challenge generator for a developer casino game. Create a high-quality code challenge that tests real programming skills.

**Challenge Specifications:**
- Technology Stack: {techStack}
- Experience Level: {experienceLevel}
- Topic: {selectedTopic}

**Code Quality Requirements:**
{formattingGuidelines}

**Challenge Requirements:**
1. Create a realistic coding scenario that a {experienceLevel.ToString().ToLower()} {techStack} developer encounters daily
2. Generate TWO code snippets:
   - One CORRECT implementation that works perfectly
   - One with a SUBTLE BUG that's realistic and educational
3. The bug should be something developers actually struggle with at the {experienceLevel} level
4. Both snippets should be well-formatted, readable, and look professionally written
5. Keep code concise but complete (8-20 lines per snippet)
6. Include proper variable names and comments where appropriate

**Bug Categories to Consider:**
- Off-by-one errors
- Null reference issues
- Race conditions
- Memory leaks
- Incorrect loop conditions
- Wrong data type usage
- Missing edge case handling
- Incorrect API usage

**Response Format (JSON):**
{{
  ""title"": ""Clear, descriptive challenge title"",
  ""description"": ""Brief explanation of what the code should accomplish and what to look for"",
  ""correctCode"": ""// Properly formatted working code here"",
  ""buggyCode"": ""// Properly formatted code with subtle bug here"",
  ""explanation"": ""Detailed explanation of the bug, why it occurs, and how to fix it. Include educational context."",
  ""topic"": ""{selectedTopic}""
}}

**Important Notes:**
- Ensure both code snippets are syntactically valid and properly indented
- The bug should be subtle enough to be challenging but clear enough to be educational
- Focus on patterns and mistakes that {experienceLevel} developers commonly make
- Make the explanation educational and actionable

Generate the challenge now:";
    }

    private string BuildEnhancedDailyChallengePrompt()
    {
        var allTechStacks = Enum.GetValues<TechnologyStack>();
        var randomTechStack = allTechStacks[Random.Shared.Next(allTechStacks.Length)];
        var formattingGuidelines = GetFormattingGuidelines(randomTechStack);

        return $@"
Generate a SPECIAL DAILY CHALLENGE for the Code Casino - make this extra engaging and educational!

**Today's Challenge Specifications:**
- Technology: {randomTechStack}
- Difficulty: Intermediate (accessible to multiple skill levels)
- Theme: Real-world production scenario
- Focus: Common critical mistakes in professional development

**Code Quality Requirements:**
{formattingGuidelines}

**Daily Challenge Special Requirements:**
1. Base the challenge on a REAL production scenario that developers face
2. The bug should represent a critical issue that could cause:
   - Performance problems
   - Security vulnerabilities  
   - Data corruption
   - User experience issues
   - System crashes

**High-Impact Bug Categories:**
- SQL injection vulnerabilities
- XSS vulnerabilities
- Race conditions in concurrent code
- Memory leaks in long-running processes
- Incorrect error handling
- Authentication/authorization flaws
- Performance bottlenecks
- Data validation failures

**Response Format (JSON):**
{{
  ""title"": ""Daily Challenge: [Compelling Title with Real-World Context]"",
  ""description"": ""Scenario: [Describe a real production situation] - Which implementation is secure/correct?"",
  ""correctCode"": ""// Production-ready, secure, properly formatted code"",
  ""buggyCode"": ""// Code with critical flaw, but still properly formatted"",
  ""explanation"": ""Why this matters in production: [Real-world impact] + Technical explanation + Prevention strategies"",
  ""topic"": ""Production-ready [specific topic]""
}}

**Make it Special:**
- Use realistic variable names and contexts
- Include comments that a professional developer would write
- Ensure the correct version follows industry best practices
- Make the explanation include real-world consequences and prevention tips

Create today's premium daily challenge:";
    }

    private string GetFormattingGuidelines(TechnologyStack techStack)
    {
        return techStack switch
        {
            TechnologyStack.DotNet => @"
- Use 4-space indentation
- Follow C# naming conventions (PascalCase for classes/methods, camelCase for variables)
- Place opening braces on new lines for methods/classes
- Include appropriate using statements
- Add XML documentation comments for public methods",

            TechnologyStack.JavaScript => @"
- Use 2-space indentation
- Follow JavaScript naming conventions (camelCase)
- Use const/let appropriately, avoid var
- Include JSDoc comments where helpful
- Use modern ES6+ syntax",

            TechnologyStack.TypeScript => @"
- Use 2-space indentation
- Follow TypeScript conventions with proper type annotations
- Use interface/type definitions appropriately
- Include proper import/export statements
- Add TSDoc comments for complex functions",

            TechnologyStack.Python => @"
- Use 4-space indentation (never tabs)
- Follow PEP 8 style guidelines
- Use snake_case for variables and functions
- Include docstrings for functions
- Add type hints where appropriate",

            TechnologyStack.Java => @"
- Use 4-space indentation
- Follow Java naming conventions (camelCase for variables/methods, PascalCase for classes)
- Place opening braces on same line
- Include proper package declarations
- Add Javadoc comments for public methods",

            TechnologyStack.React => @"
- Use 2-space indentation
- Follow React/JSX best practices
- Use functional components with hooks
- Include PropTypes or TypeScript interfaces
- Add JSDoc comments for component props",

            _ => @"
- Use consistent indentation (2 or 4 spaces)
- Follow language-standard naming conventions
- Include appropriate comments
- Ensure proper code structure and readability"
        };
    }

    private string BuildValidationPrompt(string code, TechnologyStack techStack)
    {
        return $@"
Validate if this {techStack} code is syntactically correct and would compile/run without syntax errors:

```{techStack.ToString().ToLower()}
{code}
```

Check for:
- Syntax errors
- Missing brackets/parentheses
- Incorrect language constructs
- Basic structural issues

Respond with only: VALID or INVALID
";
    }

    private async Task<string?> CallOpenAIAsync(string prompt)
    {
        var requestBody = new
        {
            model = "gpt-4o-mini", // Using the more cost-effective model
            messages = new[]
            {
                    new { role = "system", content = "You are an expert programming instructor and challenge creator with 15+ years of industry experience. You create realistic, educational coding challenges that help developers improve their skills. Always respond with valid, well-formatted JSON when requested." },
                    new { role = "user", content = prompt }
                },
            max_tokens = 1500, // Increased for better code formatting
            temperature = 0.7
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

    private AIChallengeResponseDto? ParseChallengeResponse(string response, TechnologyStack techStack, ExperienceLevel experienceLevel)
    {
        try
        {
            // Clean the response - sometimes AI adds markdown formatting
            var cleanResponse = response.Trim();
            if (cleanResponse.StartsWith("```json"))
            {
                cleanResponse = cleanResponse.Substring(7);
            }
            if (cleanResponse.EndsWith("```"))
            {
                cleanResponse = cleanResponse.Substring(0, cleanResponse.Length - 3);
            }

            var challengeData = JsonSerializer.Deserialize<AIChallengeData>(cleanResponse, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (challengeData != null && !string.IsNullOrEmpty(challengeData.Title))
            {
                // Format the code using the formatter service
                var formattedCorrectCode = _codeFormatterService.BeautifyCode(challengeData.CorrectCode, techStack);
                var formattedBuggyCode = _codeFormatterService.BeautifyCode(challengeData.BuggyCode, techStack);

                return new AIChallengeResponseDto
                {
                    Title = challengeData.Title,
                    Description = challengeData.Description,
                    CorrectCode = formattedCorrectCode,
                    BuggyCode = formattedBuggyCode,
                    Explanation = challengeData.Explanation,
                    Topic = challengeData.Topic ?? "General",
                    TechStack = techStack,
                    DifficultyLevel = experienceLevel
                };
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse AI challenge response: {Response}", response);
        }

        return null;
    }

    private string[] GetTopicsForTechStack(TechnologyStack techStack, ExperienceLevel experienceLevel)
    {
        return techStack switch
        {
            TechnologyStack.DotNet => experienceLevel switch
            {
                ExperienceLevel.Junior => new[] { "Variables and Types", "Null Handling", "Collections", "String Manipulation", "Basic LINQ" },
                ExperienceLevel.Middle => new[] { "Async/Await", "Exception Handling", "Generics", "Entity Framework", "Dependency Injection" },
                ExperienceLevel.Senior => new[] { "Memory Management", "Performance Optimization", "Design Patterns", "Concurrent Programming", "Advanced LINQ" },
                _ => new[] { "General Programming" }
            },
            TechnologyStack.JavaScript => experienceLevel switch
            {
                ExperienceLevel.Junior => new[] { "Variable Scope", "Array Methods", "Object Properties", "Function Basics", "Type Coercion" },
                ExperienceLevel.Middle => new[] { "Closures", "Promises", "Event Handling", "DOM Manipulation", "Error Handling" },
                ExperienceLevel.Senior => new[] { "Performance Optimization", "Memory Leaks", "Advanced Async", "Design Patterns", "Node.js Concepts" },
                _ => new[] { "General JavaScript" }
            },
            TechnologyStack.React => experienceLevel switch
            {
                ExperienceLevel.Junior => new[] { "Component State", "Props", "Event Handling", "Conditional Rendering", "Lists and Keys" },
                ExperienceLevel.Middle => new[] { "Hooks", "Context API", "Effect Dependencies", "Component Lifecycle", "State Management" },
                ExperienceLevel.Senior => new[] { "Performance Optimization", "Custom Hooks", "Advanced Patterns", "SSR/SSG", "Testing" },
                _ => new[] { "React Basics" }
            },
            TechnologyStack.Python => experienceLevel switch
            {
                ExperienceLevel.Junior => new[] { "Lists and Dictionaries", "String Operations", "File Handling", "Basic Functions", "Exception Handling" },
                ExperienceLevel.Middle => new[] { "Decorators", "Generators", "OOP Concepts", "Modules", "List Comprehensions" },
                ExperienceLevel.Senior => new[] { "Metaclasses", "Async Programming", "Performance Optimization", "Design Patterns", "Advanced OOP" },
                _ => new[] { "Python Basics" }
            },
            _ => new[] { "General Programming", "Logic Problems", "Algorithm Basics", "Data Structures" }
        };
    }

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

    private class AIChallengeData
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CorrectCode { get; set; } = string.Empty;
        public string BuggyCode { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
        public string? Topic { get; set; }
    }
}