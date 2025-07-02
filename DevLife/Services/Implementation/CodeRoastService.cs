using DevLife.Database;
using DevLife.Dtos;
using DevLife.Enums;
using DevLife.Models.CodeRoast;
using DevLife.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace DevLife.Services.Implementation;

public class CodeRoastService : ICodeRoastService
{
    private readonly ApplicationDbContext _context;
    private readonly IAICodeRoastService _aiService;
    private readonly ILogger<CodeRoastService> _logger;

    public CodeRoastService(
        ApplicationDbContext context,
        IAICodeRoastService aiService,
        ILogger<CodeRoastService> logger)
    {
        _context = context;
        _aiService = aiService;
        _logger = logger;
    }

    public async Task<CodeRoastDashboardDto?> GetDashboardAsync(string username)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null) return null;

        var userStats = await GetUserStatsAsync(username);
        if (userStats == null) return null;

        var recentRoasts = await GetRoastHistoryAsync(username, 5);
        var recommendedTasks = await GetRecommendedTasksAsync(username, 3);
        var hallOfFame = await GetHallOfFameAsync();

        return new CodeRoastDashboardDto
        {
            UserStats = userStats,
            RecentRoasts = recentRoasts,
            RecommendedTasks = recommendedTasks,
            HallOfFame = hallOfFame
        };
    }

    public async Task<CodeRoastTaskDto?> GetCodingTaskAsync(string username, ExperienceLevel difficulty)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null) return null;

        try
        {
            // First, try to generate a new AI task
            var aiTask = await _aiService.GenerateCodeTaskAsync(new AICodeTaskRequestDto
            {
                TechStack = user.TechStack,
                DifficultyLevel = difficulty,
                SpecificTopic = null
            });

            if (aiTask != null)
            {
                // Save the AI-generated task to database
                var task = new CodeRoastTask
                {
                    Title = aiTask.Title,
                    Description = aiTask.Description,
                    Requirements = aiTask.Requirements,
                    TechStack = aiTask.TechStack,
                    DifficultyLevel = aiTask.DifficultyLevel,
                    StarterCode = aiTask.StarterCode,
                    TestCasesJson = JsonSerializer.Serialize(aiTask.TestCases),
                    ExamplesJson = JsonSerializer.Serialize(aiTask.Examples),
                    EstimatedMinutes = aiTask.EstimatedMinutes,
                    Topic = aiTask.Topic,
                    IsAIGenerated = true
                };

                _context.CodeRoastTasks.Add(task);
                await _context.SaveChangesAsync();

                return MapTaskToDto(task);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to generate AI task, falling back to database tasks");
        }

        // Fallback to database tasks
        var dbTask = await _context.CodeRoastTasks
            .Where(t => t.IsActive && t.TechStack == user.TechStack && t.DifficultyLevel == difficulty)
            .OrderBy(t => Guid.NewGuid()) // Random selection
            .FirstOrDefaultAsync();

        return dbTask != null ? MapTaskToDto(dbTask) : null;
    }

    public async Task<CodeRoastResultDto> SubmitCodeAsync(string username, CodeRoastSubmissionDto submissionDto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null) throw new InvalidOperationException("User not found");

        var task = await _context.CodeRoastTasks.FindAsync(submissionDto.TaskId);
        if (task == null) throw new InvalidOperationException("Task not found");

        // Get AI evaluation
        var evaluation = await _aiService.EvaluateCodeAsync(new AICodeEvaluationRequestDto
        {
            Code = submissionDto.Code,
            TaskDescription = task.Description,
            TechStack = task.TechStack,
            DifficultyLevel = task.DifficultyLevel
        });

        if (evaluation == null)
        {
            throw new InvalidOperationException("Failed to evaluate code");
        }

        // Create submission record
        var submission = new CodeRoastSubmission
        {
            UserId = user.Id,
            TaskId = submissionDto.TaskId,
            SubmittedCode = submissionDto.Code,
            UserNotes = submissionDto.Notes,
            TimeSpentMinutes = submissionDto.TimeSpentMinutes,

            // AI evaluation results
            OverallScore = evaluation.OverallScore,
            RoastMessage = evaluation.RoastMessage,
            TechnicalFeedback = evaluation.TechnicalFeedback,
            RoastSeverity = evaluation.RoastSeverity,

            // Quality assessment
            ReadabilityScore = evaluation.QualityAssessment.ReadabilityScore,
            PerformanceScore = evaluation.QualityAssessment.PerformanceScore,
            CorrectnessScore = evaluation.QualityAssessment.CorrectnessScore,
            BestPracticesScore = evaluation.QualityAssessment.BestPracticesScore,

            PositivePointsJson = JsonSerializer.Serialize(evaluation.QualityAssessment.PositivePoints),
            ImprovementPointsJson = JsonSerializer.Serialize(evaluation.QualityAssessment.ImprovementPoints),
            RedFlagsJson = JsonSerializer.Serialize(evaluation.QualityAssessment.RedFlags),

            CodeStyle = evaluation.QualityAssessment.CodeStyle,
            DetectedPatternsJson = JsonSerializer.Serialize(evaluation.QualityAssessment.DetectedPatterns),
            CodeSmellsJson = JsonSerializer.Serialize(evaluation.QualityAssessment.CodeSmells)
        };

        _context.CodeRoastSubmissions.Add(submission);
        await _context.SaveChangesAsync();

        // Update user stats
        await UpdateUserStatsAsync(user.Id, submission);

        return MapSubmissionToResultDto(submission, task);
    }

    public async Task<CodeRoastStatsDto?> GetUserStatsAsync(string username)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null) return null;

        var stats = await GetOrCreateUserStats(user.Id);

        return new CodeRoastStatsDto
        {
            TotalSubmissions = stats.TotalSubmissions,
            TotalRoasts = stats.TotalRoasts,
            TotalPraises = stats.TotalPraises,
            AverageScore = stats.AverageScore,
            HighestScore = stats.HighestScore,
            LowestScore = stats.LowestScore,

            CurrentStreak = stats.CurrentStreak,
            LongestStreak = stats.LongestStreak,
            CurrentRoastStreak = stats.CurrentRoastStreak,
            LongestRoastStreak = stats.LongestRoastStreak,

            TotalTimeSpentMinutes = stats.TotalTimeSpentMinutes,
            AverageTimePerTask = stats.AverageTimePerTask,

            JuniorTasksCompleted = stats.JuniorTasksCompleted,
            MiddleTasksCompleted = stats.MiddleTasksCompleted,
            SeniorTasksCompleted = stats.SeniorTasksCompleted,

            AverageReadabilityScore = stats.AverageReadabilityScore,
            AveragePerformanceScore = stats.AveragePerformanceScore,
            AverageCorrectnessScore = stats.AverageCorrectnessScore,
            AverageBestPracticesScore = stats.AverageBestPracticesScore,

            LastSubmission = stats.LastSubmission,
            RecentScores = JsonSerializer.Deserialize<List<int>>(stats.RecentScoresJson) ?? new List<int>(),
            UnlockedAchievements = JsonSerializer.Deserialize<List<string>>(stats.UnlockedAchievementsJson) ?? new List<string>(),
            PerfectScores = stats.PerfectScores
        };
    }

    public async Task<List<CodeRoastResultDto>> GetRoastHistoryAsync(string username, int limit = 10)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null) return new List<CodeRoastResultDto>();

        var submissions = await _context.CodeRoastSubmissions
            .Include(s => s.Task)
            .Where(s => s.UserId == user.Id)
            .OrderByDescending(s => s.SubmittedAt)
            .Take(limit)
            .ToListAsync();

        return submissions.Select(s => MapSubmissionToResultDto(s, s.Task)).ToList();
    }

    public async Task<CodeRoastHallOfFameDto> GetHallOfFameAsync()
    {
        var bestScores = await _context.CodeRoastSubmissions
            .Include(s => s.User)
            .Include(s => s.Task)
            .OrderByDescending(s => s.OverallScore)
            .Take(5)
            .Select(s => new HallOfFameEntryDto
            {
                Username = s.User.Username,
                FirstName = s.User.FirstName,
                LastName = s.User.LastName,
                TechStack = s.User.TechStack,
                ZodiacSign = s.User.ZodiacSign,
                Score = s.OverallScore,
                TaskTitle = s.Task.Title,
                RoastMessage = s.RoastMessage,
                SubmittedAt = s.SubmittedAt,
                RoastSeverity = s.RoastSeverity
            })
            .ToListAsync();

        var worstScores = await _context.CodeRoastSubmissions
            .Include(s => s.User)
            .Include(s => s.Task)
            .OrderBy(s => s.OverallScore)
            .Take(5)
            .Select(s => new HallOfFameEntryDto
            {
                Username = s.User.Username,
                FirstName = s.User.FirstName,
                LastName = s.User.LastName,
                TechStack = s.User.TechStack,
                ZodiacSign = s.User.ZodiacSign,
                Score = s.OverallScore,
                TaskTitle = s.Task.Title,
                RoastMessage = s.RoastMessage,
                SubmittedAt = s.SubmittedAt,
                RoastSeverity = s.RoastSeverity
            })
            .ToListAsync();

        var funniestRoasts = await _context.CodeRoastSubmissions
            .Include(s => s.User)
            .Include(s => s.Task)
            .Where(s => s.RoastSeverity == RoastSeverity.Brutal || s.RoastSeverity == RoastSeverity.Devastating)
            .OrderByDescending(s => s.SubmittedAt)
            .Take(5)
            .Select(s => new HallOfFameEntryDto
            {
                Username = s.User.Username,
                FirstName = s.User.FirstName,
                LastName = s.User.LastName,
                TechStack = s.User.TechStack,
                ZodiacSign = s.User.ZodiacSign,
                Score = s.OverallScore,
                TaskTitle = s.Task.Title,
                RoastMessage = s.RoastMessage,
                SubmittedAt = s.SubmittedAt,
                RoastSeverity = s.RoastSeverity
            })
            .ToListAsync();

        return new CodeRoastHallOfFameDto
        {
            BestScores = bestScores,
            WorstScores = worstScores,
            FunniestRoasts = funniestRoasts,
            MostImprovedUsers = new List<HallOfFameEntryDto>() // TODO: Implement improvement calculation
        };
    }

    public async Task InitializeUserStatsAsync(int userId)
    {
        await GetOrCreateUserStats(userId);
    }

    public async Task<List<CodeRoastTaskDto>> GetRecommendedTasksAsync(string username, int limit = 3)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null) return new List<CodeRoastTaskDto>();

        var userStats = await GetOrCreateUserStats(user.Id);

        // Recommend tasks based on user's weakest areas
        var recommendedDifficulty = DetermineRecommendedDifficulty(userStats);

        var tasks = await _context.CodeRoastTasks
            .Where(t => t.IsActive && t.TechStack == user.TechStack && t.DifficultyLevel == recommendedDifficulty)
            .OrderBy(t => Guid.NewGuid())
            .Take(limit)
            .ToListAsync();

        return tasks.Select(MapTaskToDto).ToList();
    }

    private async Task<CodeRoastStats> GetOrCreateUserStats(int userId)
    {
        var stats = await _context.CodeRoastStats.FirstOrDefaultAsync(s => s.UserId == userId);
        if (stats == null)
        {
            stats = new CodeRoastStats { UserId = userId };
            _context.CodeRoastStats.Add(stats);
            await _context.SaveChangesAsync();
        }
        return stats;
    }

    private async Task UpdateUserStatsAsync(int userId, CodeRoastSubmission submission)
    {
        var stats = await GetOrCreateUserStats(userId);

        // Update basic stats
        stats.TotalSubmissions++;
        stats.TotalTimeSpentMinutes += submission.TimeSpentMinutes;
        stats.AverageTimePerTask = (double)stats.TotalTimeSpentMinutes / stats.TotalSubmissions;
        stats.LastSubmission = submission.SubmittedAt;

        // Update score statistics
        var scores = await _context.CodeRoastSubmissions
            .Where(s => s.UserId == userId)
            .Select(s => s.OverallScore)
            .ToListAsync();

        stats.AverageScore = scores.Average();
        stats.HighestScore = scores.Max();
        stats.LowestScore = scores.Min();

        // Update roast/praise counts
        if (submission.IsRoasted)
        {
            stats.TotalRoasts++;
            stats.CurrentStreak = 0;
            stats.CurrentRoastStreak++;
            if (stats.CurrentRoastStreak > stats.LongestRoastStreak)
                stats.LongestRoastStreak = stats.CurrentRoastStreak;
        }
        else
        {
            stats.TotalPraises++;
            stats.CurrentRoastStreak = 0;
            stats.CurrentStreak++;
            if (stats.CurrentStreak > stats.LongestStreak)
                stats.LongestStreak = stats.CurrentStreak;
        }

        if (submission.IsPerfect)
        {
            stats.PerfectScores++;
        }

        // Update difficulty breakdown
        var task = await _context.CodeRoastTasks.FindAsync(submission.TaskId);
        if (task != null)
        {
            switch (task.DifficultyLevel)
            {
                case ExperienceLevel.Junior:
                    stats.JuniorTasksCompleted++;
                    break;
                case ExperienceLevel.Middle:
                    stats.MiddleTasksCompleted++;
                    break;
                case ExperienceLevel.Senior:
                    stats.SeniorTasksCompleted++;
                    break;
            }
        }

        // Update quality metrics
        var allSubmissions = await _context.CodeRoastSubmissions
            .Where(s => s.UserId == userId)
            .ToListAsync();

        stats.AverageReadabilityScore = allSubmissions.Average(s => s.ReadabilityScore);
        stats.AveragePerformanceScore = allSubmissions.Average(s => s.PerformanceScore);
        stats.AverageCorrectnessScore = allSubmissions.Average(s => s.CorrectnessScore);
        stats.AverageBestPracticesScore = allSubmissions.Average(s => s.BestPracticesScore);

        // Update recent scores (keep last 10)
        var recentScores = JsonSerializer.Deserialize<List<int>>(stats.RecentScoresJson) ?? new List<int>();
        recentScores.Add(submission.OverallScore);
        if (recentScores.Count > 10)
        {
            recentScores.RemoveAt(0);
        }
        stats.RecentScoresJson = JsonSerializer.Serialize(recentScores);

        // Check for achievements
        await CheckAndUnlockAchievements(stats, submission);

        stats.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    private async Task CheckAndUnlockAchievements(CodeRoastStats stats, CodeRoastSubmission submission)
    {
        var achievements = JsonSerializer.Deserialize<List<string>>(stats.UnlockedAchievementsJson) ?? new List<string>();

        // First submission
        if (stats.TotalSubmissions == 1 && !achievements.Contains("FirstSubmission"))
        {
            achievements.Add("FirstSubmission");
        }

        // Perfect score
        if (submission.IsPerfect && !achievements.Contains("PerfectScore"))
        {
            achievements.Add("PerfectScore");
        }

        // Perfectionist (multiple perfect scores)
        if (stats.PerfectScores >= 3 && !achievements.Contains("Perfectionist"))
        {
            achievements.Add("Perfectionist");
        }

        // Survival streak (10 consecutive submissions, regardless of score)
        if (stats.TotalSubmissions >= 10 && !achievements.Contains("SurvivalStreak"))
        {
            achievements.Add("SurvivalStreak");
        }

        // Resilient (kept trying despite roasts)
        if (stats.TotalRoasts >= 5 && stats.TotalSubmissions >= 10 && !achievements.Contains("Resilient"))
        {
            achievements.Add("Resilient");
        }

        stats.UnlockedAchievementsJson = JsonSerializer.Serialize(achievements);
    }

    private ExperienceLevel DetermineRecommendedDifficulty(CodeRoastStats stats)
    {
        if (stats.TotalSubmissions == 0) return ExperienceLevel.Junior;

        // If user is doing well, recommend higher difficulty
        if (stats.AverageScore >= 80)
        {
            if (stats.SeniorTasksCompleted < 5) return ExperienceLevel.Senior;
            if (stats.MiddleTasksCompleted < 10) return ExperienceLevel.Middle;
        }
        else if (stats.AverageScore >= 60)
        {
            if (stats.MiddleTasksCompleted < 5) return ExperienceLevel.Middle;
        }

        return ExperienceLevel.Junior;
    }

    private CodeRoastTaskDto MapTaskToDto(CodeRoastTask task)
    {
        return new CodeRoastTaskDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Requirements = task.Requirements,
            TechStack = task.TechStack,
            DifficultyLevel = task.DifficultyLevel,
            StarterCode = task.StarterCode,
            TestCases = JsonSerializer.Deserialize<List<string>>(task.TestCasesJson) ?? new List<string>(),
            Examples = JsonSerializer.Deserialize<List<string>>(task.ExamplesJson) ?? new List<string>(),
            EstimatedMinutes = task.EstimatedMinutes,
            CreatedAt = task.CreatedAt
        };
    }

    private CodeRoastResultDto MapSubmissionToResultDto(CodeRoastSubmission submission, CodeRoastTask task)
    {
        return new CodeRoastResultDto
        {
            Id = submission.Id,
            TaskId = submission.TaskId,
            TaskTitle = task.Title,
            SubmittedCode = submission.SubmittedCode,
            UserNotes = submission.UserNotes,

            OverallScore = submission.OverallScore,
            RoastMessage = submission.RoastMessage,
            TechnicalFeedback = submission.TechnicalFeedback,

            QualityAssessment = new CodeQualityAssessmentDto
            {
                ReadabilityScore = submission.ReadabilityScore,
                PerformanceScore = submission.PerformanceScore,
                CorrectnessScore = submission.CorrectnessScore,
                BestPracticesScore = submission.BestPracticesScore,
                PositivePoints = JsonSerializer.Deserialize<List<string>>(submission.PositivePointsJson) ?? new List<string>(),
                ImprovementPoints = JsonSerializer.Deserialize<List<string>>(submission.ImprovementPointsJson) ?? new List<string>(),
                RedFlags = JsonSerializer.Deserialize<List<string>>(submission.RedFlagsJson) ?? new List<string>(),
                CodeStyle = submission.CodeStyle,
                DetectedPatterns = JsonSerializer.Deserialize<List<string>>(submission.DetectedPatternsJson) ?? new List<string>(),
                CodeSmells = JsonSerializer.Deserialize<List<string>>(submission.CodeSmellsJson) ?? new List<string>()
            },

            SubmittedAt = submission.SubmittedAt,
            TimeSpentMinutes = submission.TimeSpentMinutes,
            IsRoasted = submission.IsRoasted,
            IsPraised = submission.IsPraised,
            RoastSeverity = submission.RoastSeverity
        };
    }
}