using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevLife.Migrations
{
    /// <inheritdoc />
    public partial class MeetingExcuseAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MeetingExcuseFavorites");

            migrationBuilder.DropTable(
                name: "MeetingExcuseStats");

            migrationBuilder.DropTable(
                name: "MeetingExcuseUsages");

            migrationBuilder.DropTable(
                name: "MeetingExcuses");
        }
    }
}
