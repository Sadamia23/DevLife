using DevLife.Models;
using DevLife.Models.BugChase;
using DevLife.Models.CodeCasino;
using DevLife.Models.CodeRoast;
using DevLife.Models.DevDating;
using DevLife.Models.GitHubAnalysis;
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

    // Github Analysis Entities
    public DbSet<GitHubAnalysisResult> GitHubAnalysisResults { get; set; }
    public DbSet<GitHubRepository> GitHubRepositories { get; set; }
    public DbSet<GitHubAnalysisFavorite> GitHubAnalysisFavorites { get; set; }
    public DbSet<GitHubAnalysisStats> GitHubAnalysisStats { get; set; }

    // Dev Dating entities
    public DbSet<DatingProfile> DatingProfiles { get; set; }
    public DbSet<SwipeAction> SwipeActions { get; set; }
    public DbSet<Match> Matches { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }
    public DbSet<DatingStats> DatingStats { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
