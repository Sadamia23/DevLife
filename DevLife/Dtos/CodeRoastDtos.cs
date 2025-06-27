using DevLife.Enums;
using System.ComponentModel.DataAnnotations;

namespace DevLife.Dtos;

public class CodeRoastTaskDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Requirements { get; set; } = string.Empty;
    public TechnologyStack TechStack { get; set; }
    public ExperienceLevel DifficultyLevel { get; set; }
    public string? StarterCode { get; set; }
    public List<string> TestCases { get; set; } = new();
    public List<string> Examples { get; set; } = new();
    public int EstimatedMinutes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CodeRoastSubmissionDto
{
    [Required]
    public int TaskId { get; set; }

    [Required]
    [StringLength(10000, MinimumLength = 10)]
    public string Code { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public int TimeSpentMinutes { get; set; } = 0;
}

public class CodeRoastResultDto
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public string TaskTitle { get; set; } = string.Empty;
    public string SubmittedCode { get; set; } = string.Empty;
    public string? UserNotes { get; set; }

    // AI Evaluation Results
    public int OverallScore { get; set; } // 0-100
    public string RoastMessage { get; set; } = string.Empty;
    public string TechnicalFeedback { get; set; } = string.Empty;
    public CodeQualityAssessmentDto QualityAssessment { get; set; } = new();

    // Metadata
    public DateTime SubmittedAt { get; set; }
    public int TimeSpentMinutes { get; set; }
    public bool IsRoasted { get; set; } // true if score < 70
    public bool IsPraised { get; set; } // true if score >= 70
    public RoastSeverity RoastSeverity { get; set; }
}

public class CodeQualityAssessmentDto
{
    public int ReadabilityScore { get; set; } // 0-100
    public int PerformanceScore { get; set; } // 0-100
    public int CorrectnessScore { get; set; } // 0-100
    public int BestPracticesScore { get; set; } // 0-100

    public List<string> PositivePoints { get; set; } = new();
    public List<string> ImprovementPoints { get; set; } = new();
    public List<string> RedFlags { get; set; } = new();

    public string CodeStyle { get; set; } = string.Empty; // "Clean", "Messy", "Chaotic", etc.
    public List<string> DetectedPatterns { get; set; } = new(); // "Singleton", "Factory", etc.
    public List<string> CodeSmells { get; set; } = new(); // "Long Method", "Code Duplication", etc.
}

public class CodeRoastStatsDto
{
    public int TotalSubmissions { get; set; }
    public int TotalRoasts { get; set; }
    public int TotalPraises { get; set; }
    public double AverageScore { get; set; }
    public int HighestScore { get; set; }
    public int LowestScore { get; set; }

    // Streaks
    public int CurrentStreak { get; set; } // Current consecutive good scores (70+)
    public int LongestStreak { get; set; }
    public int CurrentRoastStreak { get; set; } // Current consecutive bad scores (<70)
    public int LongestRoastStreak { get; set; }

    // Time statistics
    public int TotalTimeSpentMinutes { get; set; }
    public double AverageTimePerTask { get; set; }

    // Difficulty breakdown
    public int JuniorTasksCompleted { get; set; }
    public int MiddleTasksCompleted { get; set; }
    public int SeniorTasksCompleted { get; set; }

    // Quality metrics
    public double AverageReadabilityScore { get; set; }
    public double AveragePerformanceScore { get; set; }
    public double AverageCorrectnessScore { get; set; }
    public double AverageBestPracticesScore { get; set; }

    // Recent performance
    public DateTime? LastSubmission { get; set; }
    public List<int> RecentScores { get; set; } = new(); // Last 10 scores

    // Achievements
    public List<string> UnlockedAchievements { get; set; } = new();
    public int PerfectScores { get; set; } // Number of 100-point submissions
}

public class CodeRoastDashboardDto
{
    public CodeRoastStatsDto UserStats { get; set; } = new();
    public List<CodeRoastResultDto> RecentRoasts { get; set; } = new();
    public List<CodeRoastTaskDto> RecommendedTasks { get; set; } = new();
    public CodeRoastHallOfFameDto HallOfFame { get; set; } = new();
}

public class CodeRoastHallOfFameDto
{
    public List<HallOfFameEntryDto> BestScores { get; set; } = new();
    public List<HallOfFameEntryDto> WorstScores { get; set; } = new();
    public List<HallOfFameEntryDto> FunniestRoasts { get; set; } = new();
    public List<HallOfFameEntryDto> MostImprovedUsers { get; set; } = new();
}

public class HallOfFameEntryDto
{
    public string Username { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public TechnologyStack TechStack { get; set; }
    public ZodiacSign ZodiacSign { get; set; }
    public int Score { get; set; }
    public string TaskTitle { get; set; } = string.Empty;
    public string RoastMessage { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public RoastSeverity RoastSeverity { get; set; }
}

// AI Challenge Generation DTO
public class AICodeTaskRequestDto
{
    public TechnologyStack TechStack { get; set; }
    public ExperienceLevel DifficultyLevel { get; set; }
    public string? SpecificTopic { get; set; }
    public List<string>? FocusAreas { get; set; }
}

public class AICodeTaskResponseDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Requirements { get; set; } = string.Empty;
    public string? StarterCode { get; set; }
    public List<string> TestCases { get; set; } = new();
    public List<string> Examples { get; set; } = new();
    public int EstimatedMinutes { get; set; }
    public TechnologyStack TechStack { get; set; }
    public ExperienceLevel DifficultyLevel { get; set; }
    public string Topic { get; set; } = string.Empty;
}

// AI Code Evaluation DTO
public class AICodeEvaluationRequestDto
{
    public string Code { get; set; } = string.Empty;
    public string TaskDescription { get; set; } = string.Empty;
    public TechnologyStack TechStack { get; set; }
    public ExperienceLevel DifficultyLevel { get; set; }
}

public class AICodeEvaluationResponseDto
{
    public int OverallScore { get; set; }
    public string RoastMessage { get; set; } = string.Empty;
    public string TechnicalFeedback { get; set; } = string.Empty;
    public CodeQualityAssessmentDto QualityAssessment { get; set; } = new();
    public RoastSeverity RoastSeverity { get; set; }
}
