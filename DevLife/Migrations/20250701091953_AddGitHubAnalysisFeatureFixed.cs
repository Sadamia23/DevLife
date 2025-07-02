using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevLife.Migrations
{
    /// <inheritdoc />
    public partial class AddGitHubAnalysisFeatureFixed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CodeChallenges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    TechStack = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DifficultyLevel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CorrectCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BuggyCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Explanation = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodeChallenges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CodeRoastTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Requirements = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    TechStack = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DifficultyLevel = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StarterCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TestCasesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExamplesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EstimatedMinutes = table.Column<int>(type: "int", nullable: false),
                    Topic = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsAIGenerated = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodeRoastTasks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MeetingExcuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExcuseText = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BelievabilityScore = table.Column<int>(type: "int", nullable: false),
                    TagsJson = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    UsageCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    AverageRating = table.Column<double>(type: "float", nullable: false, defaultValue: 0.0),
                    RatingCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetingExcuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TechStack = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Experience = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ZodiacSign = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DailyChallenges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodeChallengeId = table.Column<int>(type: "int", nullable: false),
                    ChallengeDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BonusMultiplier = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyChallenges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyChallenges_CodeChallenges_CodeChallengeId",
                        column: x => x.CodeChallengeId,
                        principalTable: "CodeChallenges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BugChaseScores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    Distance = table.Column<int>(type: "int", nullable: false),
                    SurvivalTime = table.Column<long>(type: "bigint", nullable: false),
                    BugsAvoided = table.Column<int>(type: "int", nullable: false),
                    DeadlinesAvoided = table.Column<int>(type: "int", nullable: false),
                    MeetingsAvoided = table.Column<int>(type: "int", nullable: false),
                    CoffeeCollected = table.Column<int>(type: "int", nullable: false),
                    WeekendsCollected = table.Column<int>(type: "int", nullable: false),
                    PlayedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BugChaseScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BugChaseScores_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BugChaseStats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    BestScore = table.Column<int>(type: "int", nullable: false),
                    TotalGamesPlayed = table.Column<int>(type: "int", nullable: false),
                    TotalDistance = table.Column<int>(type: "int", nullable: false),
                    TotalSurvivalTime = table.Column<long>(type: "bigint", nullable: false),
                    TotalBugsAvoided = table.Column<int>(type: "int", nullable: false),
                    TotalDeadlinesAvoided = table.Column<int>(type: "int", nullable: false),
                    TotalMeetingsAvoided = table.Column<int>(type: "int", nullable: false),
                    TotalCoffeeCollected = table.Column<int>(type: "int", nullable: false),
                    TotalWeekendsCollected = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BugChaseStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BugChaseStats_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CodeRoastStats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TotalSubmissions = table.Column<int>(type: "int", nullable: false),
                    TotalRoasts = table.Column<int>(type: "int", nullable: false),
                    TotalPraises = table.Column<int>(type: "int", nullable: false),
                    PerfectScores = table.Column<int>(type: "int", nullable: false),
                    AverageScore = table.Column<double>(type: "float", nullable: false),
                    HighestScore = table.Column<int>(type: "int", nullable: false),
                    LowestScore = table.Column<int>(type: "int", nullable: false),
                    CurrentStreak = table.Column<int>(type: "int", nullable: false),
                    LongestStreak = table.Column<int>(type: "int", nullable: false),
                    CurrentRoastStreak = table.Column<int>(type: "int", nullable: false),
                    LongestRoastStreak = table.Column<int>(type: "int", nullable: false),
                    TotalTimeSpentMinutes = table.Column<int>(type: "int", nullable: false),
                    AverageTimePerTask = table.Column<double>(type: "float", nullable: false),
                    JuniorTasksCompleted = table.Column<int>(type: "int", nullable: false),
                    MiddleTasksCompleted = table.Column<int>(type: "int", nullable: false),
                    SeniorTasksCompleted = table.Column<int>(type: "int", nullable: false),
                    AverageReadabilityScore = table.Column<double>(type: "float", nullable: false),
                    AveragePerformanceScore = table.Column<double>(type: "float", nullable: false),
                    AverageCorrectnessScore = table.Column<double>(type: "float", nullable: false),
                    AverageBestPracticesScore = table.Column<double>(type: "float", nullable: false),
                    RecentScoresJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UnlockedAchievementsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastSubmission = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodeRoastStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CodeRoastStats_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CodeRoastSubmissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    SubmittedCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    OverallScore = table.Column<int>(type: "int", nullable: false),
                    RoastMessage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TechnicalFeedback = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReadabilityScore = table.Column<int>(type: "int", nullable: false),
                    PerformanceScore = table.Column<int>(type: "int", nullable: false),
                    CorrectnessScore = table.Column<int>(type: "int", nullable: false),
                    BestPracticesScore = table.Column<int>(type: "int", nullable: false),
                    PositivePointsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImprovementPointsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RedFlagsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CodeStyle = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DetectedPatternsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CodeSmellsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EvaluatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimeSpentMinutes = table.Column<int>(type: "int", nullable: false),
                    RoastSeverity = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodeRoastSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CodeRoastSubmissions_CodeRoastTasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "CodeRoastTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CodeRoastSubmissions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GitHubAnalysisResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    PersonalityType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PersonalityDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    StrengthsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WeaknessesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CelebrityDevelopersJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RepositoriesAnalyzed = table.Column<int>(type: "int", nullable: false),
                    TotalCommits = table.Column<int>(type: "int", nullable: false),
                    TotalFiles = table.Column<int>(type: "int", nullable: false),
                    CommitMessageQuality = table.Column<int>(type: "int", nullable: false),
                    CodeCommentingScore = table.Column<int>(type: "int", nullable: false),
                    VariableNamingScore = table.Column<int>(type: "int", nullable: false),
                    ProjectStructureScore = table.Column<int>(type: "int", nullable: false),
                    OverallScore = table.Column<int>(type: "int", nullable: false),
                    AnalysisDetailsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GitHubUsername = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AnalyzedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SharedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ShareCount = table.Column<int>(type: "int", nullable: false),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GitHubAnalysisResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GitHubAnalysisResults_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GitHubAnalysisStats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TotalAnalyses = table.Column<int>(type: "int", nullable: false),
                    TotalRepositoriesAnalyzed = table.Column<int>(type: "int", nullable: false),
                    TotalCommitsAnalyzed = table.Column<int>(type: "int", nullable: false),
                    AverageOverallScore = table.Column<double>(type: "float", nullable: false),
                    AverageCommitQuality = table.Column<double>(type: "float", nullable: false),
                    AverageCommentingScore = table.Column<double>(type: "float", nullable: false),
                    AverageNamingScore = table.Column<double>(type: "float", nullable: false),
                    AverageStructureScore = table.Column<double>(type: "float", nullable: false),
                    MostCommonPersonalityType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FavoriteLanguage = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RecentAnalysesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UnlockedAchievementsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastAnalysis = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FirstAnalysis = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CurrentStreak = table.Column<int>(type: "int", nullable: false),
                    LongestStreak = table.Column<int>(type: "int", nullable: false),
                    ShareCount = table.Column<int>(type: "int", nullable: false),
                    FavoriteCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GitHubAnalysisStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GitHubAnalysisStats_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MeetingExcuseFavorites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    MeetingExcuseId = table.Column<int>(type: "int", nullable: false),
                    CustomName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    UserRating = table.Column<int>(type: "int", nullable: true),
                    SavedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetingExcuseFavorites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeetingExcuseFavorites_MeetingExcuses_MeetingExcuseId",
                        column: x => x.MeetingExcuseId,
                        principalTable: "MeetingExcuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MeetingExcuseFavorites_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MeetingExcuseStats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TotalExcusesGenerated = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    TotalFavorites = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    FavoriteCategory = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FavoriteType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AverageBelievability = table.Column<double>(type: "float", nullable: false, defaultValue: 0.0),
                    RecentExcusesJson = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CurrentStreak = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    LongestStreak = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    UnlockedAchievementsJson = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    LastExcuseGenerated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetingExcuseStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeetingExcuseStats_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MeetingExcuseUsages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    MeetingExcuseId = table.Column<int>(type: "int", nullable: false),
                    Context = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    WasSuccessful = table.Column<bool>(type: "bit", nullable: true),
                    UsedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetingExcuseUsages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeetingExcuseUsages_MeetingExcuses_MeetingExcuseId",
                        column: x => x.MeetingExcuseId,
                        principalTable: "MeetingExcuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MeetingExcuseUsages_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserGameSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CodeChallengeId = table.Column<int>(type: "int", nullable: true),
                    AIChallengeTitle = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AIChallengeDescription = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AICorrectCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AIBuggyCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AIExplanation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AITopic = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PointsBet = table.Column<int>(type: "int", nullable: false),
                    UserChoice = table.Column<bool>(type: "bit", nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false),
                    PointsWon = table.Column<int>(type: "int", nullable: false),
                    LuckMultiplier = table.Column<double>(type: "float", nullable: false),
                    PlayedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDailyChallenge = table.Column<bool>(type: "bit", nullable: false),
                    IsAIGenerated = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGameSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserGameSessions_CodeChallenges_CodeChallengeId",
                        column: x => x.CodeChallengeId,
                        principalTable: "CodeChallenges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_UserGameSessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserStats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TotalPoints = table.Column<int>(type: "int", nullable: false),
                    CurrentStreak = table.Column<int>(type: "int", nullable: false),
                    LongestStreak = table.Column<int>(type: "int", nullable: false),
                    TotalGamesPlayed = table.Column<int>(type: "int", nullable: false),
                    TotalGamesWon = table.Column<int>(type: "int", nullable: false),
                    LastDailyChallenge = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserStats_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GitHubAnalysisFavorites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    GitHubAnalysisResultId = table.Column<int>(type: "int", nullable: false),
                    CustomName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SavedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GitHubAnalysisFavorites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GitHubAnalysisFavorites_GitHubAnalysisResults_GitHubAnalysisResultId",
                        column: x => x.GitHubAnalysisResultId,
                        principalTable: "GitHubAnalysisResults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GitHubAnalysisFavorites_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "GitHubRepositories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnalysisResultId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    PrimaryLanguage = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StarsCount = table.Column<int>(type: "int", nullable: false),
                    ForksCount = table.Column<int>(type: "int", nullable: false),
                    CommitsAnalyzed = table.Column<int>(type: "int", nullable: false),
                    FilesAnalyzed = table.Column<int>(type: "int", nullable: false),
                    RepoCommitQuality = table.Column<int>(type: "int", nullable: false),
                    RepoCommentingScore = table.Column<int>(type: "int", nullable: false),
                    RepoNamingScore = table.Column<int>(type: "int", nullable: false),
                    RepoStructureScore = table.Column<int>(type: "int", nullable: false),
                    RepositoryDetailsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AnalyzedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GitHubRepositories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GitHubRepositories_GitHubAnalysisResults_AnalysisResultId",
                        column: x => x.AnalysisResultId,
                        principalTable: "GitHubAnalysisResults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BugChaseScores_PlayedAt",
                table: "BugChaseScores",
                column: "PlayedAt");

            migrationBuilder.CreateIndex(
                name: "IX_BugChaseScores_Score_Distance_PlayedAt",
                table: "BugChaseScores",
                columns: new[] { "Score", "Distance", "PlayedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_BugChaseScores_UserId",
                table: "BugChaseScores",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BugChaseStats_UserId",
                table: "BugChaseStats",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CodeRoastStats_AverageScore",
                table: "CodeRoastStats",
                column: "AverageScore");

            migrationBuilder.CreateIndex(
                name: "IX_CodeRoastStats_LastSubmission",
                table: "CodeRoastStats",
                column: "LastSubmission");

            migrationBuilder.CreateIndex(
                name: "IX_CodeRoastStats_TotalSubmissions",
                table: "CodeRoastStats",
                column: "TotalSubmissions");

            migrationBuilder.CreateIndex(
                name: "IX_CodeRoastStats_UserId",
                table: "CodeRoastStats",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CodeRoastSubmissions_OverallScore",
                table: "CodeRoastSubmissions",
                column: "OverallScore");

            migrationBuilder.CreateIndex(
                name: "IX_CodeRoastSubmissions_TaskId_OverallScore",
                table: "CodeRoastSubmissions",
                columns: new[] { "TaskId", "OverallScore" });

            migrationBuilder.CreateIndex(
                name: "IX_CodeRoastSubmissions_UserId_SubmittedAt",
                table: "CodeRoastSubmissions",
                columns: new[] { "UserId", "SubmittedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CodeRoastTasks_CreatedAt",
                table: "CodeRoastTasks",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CodeRoastTasks_TechStack_DifficultyLevel_IsActive",
                table: "CodeRoastTasks",
                columns: new[] { "TechStack", "DifficultyLevel", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_DailyChallenges_ChallengeDate",
                table: "DailyChallenges",
                column: "ChallengeDate",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DailyChallenges_CodeChallengeId",
                table: "DailyChallenges",
                column: "CodeChallengeId");

            migrationBuilder.CreateIndex(
                name: "IX_GitHubAnalysisFavorites_GitHubAnalysisResultId",
                table: "GitHubAnalysisFavorites",
                column: "GitHubAnalysisResultId");

            migrationBuilder.CreateIndex(
                name: "IX_GitHubAnalysisFavorites_UserId_GitHubAnalysisResultId",
                table: "GitHubAnalysisFavorites",
                columns: new[] { "UserId", "GitHubAnalysisResultId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GitHubAnalysisFavorites_UserId_SavedAt",
                table: "GitHubAnalysisFavorites",
                columns: new[] { "UserId", "SavedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_GitHubAnalysisResults_GitHubUsername",
                table: "GitHubAnalysisResults",
                column: "GitHubUsername");

            migrationBuilder.CreateIndex(
                name: "IX_GitHubAnalysisResults_IsPublic_ShareCount",
                table: "GitHubAnalysisResults",
                columns: new[] { "IsPublic", "ShareCount" });

            migrationBuilder.CreateIndex(
                name: "IX_GitHubAnalysisResults_OverallScore",
                table: "GitHubAnalysisResults",
                column: "OverallScore");

            migrationBuilder.CreateIndex(
                name: "IX_GitHubAnalysisResults_UserId_AnalyzedAt",
                table: "GitHubAnalysisResults",
                columns: new[] { "UserId", "AnalyzedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_GitHubAnalysisStats_AverageOverallScore",
                table: "GitHubAnalysisStats",
                column: "AverageOverallScore");

            migrationBuilder.CreateIndex(
                name: "IX_GitHubAnalysisStats_CurrentStreak",
                table: "GitHubAnalysisStats",
                column: "CurrentStreak");

            migrationBuilder.CreateIndex(
                name: "IX_GitHubAnalysisStats_LastAnalysis",
                table: "GitHubAnalysisStats",
                column: "LastAnalysis");

            migrationBuilder.CreateIndex(
                name: "IX_GitHubAnalysisStats_TotalAnalyses",
                table: "GitHubAnalysisStats",
                column: "TotalAnalyses");

            migrationBuilder.CreateIndex(
                name: "IX_GitHubAnalysisStats_UserId",
                table: "GitHubAnalysisStats",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GitHubRepositories_AnalysisResultId",
                table: "GitHubRepositories",
                column: "AnalysisResultId");

            migrationBuilder.CreateIndex(
                name: "IX_GitHubRepositories_AnalyzedAt",
                table: "GitHubRepositories",
                column: "AnalyzedAt");

            migrationBuilder.CreateIndex(
                name: "IX_GitHubRepositories_PrimaryLanguage_StarsCount",
                table: "GitHubRepositories",
                columns: new[] { "PrimaryLanguage", "StarsCount" });

            migrationBuilder.CreateIndex(
                name: "IX_MeetingExcuseFavorites_MeetingExcuseId",
                table: "MeetingExcuseFavorites",
                column: "MeetingExcuseId");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingExcuseFavorites_UserId_MeetingExcuseId",
                table: "MeetingExcuseFavorites",
                columns: new[] { "UserId", "MeetingExcuseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MeetingExcuseFavorites_UserId_SavedAt",
                table: "MeetingExcuseFavorites",
                columns: new[] { "UserId", "SavedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_MeetingExcuses_AverageRating_RatingCount",
                table: "MeetingExcuses",
                columns: new[] { "AverageRating", "RatingCount" });

            migrationBuilder.CreateIndex(
                name: "IX_MeetingExcuses_BelievabilityScore_IsActive",
                table: "MeetingExcuses",
                columns: new[] { "BelievabilityScore", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_MeetingExcuses_Category_Type_IsActive",
                table: "MeetingExcuses",
                columns: new[] { "Category", "Type", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_MeetingExcuses_UsageCount",
                table: "MeetingExcuses",
                column: "UsageCount");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingExcuseStats_CurrentStreak",
                table: "MeetingExcuseStats",
                column: "CurrentStreak");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingExcuseStats_LastExcuseGenerated",
                table: "MeetingExcuseStats",
                column: "LastExcuseGenerated");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingExcuseStats_LongestStreak",
                table: "MeetingExcuseStats",
                column: "LongestStreak");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingExcuseStats_TotalExcusesGenerated",
                table: "MeetingExcuseStats",
                column: "TotalExcusesGenerated");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingExcuseStats_UserId",
                table: "MeetingExcuseStats",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MeetingExcuseUsages_MeetingExcuseId_UsedAt",
                table: "MeetingExcuseUsages",
                columns: new[] { "MeetingExcuseId", "UsedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_MeetingExcuseUsages_UsedAt",
                table: "MeetingExcuseUsages",
                column: "UsedAt");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingExcuseUsages_UserId_UsedAt",
                table: "MeetingExcuseUsages",
                columns: new[] { "UserId", "UsedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_UserGameSessions_CodeChallengeId",
                table: "UserGameSessions",
                column: "CodeChallengeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGameSessions_UserId",
                table: "UserGameSessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserStats_UserId",
                table: "UserStats",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BugChaseScores");

            migrationBuilder.DropTable(
                name: "BugChaseStats");

            migrationBuilder.DropTable(
                name: "CodeRoastStats");

            migrationBuilder.DropTable(
                name: "CodeRoastSubmissions");

            migrationBuilder.DropTable(
                name: "DailyChallenges");

            migrationBuilder.DropTable(
                name: "GitHubAnalysisFavorites");

            migrationBuilder.DropTable(
                name: "GitHubAnalysisStats");

            migrationBuilder.DropTable(
                name: "GitHubRepositories");

            migrationBuilder.DropTable(
                name: "MeetingExcuseFavorites");

            migrationBuilder.DropTable(
                name: "MeetingExcuseStats");

            migrationBuilder.DropTable(
                name: "MeetingExcuseUsages");

            migrationBuilder.DropTable(
                name: "UserGameSessions");

            migrationBuilder.DropTable(
                name: "UserStats");

            migrationBuilder.DropTable(
                name: "CodeRoastTasks");

            migrationBuilder.DropTable(
                name: "GitHubAnalysisResults");

            migrationBuilder.DropTable(
                name: "MeetingExcuses");

            migrationBuilder.DropTable(
                name: "CodeChallenges");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
