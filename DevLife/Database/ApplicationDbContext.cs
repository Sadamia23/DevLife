using DevLife.Models;
using DevLife.Models.BugChase;
using DevLife.Models.CodeCasino;
using DevLife.Models.CodeRoast;
using DevLife.Models.MeetingExcuse;
using Microsoft.EntityFrameworkCore;

namespace DevLife.Database;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // Existing entities
    public DbSet<User> Users { get; set; }
    public DbSet<CodeChallenge> CodeChallenges { get; set; }
    public DbSet<UserGameSession> UserGameSessions { get; set; }
    public DbSet<UserStats> UserStats { get; set; }
    public DbSet<DailyChallenge> DailyChallenges { get; set; }

    // Bug Chase entities
    public DbSet<BugChaseScore> BugChaseScores { get; set; }
    public DbSet<BugChaseStats> BugChaseStats { get; set; }

    // Code Roast entities
    public DbSet<CodeRoastTask> CodeRoastTasks { get; set; }
    public DbSet<CodeRoastSubmission> CodeRoastSubmissions { get; set; }
    public DbSet<CodeRoastStats> CodeRoastStats { get; set; }
    
    // Meeting Excuse entities
    public DbSet<MeetingExcuse> MeetingExcuses { get; set; }
    public DbSet<MeetingExcuseFavorite> MeetingExcuseFavorites { get; set; }
    public DbSet<MeetingExcuseUsage> MeetingExcuseUsages { get; set; }
    public DbSet<MeetingExcuseStats> MeetingExcuseStats { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.TechStack).HasConversion<string>();
            entity.Property(e => e.Experience).HasConversion<string>();
            entity.Property(e => e.ZodiacSign).HasConversion<string>();
        });

        modelBuilder.Entity<CodeChallenge>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.CorrectCode).IsRequired();
            entity.Property(e => e.BuggyCode).IsRequired();
            entity.Property(e => e.Explanation).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.TechStack).HasConversion<string>();
            entity.Property(e => e.DifficultyLevel).HasConversion<string>();
        });

        modelBuilder.Entity<UserGameSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.CodeChallenge)
                  .WithMany()
                  .HasForeignKey(e => e.CodeChallengeId)
                  .OnDelete(DeleteBehavior.SetNull)
                  .IsRequired(false);

            entity.Property(e => e.AIChallengeTitle).HasMaxLength(500);
            entity.Property(e => e.AIChallengeDescription).HasMaxLength(2000);
            entity.Property(e => e.AICorrectCode).HasColumnType("nvarchar(max)");
            entity.Property(e => e.AIBuggyCode).HasColumnType("nvarchar(max)");
            entity.Property(e => e.AIExplanation).HasColumnType("nvarchar(max)");
            entity.Property(e => e.AITopic).HasMaxLength(200);
        });

        modelBuilder.Entity<UserStats>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DailyChallenge>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ChallengeDate).IsUnique();
            entity.HasOne(e => e.CodeChallenge)
                  .WithMany()
                  .HasForeignKey(e => e.CodeChallengeId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BugChaseScore>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.SurvivalTime)
                  .HasConversion(
                      timespan => timespan.Ticks,
                      ticks => new TimeSpan(ticks));

            entity.HasIndex(e => new { e.Score, e.Distance, e.PlayedAt });
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.PlayedAt);
        });

        modelBuilder.Entity<BugChaseStats>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.TotalSurvivalTime)
                  .HasConversion(
                      timespan => timespan.Ticks,
                      ticks => new TimeSpan(ticks));
        });

        modelBuilder.Entity<CodeRoastTask>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.Requirements).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.TechStack).HasConversion<string>();
            entity.Property(e => e.DifficultyLevel).HasConversion<string>();
            entity.Property(e => e.StarterCode).HasColumnType("nvarchar(max)");
            entity.Property(e => e.TestCasesJson).HasColumnType("nvarchar(max)");
            entity.Property(e => e.ExamplesJson).HasColumnType("nvarchar(max)");
            entity.Property(e => e.Topic).HasMaxLength(100);

            entity.HasIndex(e => new { e.TechStack, e.DifficultyLevel, e.IsActive });
            entity.HasIndex(e => e.CreatedAt);
        });

        modelBuilder.Entity<CodeRoastSubmission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Task)
                  .WithMany(t => t.Submissions)
                  .HasForeignKey(e => e.TaskId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.SubmittedCode).HasColumnType("nvarchar(max)");
            entity.Property(e => e.RoastMessage).HasColumnType("nvarchar(max)");
            entity.Property(e => e.TechnicalFeedback).HasColumnType("nvarchar(max)");
            entity.Property(e => e.PositivePointsJson).HasColumnType("nvarchar(max)");
            entity.Property(e => e.ImprovementPointsJson).HasColumnType("nvarchar(max)");
            entity.Property(e => e.RedFlagsJson).HasColumnType("nvarchar(max)");
            entity.Property(e => e.DetectedPatternsJson).HasColumnType("nvarchar(max)");
            entity.Property(e => e.CodeSmellsJson).HasColumnType("nvarchar(max)");

            entity.Property(e => e.RoastSeverity).HasConversion<string>();

            entity.HasIndex(e => new { e.UserId, e.SubmittedAt });
            entity.HasIndex(e => e.OverallScore);
            entity.HasIndex(e => new { e.TaskId, e.OverallScore });
        });

        modelBuilder.Entity<CodeRoastStats>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.RecentScoresJson).HasColumnType("nvarchar(max)");
            entity.Property(e => e.UnlockedAchievementsJson).HasColumnType("nvarchar(max)");

            entity.HasIndex(e => e.AverageScore);
            entity.HasIndex(e => e.TotalSubmissions);
            entity.HasIndex(e => e.LastSubmission);
        });

        modelBuilder.Entity<MeetingExcuse>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ExcuseText).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Category).HasConversion<string>();
            entity.Property(e => e.Type).HasConversion<string>();
            entity.Property(e => e.BelievabilityScore).IsRequired();
            entity.Property(e => e.TagsJson).HasColumnType("nvarchar(1000)");
            entity.Property(e => e.UsageCount).HasDefaultValue(0);
            entity.Property(e => e.AverageRating).HasDefaultValue(0.0);
            entity.Property(e => e.RatingCount).HasDefaultValue(0);
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasIndex(e => new { e.Category, e.Type, e.IsActive });
            entity.HasIndex(e => new { e.BelievabilityScore, e.IsActive });
            entity.HasIndex(e => new { e.AverageRating, e.RatingCount });
            entity.HasIndex(e => e.UsageCount);
        });

        modelBuilder.Entity<MeetingExcuseFavorite>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.MeetingExcuse)
                  .WithMany(me => me.Favorites)
                  .HasForeignKey(e => e.MeetingExcuseId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.CustomName).HasMaxLength(200);
            entity.Property(e => e.UserRating).IsRequired(false);

            entity.HasIndex(e => new { e.UserId, e.MeetingExcuseId }).IsUnique();
            entity.HasIndex(e => new { e.UserId, e.SavedAt });
        });

        modelBuilder.Entity<MeetingExcuseUsage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.MeetingExcuse)
                  .WithMany(me => me.UsageHistory)
                  .HasForeignKey(e => e.MeetingExcuseId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.Context).HasMaxLength(500);
            entity.Property(e => e.WasSuccessful).IsRequired(false);

            entity.HasIndex(e => new { e.UserId, e.UsedAt });
            entity.HasIndex(e => new { e.MeetingExcuseId, e.UsedAt });
            entity.HasIndex(e => e.UsedAt);
        });

        modelBuilder.Entity<MeetingExcuseStats>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.TotalExcusesGenerated).HasDefaultValue(0);
            entity.Property(e => e.TotalFavorites).HasDefaultValue(0);
            entity.Property(e => e.FavoriteCategory).HasConversion<string>().IsRequired(false);
            entity.Property(e => e.FavoriteType).HasConversion<string>().IsRequired(false);
            entity.Property(e => e.AverageBelievability).HasDefaultValue(0.0);
            entity.Property(e => e.RecentExcusesJson).HasColumnType("nvarchar(2000)");
            entity.Property(e => e.CurrentStreak).HasDefaultValue(0);
            entity.Property(e => e.LongestStreak).HasDefaultValue(0);
            entity.Property(e => e.UnlockedAchievementsJson).HasColumnType("nvarchar(2000)");

            entity.HasIndex(e => e.TotalExcusesGenerated);
            entity.HasIndex(e => e.CurrentStreak);
            entity.HasIndex(e => e.LongestStreak);
            entity.HasIndex(e => e.LastExcuseGenerated);
        });

        base.OnModelCreating(modelBuilder);
    }
}
