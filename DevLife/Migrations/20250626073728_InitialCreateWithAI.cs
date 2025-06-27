using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevLife.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateWithAI : Migration
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
                name: "IX_UserGameSessions_CodeChallengeId",
                table: "UserGameSessions",
                column: "CodeChallengeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGameSessions_UserId",
                table: "UserGameSessions",
                column: "UserId");

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
                name: "DailyChallenges");

            migrationBuilder.DropTable(
                name: "UserGameSessions");

            migrationBuilder.DropTable(
                name: "UserStats");

            migrationBuilder.DropTable(
                name: "CodeChallenges");
        }
    }
}
