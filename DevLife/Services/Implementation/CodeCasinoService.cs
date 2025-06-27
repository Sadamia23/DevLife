using DevLife.Database;
using DevLife.Dtos;
using DevLife.Enums;
using DevLife.Models;
using DevLife.Models.CodeCasino;
using DevLife.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DevLife.Services.Implementation;

    public class CodeCasinoService : ICodeCasinoService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAIChallengeService _aiChallengeService;
        private readonly ILogger<CodeCasinoService> _logger;
        private readonly Random _random = new();

        // Cache for challenges to track correct answers
        private static readonly Dictionary<int, CachedChallenge> _challengeCache = new();
        private static readonly Dictionary<DateTime, AIChallengeResponseDto> _dailyChallengeCache = new();

        public CodeCasinoService(
            ApplicationDbContext context,
            IAIChallengeService aiChallengeService,
            ILogger<CodeCasinoService> logger)
        {
            _context = context;
            _aiChallengeService = aiChallengeService;
            _logger = logger;
        }

        public async Task<CodeChallengeDto?> GetRandomChallengeAsync(string username)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return null;

            try
            {
                // Generate AI challenge based on user's tech stack and experience
                var aiChallenge = await _aiChallengeService.GenerateChallengeAsync(
                    user.TechStack,
                    user.Experience);

                if (aiChallenge != null)
                {
                    return MapAIChallengeToChallengeDto(aiChallenge, false);
                }

                // Fallback to database challenges if AI fails
                _logger.LogWarning("AI challenge generation failed, falling back to database challenges");
                return await GetDatabaseChallengeAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating AI challenge for user {Username}", username);
                return await GetDatabaseChallengeAsync(user);
            }
        }

        public async Task<DailyChallengeDto?> GetDailyChallengeAsync(string username)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return null;

            var today = DateTime.UtcNow.Date;

            try
            {
                // Check cache first
                if (_dailyChallengeCache.TryGetValue(today, out var cachedChallenge))
                {
                    var userStats = await GetOrCreateUserStats(user.Id);
                    var hasPlayed = userStats.LastDailyChallenge?.Date == today;

                    var challengeDto = MapAIChallengeToChallengeDto(cachedChallenge, true, 3);

                    return new DailyChallengeDto
                    {
                        Challenge = challengeDto,
                        ChallengeDate = today,
                        BonusMultiplier = 3,
                        HasPlayed = hasPlayed
                    };
                }

                // Generate new daily challenge
                var aiChallenge = await _aiChallengeService.GenerateDailyChallengeAsync();

                if (aiChallenge != null)
                {
                    // Cache the daily challenge
                    _dailyChallengeCache[today] = aiChallenge;

                    // Clean old cache entries (keep only last 7 days)
                    var oldEntries = _dailyChallengeCache.Keys.Where(d => d < today.AddDays(-7)).ToList();
                    foreach (var oldEntry in oldEntries)
                    {
                        _dailyChallengeCache.Remove(oldEntry);
                    }

                    var userStats = await GetOrCreateUserStats(user.Id);
                    var hasPlayed = userStats.LastDailyChallenge?.Date == today;

                    var challengeDto = MapAIChallengeToChallengeDto(aiChallenge, true, 3);

                    return new DailyChallengeDto
                    {
                        Challenge = challengeDto,
                        ChallengeDate = today,
                        BonusMultiplier = 3,
                        HasPlayed = hasPlayed
                    };
                }

                // Fallback to database daily challenge
                _logger.LogWarning("AI daily challenge generation failed, falling back to database");
                return await GetDatabaseDailyChallengeAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating AI daily challenge");
                return await GetDatabaseDailyChallengeAsync(user);
            }
        }

        public async Task<GameResultDto> PlaceBetAsync(string username, PlaceBetDto betDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) throw new InvalidOperationException("User not found");
        
            var userStats = await GetOrCreateUserStats(user.Id);
        
            // Check if user has enough points
            if (userStats.TotalPoints < betDto.PointsBet)
                throw new InvalidOperationException("Insufficient points");
        
            // Determine if this is a daily challenge based on ID pattern
            var today = DateTime.UtcNow;
            var todayId = int.Parse($"{today.Year}{today.Month:D2}{today.Day:D2}");
            var isDailyChallenge = betDto.ChallengeId == todayId;
        
            if (isDailyChallenge && userStats.LastDailyChallenge?.Date == today.Date)
                throw new InvalidOperationException("Daily challenge already completed today");
        
            // Determine if the choice is correct
            bool isCorrect;
            string explanation = "";
            string correctCode = "";
            string buggyCode = "";
            bool isAIChallenge = false;
        
            // Check if it's an AI challenge (high ID numbers or today's daily challenge)
            if (isDailyChallenge)
            {
                // Daily challenge - check cache
                if (_challengeCache.TryGetValue(betDto.ChallengeId, out var cachedDaily))
                {
                    isCorrect = cachedDaily.CorrectOption == betDto.ChosenOption;
                    explanation = cachedDaily.AIChallenge?.Explanation ?? "";
                    correctCode = cachedDaily.AIChallenge?.CorrectCode ?? "";
                    buggyCode = cachedDaily.AIChallenge?.BuggyCode ?? "";
                    isAIChallenge = true;
                }
                else
                {
                    // Fallback - assume option 1 is correct
                    isCorrect = betDto.ChosenOption == 1;
                }
            }
            else if (betDto.ChallengeId >= 100000) // High ID numbers = AI challenges
            {
                // AI-generated challenge - check cache
                if (_challengeCache.TryGetValue(betDto.ChallengeId, out var cachedChallenge))
                {
                    isCorrect = cachedChallenge.CorrectOption == betDto.ChosenOption;
                    explanation = cachedChallenge.AIChallenge?.Explanation ?? "";
                    correctCode = cachedChallenge.AIChallenge?.CorrectCode ?? "";
                    buggyCode = cachedChallenge.AIChallenge?.BuggyCode ?? "";
                    isAIChallenge = true;
                }
                else
                {
                    // Fallback - assume option 1 is correct
                    isCorrect = betDto.ChosenOption == 1;
                }
            }
            else
            {
                // Database challenge (low ID numbers)
                var challenge = await _context.CodeChallenges.FindAsync(betDto.ChallengeId);
                if (challenge == null) throw new InvalidOperationException("Challenge not found");
        
                // For database challenges, option 1 is always correct (as per original logic)
                isCorrect = betDto.ChosenOption == 1;
                explanation = challenge.Explanation;
                correctCode = challenge.CorrectCode;
                buggyCode = challenge.BuggyCode;
            }
        
            // Calculate luck multiplier based on zodiac sign
            var luckMultiplier = CalculateZodiacLuckMultiplier(user.ZodiacSign);
        
            // Calculate points
            int pointsWon = 0;
            int pointsLost = 0;
            var baseMultiplier = isDailyChallenge ? 3 : 2;
        
            if (isCorrect)
            {
                pointsWon = (int)(betDto.PointsBet * baseMultiplier * luckMultiplier);
                userStats.TotalPoints += pointsWon;
                userStats.CurrentStreak++;
                userStats.TotalGamesWon++;
        
                if (userStats.CurrentStreak > userStats.LongestStreak)
                    userStats.LongestStreak = userStats.CurrentStreak;
            }
            else
            {
                pointsLost = betDto.PointsBet;
                userStats.TotalPoints -= pointsLost;
                userStats.CurrentStreak = 0;
            }
        
            userStats.TotalGamesPlayed++;
        
            if (isDailyChallenge)
                userStats.LastDailyChallenge = DateTime.UtcNow;
        
            userStats.UpdatedAt = DateTime.UtcNow;
        
            // Create game session record
            var gameSession = new UserGameSession
            {
                UserId = user.Id,
                CodeChallengeId = isAIChallenge ? null : betDto.ChallengeId, // Null for AI challenges
                PointsBet = betDto.PointsBet,
                UserChoice = betDto.ChosenOption == 1,
                IsCorrect = isCorrect,
                PointsWon = pointsWon,
                LuckMultiplier = luckMultiplier,
                IsDailyChallenge = isDailyChallenge,
                IsAIGenerated = isAIChallenge
            };
        
            // Store AI challenge data if it's an AI challenge
            if (isAIChallenge && _challengeCache.TryGetValue(betDto.ChallengeId, out var aiChallengeData))
            {
                gameSession.AIChallengeTitle = aiChallengeData.AIChallenge?.Title;
                gameSession.AIChallengeDescription = aiChallengeData.AIChallenge?.Description;
                gameSession.AICorrectCode = aiChallengeData.AIChallenge?.CorrectCode;
                gameSession.AIBuggyCode = aiChallengeData.AIChallenge?.BuggyCode;
                gameSession.AIExplanation = aiChallengeData.AIChallenge?.Explanation;
                gameSession.AITopic = aiChallengeData.AIChallenge?.Topic;
            }
        
            _context.UserGameSessions.Add(gameSession);
            await _context.SaveChangesAsync();
        
            return new GameResultDto
            {
                IsCorrect = isCorrect,
                PointsBet = betDto.PointsBet,
                PointsWon = pointsWon,
                PointsLost = pointsLost,
                NewTotalPoints = userStats.TotalPoints,
                CurrentStreak = userStats.CurrentStreak,
                StreakBroken = !isCorrect && userStats.CurrentStreak == 0,
                LuckMultiplier = luckMultiplier,
                Explanation = explanation,
                CorrectCode = correctCode,
                BuggyCode = buggyCode
            };
        }

        public async Task<UserStatsDto?> GetUserStatsAsync(string username)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return null;

            var userStats = await GetOrCreateUserStats(user.Id);
            var today = DateTime.UtcNow.Date;
            var canPlayDaily = userStats.LastDailyChallenge?.Date != today;

            return new UserStatsDto
            {
                TotalPoints = userStats.TotalPoints,
                CurrentStreak = userStats.CurrentStreak,
                LongestStreak = userStats.LongestStreak,
                TotalGamesPlayed = userStats.TotalGamesPlayed,
                TotalGamesWon = userStats.TotalGamesWon,
                WinRate = userStats.WinRate,
                CanPlayDailyChallenge = canPlayDaily,
                LastDailyChallenge = userStats.LastDailyChallenge
            };
        }

        public async Task<List<LeaderboardEntryDto>> GetLeaderboardAsync(int limit = 10)
        {
            var userStatsData = await _context.UserStats
                .Include(us => us.User)
                .OrderByDescending(us => us.TotalPoints)
                .ThenByDescending(us => us.CurrentStreak)
                .Take(limit)
                .ToListAsync();

            var leaderboard = userStatsData
                .Select((us, index) => new LeaderboardEntryDto
                {
                    Rank = index + 1,
                    Username = us.User.Username,
                    FirstName = us.User.FirstName,
                    LastName = us.User.LastName,
                    TechStack = us.User.TechStack,
                    ZodiacSign = us.User.ZodiacSign,
                    TotalPoints = us.TotalPoints,
                    CurrentStreak = us.CurrentStreak,
                    TotalGamesWon = us.TotalGamesWon,
                    WinRate = us.WinRate
                })
                .ToList();

            return leaderboard;
        }

        public async Task<CasinoStatsResponse?> GetCasinoStatsAsync(string username)
        {
            var userStats = await GetUserStatsAsync(username);
            if (userStats == null) return null;

            var leaderboard = await GetLeaderboardAsync(5);
            var dailyChallenge = await GetDailyChallengeAsync(username);

            return new CasinoStatsResponse
            {
                UserStats = userStats,
                TopPlayers = leaderboard,
                DailyChallenge = dailyChallenge
            };
        }

        public double CalculateZodiacLuckMultiplier(ZodiacSign zodiacSign)
        {
            return zodiacSign switch
            {
                ZodiacSign.Leo => 1.15,
                ZodiacSign.Sagittarius => 1.12,
                ZodiacSign.Aries => 1.10,
                ZodiacSign.Gemini => 1.08,
                ZodiacSign.Libra => 1.08,
                ZodiacSign.Aquarius => 1.05,
                ZodiacSign.Scorpio => 1.05,
                ZodiacSign.Pisces => 1.03,
                ZodiacSign.Cancer => 1.02,
                ZodiacSign.Taurus => 1.00,
                ZodiacSign.Virgo => 0.98,
                ZodiacSign.Capricorn => 0.95,
                _ => 1.00
            };
        }

        public async Task InitializeUserStatsAsync(int userId)
        {
            var existingStats = await _context.UserStats.FirstOrDefaultAsync(us => us.UserId == userId);
            if (existingStats == null)
            {
                var userStats = new UserStats { UserId = userId };
                _context.UserStats.Add(userStats);
                await _context.SaveChangesAsync();
            }
        }

        private async Task<UserStats> GetOrCreateUserStats(int userId)
        {
            var userStats = await _context.UserStats.FirstOrDefaultAsync(us => us.UserId == userId);
            if (userStats == null)
            {
                userStats = new UserStats { UserId = userId };
                _context.UserStats.Add(userStats);
                await _context.SaveChangesAsync();
            }
            return userStats;
        }

        private CodeChallengeDto MapAIChallengeToChallengeDto(AIChallengeResponseDto aiChallenge, bool isDailyChallenge, int bonusMultiplier = 2)
        {
            // Generate appropriate ID based on challenge type
            int challengeId;
        
            if (isDailyChallenge)
            {
                // Use date-based ID for daily challenges (format: YYYYMMDD)
                // This ensures same daily challenge has same ID for all users
                var today = DateTime.UtcNow;
                challengeId = int.Parse($"{today.Year}{today.Month:D2}{today.Day:D2}");
                // Result: 20250626 for June 26, 2025
            }
            else
            {
                // Use high random numbers for regular AI challenges (100000-999999)
                challengeId = _random.Next(100000, 999999);
            }
        
            // Randomize which code appears as option 1 or 2
            var showCorrectFirst = _random.Next(2) == 0;
            var correctOption = showCorrectFirst ? 1 : 2;
        
            // Cache the challenge with correct answer info
            var cachedChallenge = new CachedChallenge
            {
                AIChallenge = aiChallenge,
                CorrectOption = correctOption,
                IsDailyChallenge = isDailyChallenge,
                Date = DateTime.UtcNow
            };
        
            _challengeCache[challengeId] = cachedChallenge;
        
            // Clean old cache entries (keep only last 100 entries)
            if (_challengeCache.Count > 100)
            {
                var oldEntries = _challengeCache.OrderBy(x => x.Value.Date).Take(20).Select(x => x.Key).ToList();
                foreach (var oldEntry in oldEntries)
                {
                    _challengeCache.Remove(oldEntry);
                }
            }
        
            return new CodeChallengeDto
            {
                Id = challengeId,
                Title = aiChallenge.Title,
                Description = aiChallenge.Description,
                TechStack = aiChallenge.TechStack,
                DifficultyLevel = aiChallenge.DifficultyLevel,
                CodeOption1 = showCorrectFirst ? aiChallenge.CorrectCode : aiChallenge.BuggyCode,
                CodeOption2 = showCorrectFirst ? aiChallenge.BuggyCode : aiChallenge.CorrectCode,
                IsDailyChallenge = isDailyChallenge,
                BonusMultiplier = bonusMultiplier
            };
        }

        // Fallback methods for database challenges (if AI fails)
        private async Task<CodeChallengeDto?> GetDatabaseChallengeAsync(User user)
        {
            var challenges = await _context.CodeChallenges
                .Where(c => c.IsActive && c.TechStack == user.TechStack)
                .ToListAsync();

            if (!challenges.Any()) return null;

            var challenge = challenges[_random.Next(challenges.Count)];
            return MapDatabaseChallengeToDto(challenge, false);
        }

        private async Task<DailyChallengeDto?> GetDatabaseDailyChallengeAsync(User user)
        {
            var today = DateTime.UtcNow.Date;
            var dailyChallenge = await _context.DailyChallenges
                .Include(dc => dc.CodeChallenge)
                .FirstOrDefaultAsync(dc => dc.ChallengeDate.Date == today);

            if (dailyChallenge == null)
            {
                var availableChallenges = await _context.CodeChallenges
                    .Where(c => c.IsActive)
                    .ToListAsync();

                if (!availableChallenges.Any()) return null;

                var selectedChallenge = availableChallenges[_random.Next(availableChallenges.Count)];

                dailyChallenge = new DailyChallenge
                {
                    CodeChallengeId = selectedChallenge.Id,
                    ChallengeDate = today,
                    BonusMultiplier = 3
                };

                _context.DailyChallenges.Add(dailyChallenge);
                await _context.SaveChangesAsync();

                dailyChallenge.CodeChallenge = selectedChallenge;
            }

            var userStats = await GetOrCreateUserStats(user.Id);
            var hasPlayed = userStats.LastDailyChallenge?.Date == today;

            var challengeDto = MapDatabaseChallengeToDto(dailyChallenge.CodeChallenge, true, dailyChallenge.BonusMultiplier);

            return new DailyChallengeDto
            {
                Challenge = challengeDto,
                ChallengeDate = dailyChallenge.ChallengeDate,
                BonusMultiplier = dailyChallenge.BonusMultiplier,
                HasPlayed = hasPlayed
            };
        }

        private CodeChallengeDto MapDatabaseChallengeToDto(CodeChallenge challenge, bool isDailyChallenge, int bonusMultiplier = 2)
        {
            var showCorrectFirst = _random.Next(2) == 0;

            return new CodeChallengeDto
            {
                Id = challenge.Id,
                Title = challenge.Title,
                Description = challenge.Description,
                TechStack = challenge.TechStack,
                DifficultyLevel = challenge.DifficultyLevel,
                CodeOption1 = showCorrectFirst ? challenge.CorrectCode : challenge.BuggyCode,
                CodeOption2 = showCorrectFirst ? challenge.BuggyCode : challenge.CorrectCode,
                IsDailyChallenge = isDailyChallenge,
                BonusMultiplier = bonusMultiplier
            };
        }

        // Helper class for caching challenge data
        private class CachedChallenge
        {
            public AIChallengeResponseDto? AIChallenge { get; set; }
            public int CorrectOption { get; set; } // 1 or 2
            public bool IsDailyChallenge { get; set; }
            public DateTime Date { get; set; }
        }
    }
