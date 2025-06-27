using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevLife.Migrations
{
    /// <inheritdoc />
    public partial class CodeRoastAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CodeRoastStats");

            migrationBuilder.DropTable(
                name: "CodeRoastSubmissions");

            migrationBuilder.DropTable(
                name: "CodeRoastTasks");
        }
    }
}
