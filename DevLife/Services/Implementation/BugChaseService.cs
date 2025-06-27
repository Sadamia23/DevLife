using DevLife.Database;
using DevLife.Dtos;
using DevLife.Models.BugChase;
using DevLife.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DevLife.Services.Implementation;

public class BugChaseService : IBugChaseService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<BugChaseService> _logger;

    public BugChaseService(ApplicationDbContext context, ILogger<BugChaseService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<BugChaseGameResultDto> SubmitScoreAsync(string username, BugChaseScoreDto scoreDto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null) throw new InvalidOperationException("User not found");

        // Get or create user stats
        var userStats = await GetOrCreateUserStats(user.Id);

        // Create new score record
        var newScore = new BugChaseScore
        {
            UserId = user.Id,
            Score = scoreDto.Score,
            Distance = scoreDto.Distance,
            SurvivalTime = scoreDto.SurvivalTime,
            BugsAvoided = scoreDto.BugsAvoided,
            DeadlinesAvoided = scoreDto.DeadlinesAvoided,
            MeetingsAvoided = scoreDto.MeetingsAvoided,
            CoffeeCollected = scoreDto.CoffeeCollected,
            WeekendsCollected = scoreDto.WeekendsCollected,
            PlayedAt = DateTime.UtcNow
        };

        _context.BugChaseScores.Add(newScore);

        // Check if this is a new best score
        bool isNewBestScore = scoreDto.Score > userStats.BestScore;

        // Update user stats
        userStats.TotalGamesPlayed++;
        userStats.TotalDistance += scoreDto.Distance;
        userStats.TotalSurvivalTime += scoreDto.SurvivalTime;
        userStats.TotalBugsAvoided += scoreDto.BugsAvoided;
        userStats.TotalDeadlinesAvoided += scoreDto.DeadlinesAvoided;
        userStats.TotalMeetingsAvoided += scoreDto.MeetingsAvoided;
        userStats.TotalCoffeeCollected += scoreDto.CoffeeCollected;
        userStats.TotalWeekendsCollected += scoreDto.WeekendsCollected;
        userStats.UpdatedAt = DateTime.UtcNow;

        if (isNewBestScore)
        {
            userStats.BestScore = scoreDto.Score;
        }

        await _context.SaveChangesAsync();

        // Get rank for this score
        var rank = await GetScoreRank(scoreDto.Score);

        return new BugChaseGameResultDto
        {
            Id = newScore.Id,
            Score = newScore.Score,
            Distance = newScore.Distance,
            SurvivalTime = newScore.SurvivalTime,
            BugsAvoided = newScore.BugsAvoided,
            DeadlinesAvoided = newScore.DeadlinesAvoided,
            MeetingsAvoided = newScore.MeetingsAvoided,
            CoffeeCollected = newScore.CoffeeCollected,
            WeekendsCollected = newScore.WeekendsCollected,
            PlayedAt = newScore.PlayedAt,
            IsNewBestScore = isNewBestScore,
            Rank = rank
        };
    }

    public async Task<BugChaseStatsDto?> GetUserStatsAsync(string username)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null) return null;

        var userStats = await GetOrCreateUserStats(user.Id);

        return new BugChaseStatsDto
        {
            BestScore = userStats.BestScore,
            TotalGamesPlayed = userStats.TotalGamesPlayed,
            TotalDistance = userStats.TotalDistance,
            TotalSurvivalTime = userStats.TotalSurvivalTime,
            TotalBugsAvoided = userStats.TotalBugsAvoided,
            TotalDeadlinesAvoided = userStats.TotalDeadlinesAvoided,
            TotalMeetingsAvoided = userStats.TotalMeetingsAvoided,
            TotalCoffeeCollected = userStats.TotalCoffeeCollected,
            TotalWeekendsCollected = userStats.TotalWeekendsCollected,
            AverageScore = userStats.AverageScore,
            AverageSurvivalTime = userStats.AverageSurvivalTime
        };
    }

    public async Task<List<BugChaseLeaderboardEntryDto>> GetLeaderboardAsync(int limit = 5)
    {
        var topScores = await _context.BugChaseScores
            .Include(s => s.User)
            .OrderByDescending(s => s.Score)
            .ThenByDescending(s => s.Distance)
            .ThenBy(s => s.PlayedAt)
            .Take(limit)
            .ToListAsync();

        var leaderboard = new List<BugChaseLeaderboardEntryDto>();

        for (int i = 0; i < topScores.Count; i++)
        {
            var score = topScores[i];
            leaderboard.Add(new BugChaseLeaderboardEntryDto
            {
                Rank = i + 1,
                Username = score.User.Username,
                FirstName = score.User.FirstName,
                LastName = score.User.LastName,
                TechStack = score.User.TechStack,
                ZodiacSign = score.User.ZodiacSign,
                Score = score.Score,
                Distance = score.Distance,
                SurvivalTime = score.SurvivalTime,
                PlayedAt = score.PlayedAt
            });
        }

        return leaderboard;
    }

    public async Task<List<BugChaseGameResultDto>> GetRecentGamesAsync(string username, int limit = 10)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null) return new List<BugChaseGameResultDto>();

        var recentScores = await _context.BugChaseScores
            .Where(s => s.UserId == user.Id)
            .OrderByDescending(s => s.PlayedAt)
            .Take(limit)
            .ToListAsync();

        var recentGames = new List<BugChaseGameResultDto>();
        var userStats = await GetOrCreateUserStats(user.Id);

        foreach (var score in recentScores)
        {
            var rank = await GetScoreRank(score.Score);

            recentGames.Add(new BugChaseGameResultDto
            {
                Id = score.Id,
                Score = score.Score,
                Distance = score.Distance,
                SurvivalTime = score.SurvivalTime,
                BugsAvoided = score.BugsAvoided,
                DeadlinesAvoided = score.DeadlinesAvoided,
                MeetingsAvoided = score.MeetingsAvoided,
                CoffeeCollected = score.CoffeeCollected,
                WeekendsCollected = score.WeekendsCollected,
                PlayedAt = score.PlayedAt,
                IsNewBestScore = score.Score == userStats.BestScore,
                Rank = rank
            });
        }

        return recentGames;
    }

    public async Task<BugChaseDashboardDto?> GetDashboardAsync(string username)
    {
        var userStats = await GetUserStatsAsync(username);
        if (userStats == null) return null;

        var leaderboard = await GetLeaderboardAsync(5);
        var recentGames = await GetRecentGamesAsync(username, 5);

        return new BugChaseDashboardDto
        {
            UserStats = userStats,
            TopScores = leaderboard,
            RecentGames = recentGames
        };
    }

    public async Task InitializeUserStatsAsync(int userId)
    {
        var existingStats = await _context.BugChaseStats.FirstOrDefaultAsync(s => s.UserId == userId);
        if (existingStats == null)
        {
            var newStats = new BugChaseStats { UserId = userId };
            _context.BugChaseStats.Add(newStats);
            await _context.SaveChangesAsync();
        }
    }

    private async Task<BugChaseStats> GetOrCreateUserStats(int userId)
    {
        var userStats = await _context.BugChaseStats.FirstOrDefaultAsync(s => s.UserId == userId);
        if (userStats == null)
        {
            userStats = new BugChaseStats { UserId = userId };
            _context.BugChaseStats.Add(userStats);
            await _context.SaveChangesAsync();
        }
        return userStats;
    }

    private async Task<int> GetScoreRank(int score)
    {
        var betterScoresCount = await _context.BugChaseScores
            .Where(s => s.Score > score)
            .Select(s => s.Score)
            .Distinct()
            .CountAsync();

        return betterScoresCount + 1;
    }
}