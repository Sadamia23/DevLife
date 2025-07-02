using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevLife.Migrations
{
    /// <inheritdoc />
    public partial class DevDatingAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserGameSessions_CodeChallenges_CodeChallengeId",
                table: "UserGameSessions");

            migrationBuilder.DropIndex(
                name: "IX_UserStats_UserId",
                table: "UserStats");

            migrationBuilder.DropIndex(
                name: "IX_MeetingExcuseUsages_MeetingExcuseId_UsedAt",
                table: "MeetingExcuseUsages");

            migrationBuilder.DropIndex(
                name: "IX_MeetingExcuseUsages_UsedAt",
                table: "MeetingExcuseUsages");

            migrationBuilder.DropIndex(
                name: "IX_MeetingExcuseUsages_UserId_UsedAt",
                table: "MeetingExcuseUsages");

            migrationBuilder.DropIndex(
                name: "IX_MeetingExcuseStats_CurrentStreak",
                table: "MeetingExcuseStats");

            migrationBuilder.DropIndex(
                name: "IX_MeetingExcuseStats_LastExcuseGenerated",
                table: "MeetingExcuseStats");

            migrationBuilder.DropIndex(
                name: "IX_MeetingExcuseStats_LongestStreak",
                table: "MeetingExcuseStats");

            migrationBuilder.DropIndex(
                name: "IX_MeetingExcuseStats_TotalExcusesGenerated",
                table: "MeetingExcuseStats");

            migrationBuilder.DropIndex(
                name: "IX_MeetingExcuseStats_UserId",
                table: "MeetingExcuseStats");

            migrationBuilder.DropIndex(
                name: "IX_MeetingExcuseFavorites_UserId_MeetingExcuseId",
                table: "MeetingExcuseFavorites");

            migrationBuilder.DropIndex(
                name: "IX_MeetingExcuseFavorites_UserId_SavedAt",
                table: "MeetingExcuseFavorites");

            migrationBuilder.DropIndex(
                name: "IX_GitHubRepositories_AnalyzedAt",
                table: "GitHubRepositories");

            migrationBuilder.DropIndex(
                name: "IX_GitHubRepositories_PrimaryLanguage_StarsCount",
                table: "GitHubRepositories");

            migrationBuilder.DropIndex(
                name: "IX_GitHubAnalysisStats_AverageOverallScore",
                table: "GitHubAnalysisStats");

            migrationBuilder.DropIndex(
                name: "IX_GitHubAnalysisStats_CurrentStreak",
                table: "GitHubAnalysisStats");

            migrationBuilder.DropIndex(
                name: "IX_GitHubAnalysisStats_LastAnalysis",
                table: "GitHubAnalysisStats");

            migrationBuilder.DropIndex(
                name: "IX_GitHubAnalysisStats_TotalAnalyses",
                table: "GitHubAnalysisStats");

            migrationBuilder.DropIndex(
                name: "IX_GitHubAnalysisStats_UserId",
                table: "GitHubAnalysisStats");

            migrationBuilder.DropIndex(
                name: "IX_DailyChallenges_ChallengeDate",
                table: "DailyChallenges");

            migrationBuilder.AlterColumn<string>(
                name: "AITopic",
                table: "UserGameSessions",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AIChallengeTitle",
                table: "UserGameSessions",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AIChallengeDescription",
                table: "UserGameSessions",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TotalFavorites",
                table: "MeetingExcuseStats",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "TotalExcusesGenerated",
                table: "MeetingExcuseStats",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "LongestStreak",
                table: "MeetingExcuseStats",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "FavoriteType",
                table: "MeetingExcuseStats",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "FavoriteCategory",
                table: "MeetingExcuseStats",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CurrentStreak",
                table: "MeetingExcuseStats",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<double>(
                name: "AverageBelievability",
                table: "MeetingExcuseStats",
                type: "float",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float",
                oldDefaultValue: 0.0);

            migrationBuilder.CreateTable(
                name: "DatingProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Gender = table.Column<int>(type: "int", nullable: false),
                    Preference = table.Column<int>(type: "int", nullable: false),
                    Bio = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatingProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DatingProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DatingStats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TotalSwipes = table.Column<int>(type: "int", nullable: false),
                    TotalLikes = table.Column<int>(type: "int", nullable: false),
                    TotalMatches = table.Column<int>(type: "int", nullable: false),
                    TotalMessages = table.Column<int>(type: "int", nullable: false),
                    LastActiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatingStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DatingStats_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Matches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    User1Id = table.Column<int>(type: "int", nullable: false),
                    User2Id = table.Column<int>(type: "int", nullable: false),
                    MatchedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Matches_Users_User1Id",
                        column: x => x.User1Id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Matches_Users_User2Id",
                        column: x => x.User2Id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SwipeActions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SwiperId = table.Column<int>(type: "int", nullable: false),
                    SwipedUserId = table.Column<int>(type: "int", nullable: false),
                    IsLike = table.Column<bool>(type: "bit", nullable: false),
                    SwipedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SwipeActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SwipeActions_Users_SwipedUserId",
                        column: x => x.SwipedUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SwipeActions_Users_SwiperId",
                        column: x => x.SwiperId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MatchId = table.Column<int>(type: "int", nullable: false),
                    SenderId = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IsAIGenerated = table.Column<bool>(type: "bit", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatMessages_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChatMessages_Users_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserStats_UserId",
                table: "UserStats",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingExcuseUsages_MeetingExcuseId",
                table: "MeetingExcuseUsages",
                column: "MeetingExcuseId");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingExcuseUsages_UserId",
                table: "MeetingExcuseUsages",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingExcuseStats_UserId",
                table: "MeetingExcuseStats",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingExcuseFavorites_UserId",
                table: "MeetingExcuseFavorites",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GitHubAnalysisStats_UserId",
                table: "GitHubAnalysisStats",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_MatchId",
                table: "ChatMessages",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_SenderId",
                table: "ChatMessages",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_DatingProfiles_UserId",
                table: "DatingProfiles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DatingStats_UserId",
                table: "DatingStats",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Matches_User1Id_User2Id",
                table: "Matches",
                columns: new[] { "User1Id", "User2Id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Matches_User2Id",
                table: "Matches",
                column: "User2Id");

            migrationBuilder.CreateIndex(
                name: "IX_SwipeActions_SwipedUserId",
                table: "SwipeActions",
                column: "SwipedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SwipeActions_SwiperId_SwipedUserId",
                table: "SwipeActions",
                columns: new[] { "SwiperId", "SwipedUserId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserGameSessions_CodeChallenges_CodeChallengeId",
                table: "UserGameSessions",
                column: "CodeChallengeId",
                principalTable: "CodeChallenges",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserGameSessions_CodeChallenges_CodeChallengeId",
                table: "UserGameSessions");

            migrationBuilder.DropTable(
                name: "ChatMessages");

            migrationBuilder.DropTable(
                name: "DatingProfiles");

            migrationBuilder.DropTable(
                name: "DatingStats");

            migrationBuilder.DropTable(
                name: "SwipeActions");

            migrationBuilder.DropTable(
                name: "Matches");

            migrationBuilder.DropIndex(
                name: "IX_UserStats_UserId",
                table: "UserStats");

            migrationBuilder.DropIndex(
                name: "IX_MeetingExcuseUsages_MeetingExcuseId",
                table: "MeetingExcuseUsages");

            migrationBuilder.DropIndex(
                name: "IX_MeetingExcuseUsages_UserId",
                table: "MeetingExcuseUsages");

            migrationBuilder.DropIndex(
                name: "IX_MeetingExcuseStats_UserId",
                table: "MeetingExcuseStats");

            migrationBuilder.DropIndex(
                name: "IX_MeetingExcuseFavorites_UserId",
                table: "MeetingExcuseFavorites");

            migrationBuilder.DropIndex(
                name: "IX_GitHubAnalysisStats_UserId",
                table: "GitHubAnalysisStats");

            migrationBuilder.AlterColumn<string>(
                name: "AITopic",
                table: "UserGameSessions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AIChallengeTitle",
                table: "UserGameSessions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AIChallengeDescription",
                table: "UserGameSessions",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TotalFavorites",
                table: "MeetingExcuseStats",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "TotalExcusesGenerated",
                table: "MeetingExcuseStats",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "LongestStreak",
                table: "MeetingExcuseStats",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "FavoriteType",
                table: "MeetingExcuseStats",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FavoriteCategory",
                table: "MeetingExcuseStats",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CurrentStreak",
                table: "MeetingExcuseStats",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<double>(
                name: "AverageBelievability",
                table: "MeetingExcuseStats",
                type: "float",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.CreateIndex(
                name: "IX_UserStats_UserId",
                table: "UserStats",
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
                name: "IX_MeetingExcuseFavorites_UserId_MeetingExcuseId",
                table: "MeetingExcuseFavorites",
                columns: new[] { "UserId", "MeetingExcuseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MeetingExcuseFavorites_UserId_SavedAt",
                table: "MeetingExcuseFavorites",
                columns: new[] { "UserId", "SavedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_GitHubRepositories_AnalyzedAt",
                table: "GitHubRepositories",
                column: "AnalyzedAt");

            migrationBuilder.CreateIndex(
                name: "IX_GitHubRepositories_PrimaryLanguage_StarsCount",
                table: "GitHubRepositories",
                columns: new[] { "PrimaryLanguage", "StarsCount" });

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
                name: "IX_DailyChallenges_ChallengeDate",
                table: "DailyChallenges",
                column: "ChallengeDate",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserGameSessions_CodeChallenges_CodeChallengeId",
                table: "UserGameSessions",
                column: "CodeChallengeId",
                principalTable: "CodeChallenges",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
