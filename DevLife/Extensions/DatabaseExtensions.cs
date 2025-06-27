using DevLife.Database;
using DevLife.Enums;
using DevLife.Models.CodeCasino;
using DevLife.Models.MeetingExcuse;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace DevLife.Extensions;

public static class DatabaseExtensions
{
    public static async Task SeedDatabaseAsync(this ApplicationDbContext context)
    {
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Seed challenges if none exist
        if (!await context.CodeChallenges.AnyAsync())
        {
            await SeedChallengesAsync(context);
        }
    }

    private static async Task SeedChallengesAsync(ApplicationDbContext context)
    {
        var challenges = new List<CodeChallenge>
            {
                // C# / .NET Challenges
                new CodeChallenge
                {
                    Title = "Null Reference Check",
                    Description = "Which code properly handles null reference checking?",
                    TechStack = TechnologyStack.DotNet,
                    DifficultyLevel = ExperienceLevel.Junior,
                    CorrectCode = @"public string GetUserName(User user)
{
    return user?.Name ?? ""Unknown"";
}",
                    BuggyCode = @"public string GetUserName(User user)
{
    return user.Name ?? ""Unknown"";
}",
                    Explanation = "The first code uses null-conditional operator (?.) to safely access Name property, preventing NullReferenceException.",
                    IsActive = true
                },

                new CodeChallenge
                {
                    Title = "List Initialization",
                    Description = "Which is the correct way to initialize a list in C#?",
                    TechStack = TechnologyStack.DotNet,
                    DifficultyLevel = ExperienceLevel.Junior,
                    CorrectCode = @"var numbers = new List<int> { 1, 2, 3, 4, 5 };",
                    BuggyCode = @"var numbers = new List<int> [ 1, 2, 3, 4, 5 ];",
                    Explanation = "C# uses curly braces {} for collection initialization, not square brackets [].",
                    IsActive = true
                },

                // JavaScript Challenges
                new CodeChallenge
                {
                    Title = "Array Method Chain",
                    Description = "Which code correctly filters and maps an array?",
                    TechStack = TechnologyStack.JavaScript,
                    DifficultyLevel = ExperienceLevel.Junior,
                    CorrectCode = @"const result = numbers
    .filter(x => x > 0)
    .map(x => x * 2);",
                    BuggyCode = @"const result = numbers
    .filter(x => x > 0)
    .map(x => x * 2)
    .toArray();",
                    Explanation = "JavaScript arrays don't have a toArray() method. The filter and map methods already return arrays.",
                    IsActive = true
                },

                // Python Challenges
                new CodeChallenge
                {
                    Title = "Dictionary Iteration",
                    Description = "Which is the correct way to iterate over dictionary items in Python?",
                    TechStack = TechnologyStack.Python,
                    DifficultyLevel = ExperienceLevel.Junior,
                    CorrectCode = @"for key, value in my_dict.items():
    print(f""{key}: {value}"")",
                    BuggyCode = @"for key, value in my_dict:
    print(f""{key}: {value}"")",
                    Explanation = "You need to call .items() method to iterate over key-value pairs in a Python dictionary.",
                    IsActive = true
                },

                // React Challenges
                new CodeChallenge
                {
                    Title = "useState Hook",
                    Description = "Which is the correct way to use useState hook?",
                    TechStack = TechnologyStack.React,
                    DifficultyLevel = ExperienceLevel.Middle,
                    CorrectCode = @"const [count, setCount] = useState(0);
const increment = () => setCount(count + 1);",
                    BuggyCode = @"const [count, setCount] = useState(0);
const increment = () => count++;",
                    Explanation = "In React, you must use the setter function (setCount) to update state, not direct assignment.",
                    IsActive = true
                },

                // Java Challenges
                new CodeChallenge
                {
                    Title = "String Comparison",
                    Description = "Which is the correct way to compare strings in Java?",
                    TechStack = TechnologyStack.Java,
                    DifficultyLevel = ExperienceLevel.Junior,
                    CorrectCode = @"if (str1.equals(str2)) {
    System.out.println(""Equal"");
}",
                    BuggyCode = @"if (str1 == str2) {
    System.out.println(""Equal"");
}",
                    Explanation = "In Java, use .equals() method to compare string content, not == operator which compares references.",
                    IsActive = true
                },

                // TypeScript Challenge
                new CodeChallenge
                {
                    Title = "Interface Implementation",
                    Description = "Which TypeScript interface implementation is correct?",
                    TechStack = TechnologyStack.TypeScript,
                    DifficultyLevel = ExperienceLevel.Middle,
                    CorrectCode = @"interface User {
    name: string;
    age: number;
}

const user: User = {
    name: ""John"",
    age: 30
};",
                    BuggyCode = @"interface User {
    name: string;
    age: number;
}

