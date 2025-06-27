using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevLife.Migrations
{
    /// <inheritdoc />
    public partial class BugChaseFeatureAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BugChaseScores");

            migrationBuilder.DropTable(
                name: "BugChaseStats");
        }
    }
}