const user: User = {
    name: ""John"",
    age: ""30""
};",
                    Explanation = "The age property should be a number, not a string. TypeScript enforces type safety.",
                    IsActive = true
                },

                // Angular Challenge
                new CodeChallenge
                {
                    Title = "Component Property Binding",
                    Description = "Which is the correct way to bind a property in Angular?",
                    TechStack = TechnologyStack.Angular,
                    DifficultyLevel = ExperienceLevel.Middle,
                    CorrectCode = @"<img [src]=""imageUrl"" alt=""Photo"">",
                    BuggyCode = @"<img src=""{{imageUrl}}"" alt=""Photo"">",
                    Explanation = "Use property binding [src] for dynamic values, not interpolation {{}} inside attributes.",
                    IsActive = true
                },

                // Vue Challenge
                new CodeChallenge
                {
                    Title = "Vue Data Binding",
                    Description = "Which is the correct way to bind data in Vue.js?",
                    TechStack = TechnologyStack.Vue,
                    DifficultyLevel = ExperienceLevel.Junior,
                    CorrectCode = @"<p>{{ message }}</p>",
                    BuggyCode = @"<p>{message}</p>",
                    Explanation = "Vue.js uses double curly braces {{ }} for text interpolation, not single braces.",
                    IsActive = true
                },

                // PHP Challenge
                new CodeChallenge
                {
                    Title = "Array Declaration",
                    Description = "Which is the correct way to declare an array in PHP?",
                    TechStack = TechnologyStack.PHP,
                    DifficultyLevel = ExperienceLevel.Junior,
                    CorrectCode = @"$fruits = array(""apple"", ""banana"", ""orange"");",
                    BuggyCode = @"$fruits = [""apple"", ""banana"", ""orange""];",
                    Explanation = "Both are actually correct in modern PHP, but array() is the traditional syntax.",
                    IsActive = true
                }
            };

        context.CodeChallenges.AddRange(challenges);
        await context.SaveChangesAsync();

        Console.WriteLine($"Seeded {challenges.Count} code challenges into the database.");
    }
    public static async Task SeedMeetingExcusesAsync(this ApplicationDbContext context)
    {
        if (await context.MeetingExcuses.AnyAsync())
        {
            return; // Already seeded
        }

        var excuses = new List<MeetingExcuse>
    {
        // DAILY STANDUP EXCUSES
        // Technical
        new MeetingExcuse
        {
            ExcuseText = "My IDE crashed and took my entire sprint backlog with it. Still recovering from the trauma.",
            Category = MeetingCategory.DailyStandup,
            Type = ExcuseType.Technical,
            BelievabilityScore = 8,
            TagsJson = JsonSerializer.Serialize(new[] { "IDE", "crash", "backlog", "trauma" }),
            IsActive = true
        },
        new MeetingExcuse
        {
            ExcuseText = "I discovered a race condition in my coffee machine - can't function until it's patched.",
            Category = MeetingCategory.DailyStandup,
            Type = ExcuseType.Technical,
            BelievabilityScore = 6,
            TagsJson = JsonSerializer.Serialize(new[] { "race condition", "coffee", "debugging", "dependency" }),
            IsActive = true
        },
        
        // Personal
        new MeetingExcuse
        {
            ExcuseText = "My cat deployed to production again. Currently doing damage control.",
            Category = MeetingCategory.DailyStandup,
            Type = ExcuseType.Personal,
            BelievabilityScore = 7,
            TagsJson = JsonSerializer.Serialize(new[] { "cat", "production", "deployment", "pets" }),
            IsActive = true
        },
        new MeetingExcuse
        {
            ExcuseText = "I'm stuck in a recursive family dinner loop and can't find the exit condition.",
            Category = MeetingCategory.DailyStandup,
            Type = ExcuseType.Personal,
            BelievabilityScore = 5,
            TagsJson = JsonSerializer.Serialize(new[] { "recursive", "family", "loop", "exit condition" }),
            IsActive = true
        },
        
        // Creative
        new MeetingExcuse
        {
            ExcuseText = "I achieved enlightenment through code review comments and transcended to a higher plane of existence.",
            Category = MeetingCategory.DailyStandup,
            Type = ExcuseType.Creative,
            BelievabilityScore = 2,
            TagsJson = JsonSerializer.Serialize(new[] { "enlightenment", "code review", "transcendence", "higher plane" }),
            IsActive = true
        },
        new MeetingExcuse
        {
            ExcuseText = "The rubber duck finally talked back - we're having a philosophical debate about async/await.",
            Category = MeetingCategory.DailyStandup,
            Type = ExcuseType.Creative,
            BelievabilityScore = 3,
            TagsJson = JsonSerializer.Serialize(new[] { "rubber duck", "philosophical", "async", "await", "debugging" }),
            IsActive = true
        },

        // SPRINT PLANNING EXCUSES
        // Technical
        new MeetingExcuse
        {
            ExcuseText = "Our estimation algorithm is experiencing integer overflow - all tasks now take infinity hours.",
            Category = MeetingCategory.SprintPlanning,
            Type = ExcuseType.Technical,
            BelievabilityScore = 7,
            TagsJson = JsonSerializer.Serialize(new[] { "estimation", "integer overflow", "infinity", "algorithm" }),
            IsActive = true
        },
        new MeetingExcuse
        {
            ExcuseText = "I'm currently debugging a temporal paradox in our sprint timeline.",
            Category = MeetingCategory.SprintPlanning,
            Type = ExcuseType.Technical,
            BelievabilityScore = 4,
            TagsJson = JsonSerializer.Serialize(new[] { "temporal paradox", "timeline", "debugging", "time travel" }),
            IsActive = true
        },

        // Personal
        new MeetingExcuse
        {
            ExcuseText = "My brain's cache is full and needs defragmentation before I can estimate anything.",
            Category = MeetingCategory.SprintPlanning,
            Type = ExcuseType.Personal,
            BelievabilityScore = 6,
            TagsJson = JsonSerializer.Serialize(new[] { "brain cache", "defragmentation", "estimation", "mental health" }),
            IsActive = true
        },
        new MeetingExcuse
        {
            ExcuseText = "I'm experiencing analysis paralysis due to too many story points in parallel.",
            Category = MeetingCategory.SprintPlanning,
            Type = ExcuseType.Personal,
            BelievabilityScore = 8,
            TagsJson = JsonSerializer.Serialize(new[] { "analysis paralysis", "story points", "parallel processing" }),
            IsActive = true
        },

        // Creative
        new MeetingExcuse
        {
            ExcuseText = "I've entered a zen state where all tasks are simultaneously done and not done (Schrödinger's Sprint).",
            Category = MeetingCategory.SprintPlanning,
            Type = ExcuseType.Creative,
            BelievabilityScore = 3,
            TagsJson = JsonSerializer.Serialize(new[] { "zen", "quantum", "Schrödinger", "superposition" }),
            IsActive = true
        },

        // CLIENT MEETING EXCUSES
        // Technical
        new MeetingExcuse
        {
            ExcuseText = "The client's requirements caused a stack overflow in my understanding buffer.",
            Category = MeetingCategory.ClientMeeting,
            Type = ExcuseType.Technical,
            BelievabilityScore = 7,
            TagsJson = JsonSerializer.Serialize(new[] { "requirements", "stack overflow", "buffer", "understanding" }),
            IsActive = true
        },
        new MeetingExcuse
        {
            ExcuseText = "I'm experiencing a merge conflict with the client's expectations and reality.",
            Category = MeetingCategory.ClientMeeting,
            Type = ExcuseType.Technical,
            BelievabilityScore = 9,
            TagsJson = JsonSerializer.Serialize(new[] { "merge conflict", "expectations", "reality", "git" }),
            IsActive = true
        },

        // Personal
        new MeetingExcuse
        {
            ExcuseText = "My social interaction API is down for maintenance.",
            Category = MeetingCategory.ClientMeeting,
            Type = ExcuseType.Personal,
            BelievabilityScore = 6,
            TagsJson = JsonSerializer.Serialize(new[] { "social API", "maintenance", "interaction", "downtime" }),
            IsActive = true
        },
        new MeetingExcuse
        {
            ExcuseText = "I'm allergic to business jargon and might break out in code comments.",
            Category = MeetingCategory.ClientMeeting,
            Type = ExcuseType.Personal,
            BelievabilityScore = 5,
            TagsJson = JsonSerializer.Serialize(new[] { "allergic", "business jargon", "code comments", "reaction" }),
            IsActive = true
        },

        // Health
        new MeetingExcuse
        {
            ExcuseText = "I have chronic impostor syndrome flare-up whenever clients mention 'innovative solutions'.",
            Category = MeetingCategory.ClientMeeting,
            Type = ExcuseType.Health,
            BelievabilityScore = 8,
            TagsJson = JsonSerializer.Serialize(new[] { "impostor syndrome", "innovative", "solutions", "chronic" }),
            IsActive = true
        },

        // TEAM BUILDING EXCUSES
        // Personal
        new MeetingExcuse
        {
            ExcuseText = "I'm an introvert - team building is like running a memory-intensive process on my social CPU.",
            Category = MeetingCategory.TeamBuilding,
            Type = ExcuseType.Personal,
            BelievabilityScore = 9,
            TagsJson = JsonSerializer.Serialize(new[] { "introvert", "memory intensive", "social CPU", "processing" }),
            IsActive = true
        },
        new MeetingExcuse
        {
            ExcuseText = "My personality.exe has stopped responding to team building activities.",
            Category = MeetingCategory.TeamBuilding,
            Type = ExcuseType.Personal,
            BelievabilityScore = 7,
            TagsJson = JsonSerializer.Serialize(new[] { "personality.exe", "stopped responding", "team building" }),
            IsActive = true
        },

        // Creative  
        new MeetingExcuse
        {
            ExcuseText = "I've achieved singleton pattern in real life - there can only be one of me at team events.",
            Category = MeetingCategory.TeamBuilding,
            Type = ExcuseType.Creative,
            BelievabilityScore = 4,
            TagsJson = JsonSerializer.Serialize(new[] { "singleton pattern", "design pattern", "unique instance" }),
            IsActive = true
        },
        new MeetingExcuse
        {
            ExcuseText = "Team building violates my SOLID principles - specifically the Single Responsibility Principle.",
            Category = MeetingCategory.TeamBuilding,
            Type = ExcuseType.Creative,
            BelievabilityScore = 6,
            TagsJson = JsonSerializer.Serialize(new[] { "SOLID principles", "Single Responsibility", "architecture" }),
            IsActive = true
        },

        // Emergency
        new MeetingExcuse
        {
            ExcuseText = "Critical bug in my work-life balance module - requires immediate hotfix.",
            Category = MeetingCategory.TeamBuilding,
            Type = ExcuseType.Emergency,
            BelievabilityScore = 8,
            TagsJson = JsonSerializer.Serialize(new[] { "critical bug", "work-life balance", "hotfix", "emergency" }),
            IsActive = true
        },

        // CODE REVIEW EXCUSES
        // Technical
        new MeetingExcuse
        {
            ExcuseText = "My code review neural network needs more training data before it can process this PR.",
            Category = MeetingCategory.CodeReview,
            Type = ExcuseType.Technical,
            BelievabilityScore = 6,
            TagsJson = JsonSerializer.Serialize(new[] { "neural network", "training data", "PR", "machine learning" }),
            IsActive = true
        },
        new MeetingExcuse
        {
            ExcuseText = "I'm experiencing PTSD from the last code review - Post Traumatic Syntax Disorder.",
            Category = MeetingCategory.CodeReview,
            Type = ExcuseType.Health,
            BelievabilityScore = 7,
            TagsJson = JsonSerializer.Serialize(new[] { "PTSD", "syntax disorder", "trauma", "code review" }),
            IsActive = true
        },

        // Personal
        new MeetingExcuse
        {
            ExcuseText = "My constructive criticism module is experiencing a buffer overflow.",
            Category = MeetingCategory.CodeReview,
            Type = ExcuseType.Personal,
            BelievabilityScore = 5,
            TagsJson = JsonSerializer.Serialize(new[] { "constructive criticism", "buffer overflow", "module" }),
            IsActive = true
        },

        // RETROSPECTIVE EXCUSES
        // Personal
        new MeetingExcuse
        {
            ExcuseText = "I'm stuck in an infinite loop of self-reflection and can't break out.",
            Category = MeetingCategory.Retrospective,
            Type = ExcuseType.Personal,
            BelievabilityScore = 6,
            TagsJson = JsonSerializer.Serialize(new[] { "infinite loop", "self-reflection", "break statement" }),
            IsActive = true
        },
        new MeetingExcuse
        {
            ExcuseText = "My retrospective thoughts are experiencing a race condition with my current productivity.",
            Category = MeetingCategory.Retrospective,
            Type = ExcuseType.Technical,
            BelievabilityScore = 7,
            TagsJson = JsonSerializer.Serialize(new[] { "race condition", "retrospective", "productivity", "concurrency" }),
            IsActive = true
        },

        // Creative
        new MeetingExcuse
        {
            ExcuseText = "I've transcended linear time and am experiencing all sprints simultaneously.",
            Category = MeetingCategory.Retrospective,
            Type = ExcuseType.Creative,
            BelievabilityScore = 2,
            TagsJson = JsonSerializer.Serialize(new[] { "transcended", "linear time", "simultaneous", "sprints" }),
            IsActive = true
        },

        // MYSTERIOUS/GENERAL EXCUSES
        new MeetingExcuse
        {
            ExcuseText = "I'm currently running in background mode to preserve system resources.",
            Category = MeetingCategory.DailyStandup,
            Type = ExcuseType.Mysterious,
            BelievabilityScore = 5,
            TagsJson = JsonSerializer.Serialize(new[] { "background mode", "system resources", "performance" }),
            IsActive = true
        },
        new MeetingExcuse
        {
            ExcuseText = "My attendance feature is deprecated and will be removed in the next major version.",
            Category = MeetingCategory.TeamBuilding,
            Type = ExcuseType.Mysterious,
            BelievabilityScore = 4,
            TagsJson = JsonSerializer.Serialize(new[] { "deprecated", "attendance", "major version", "breaking change" }),
            IsActive = true
        },
        new MeetingExcuse
        {
            ExcuseText = "I'm experiencing quantum entanglement with my code - if I move, the bugs will know.",
            Category = MeetingCategory.DailyStandup,
            Type = ExcuseType.Mysterious,
            BelievabilityScore = 3,
            TagsJson = JsonSerializer.Serialize(new[] { "quantum entanglement", "bugs", "observer effect" }),
            IsActive = true
        },

        // HEALTH EXCUSES
        new MeetingExcuse
        {
            ExcuseText = "I have acute carpal tunnel from too much ctrl+c, ctrl+v - doctor says no more meetings until I learn original thinking.",
            Category = MeetingCategory.SprintPlanning,
            Type = ExcuseType.Health,
            BelievabilityScore = 8,
            TagsJson = JsonSerializer.Serialize(new[] { "carpal tunnel", "ctrl+c", "ctrl+v", "original thinking" }),
            IsActive = true
        },
        new MeetingExcuse
        {
            ExcuseText = "I have chronic stack overflow syndrome - my brain keeps running out of memory during meetings.",
            Category = MeetingCategory.ClientMeeting,
            Type = ExcuseType.Health,
            BelievabilityScore = 7,
            TagsJson = JsonSerializer.Serialize(new[] { "chronic", "stack overflow", "brain", "memory", "meetings" }),
            IsActive = true
        },

        // EMERGENCY EXCUSES
        new MeetingExcuse
        {
            ExcuseText = "Production server is on fire (literally this time, not metaphorically).",
            Category = MeetingCategory.SprintPlanning,
            Type = ExcuseType.Emergency,
            BelievabilityScore = 9,
            TagsJson = JsonSerializer.Serialize(new[] { "production", "literally on fire", "emergency", "server" }),
            IsActive = true
        },
        new MeetingExcuse
        {
            ExcuseText = "Emergency database corruption - all my meeting availability got deleted.",
            Category = MeetingCategory.ClientMeeting,
            Type = ExcuseType.Emergency,
            BelievabilityScore = 8,
            TagsJson = JsonSerializer.Serialize(new[] { "database corruption", "availability", "emergency" }),
            IsActive = true
        },

        // MORE CREATIVE ONES
        new MeetingExcuse
        {
            ExcuseText = "I'm beta testing a new feature called 'selective attendance' - still working out the bugs.",
            Category = MeetingCategory.TeamBuilding,
            Type = ExcuseType.Creative,
            BelievabilityScore = 5,
            TagsJson = JsonSerializer.Serialize(new[] { "beta testing", "selective attendance", "bugs", "feature" }),
            IsActive = true
        },
        new MeetingExcuse
        {
            ExcuseText = "My calendar.exe is experiencing a memory leak - it keeps forgetting about meetings.",
            Category = MeetingCategory.Planning,
            Type = ExcuseType.Technical,
            BelievabilityScore = 7,
            TagsJson = JsonSerializer.Serialize(new[] { "calendar.exe", "memory leak", "forgetting meetings" }),
            IsActive = true
        },
        new MeetingExcuse
        {
            ExcuseText = "I'm currently implementing a new design pattern called 'Meeting Avoidance Singleton'.",
            Category = MeetingCategory.Planning,
            Type = ExcuseType.Creative,
            BelievabilityScore = 4,
            TagsJson = JsonSerializer.Serialize(new[] { "design pattern", "Meeting Avoidance", "Singleton" }),
            IsActive = true
        }
    };

        context.MeetingExcuses.AddRange(excuses);
        await context.SaveChangesAsync();

        Console.WriteLine($"Seeded {excuses.Count} meeting excuses successfully! 🎭");
    }
}
