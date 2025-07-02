using DevLife.Database;
using DevLife.Enums;
using DevLife.Models;
using DevLife.Models.DevDating;
using Microsoft.EntityFrameworkCore;

namespace DevLife.Extensions;

public class DatingDataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DatingDataSeeder> _logger;

    public DatingDataSeeder(ApplicationDbContext context, ILogger<DatingDataSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedDatingProfilesAsync()
    {
        try
        {
            _logger.LogInformation("🔍 Checking existing dating profiles...");

            // Check if we already have dating profiles
            var existingProfilesCount = await _context.DatingProfiles.CountAsync();
            if (existingProfilesCount > 0)
            {
                _logger.LogInformation("✅ Dating profiles already exist ({Count}), skipping seeding", existingProfilesCount);
                return;
            }

            _logger.LogInformation("📝 Starting to seed sample users and dating profiles...");

            // Create sample users with explicit data
            var sampleUsers = new List<User>
            {
                new User
                {
                    Username = "alexcode",
                    FirstName = "Alex",
                    LastName = "Johnson",
                    DateOfBirth = new DateTime(1995, 3, 15),
                    TechStack = TechnologyStack.JavaScript,
                    Experience = ExperienceLevel.Senior,
                    ZodiacSign = ZodiacSign.Pisces,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Username = "sarahtech",
                    FirstName = "Sarah",
                    LastName = "Wilson",
                    DateOfBirth = new DateTime(1992, 8, 22),
                    TechStack = TechnologyStack.Python,
                    Experience = ExperienceLevel.Senior,
                    ZodiacSign = ZodiacSign.Virgo,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Username = "mikecoder",
                    FirstName = "Mike",
                    LastName = "Chen",
                    DateOfBirth = new DateTime(1998, 1, 10),
                    TechStack = TechnologyStack.Java,
                    Experience = ExperienceLevel.Junior,
                    ZodiacSign = ZodiacSign.Capricorn,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Username = "emilydev",
                    FirstName = "Emily",
                    LastName = "Rodriguez",
                    DateOfBirth = new DateTime(1994, 11, 5),
                    TechStack = TechnologyStack.Angular,
                    Experience = ExperienceLevel.Middle,
                    ZodiacSign = ZodiacSign.Scorpio,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Username = "davidjs",
                    FirstName = "David",
                    LastName = "Kim",
                    DateOfBirth = new DateTime(1990, 6, 18),
                    TechStack = TechnologyStack.JavaScript,
                    Experience = ExperienceLevel.Senior,
                    ZodiacSign = ZodiacSign.Gemini,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Username = "jennypy",
                    FirstName = "Jenny",
                    LastName = "Lee",
                    DateOfBirth = new DateTime(1996, 4, 25),
                    TechStack = TechnologyStack.Python,
                    Experience = ExperienceLevel.Middle,
                    ZodiacSign = ZodiacSign.Taurus,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Username = "tomjava",
                    FirstName = "Tom",
                    LastName = "Anderson",
                    DateOfBirth = new DateTime(1993, 9, 12),
                    TechStack = TechnologyStack.Java,
                    Experience = ExperienceLevel.Senior,
                    ZodiacSign = ZodiacSign.Virgo,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Username = "amytech",
                    FirstName = "Amy",
                    LastName = "Thompson",
                    DateOfBirth = new DateTime(1997, 2, 8),
                    TechStack = TechnologyStack.React,
                    Experience = ExperienceLevel.Junior,
                    ZodiacSign = ZodiacSign.Aquarius,
                    CreatedAt = DateTime.UtcNow
                }
            };

            // Check for existing usernames to avoid conflicts
            var existingUsernames = await _context.Users
                .Where(u => sampleUsers.Select(su => su.Username).Contains(u.Username))
                .Select(u => u.Username)
                .ToListAsync();

            if (existingUsernames.Any())
            {
                _logger.LogInformation("⚠️ Some sample usernames already exist: {Usernames}", string.Join(", ", existingUsernames));
                sampleUsers = sampleUsers.Where(u => !existingUsernames.Contains(u.Username)).ToList();
            }

            if (!sampleUsers.Any())
            {
                _logger.LogInformation("✅ All sample users already exist, skipping user creation");
                return;
            }

            // Add users to database and save to get IDs
            _context.Users.AddRange(sampleUsers);
            await _context.SaveChangesAsync();

            _logger.LogInformation("✅ Added {Count} sample users", sampleUsers.Count);

            // Create dating profiles for each user
            var datingProfiles = new List<DatingProfile>
            {
                new DatingProfile
                {
                    UserId = sampleUsers[0].Id, // Alex - Male
                    Gender = Gender.Male,
                    Preference = Gender.Female,
                    Bio = "Full-stack developer who loves building cool apps! When I'm not coding, you'll find me hiking or trying out new coffee shops. Always up for a good tech debate! ☕️💻",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new DatingProfile
                {
                    UserId = sampleUsers[1].Id, // Sarah - Female
                    Gender = Gender.Female,
                    Preference = Gender.Male,
                    Bio = "Python enthusiast and data science geek! I enjoy solving complex problems and building ML models. Looking for someone who shares my passion for tech and good conversations. 🐍📊",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new DatingProfile
                {
                    UserId = sampleUsers[2].Id, // Mike - Male
                    Gender = Gender.Male,
                    Preference = Gender.Female,
                    Bio = "Junior Java developer on a mission to learn everything! I love clean code, video games, and pizza nights. Seeking someone patient with my coding jokes! 🎮🍕",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new DatingProfile
                {
                    UserId = sampleUsers[3].Id, // Emily - Female
                    Gender = Gender.Female,
                    Preference = Gender.Male,
                    Bio = "C# developer who thinks strongly typed languages are the best! I'm into rock climbing and building robust applications. Let's debug life together! 🧗‍♀️⚡",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new DatingProfile
                {
                    UserId = sampleUsers[4].Id, // David - Male
                    Gender = Gender.Male,
                    Preference = Gender.Female,
                    Bio = "Senior JavaScript wizard who can make browsers do magical things! I love mentoring, traveling, and discovering new frameworks. Swipe right if you appreciate clean code! ✨🌍",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new DatingProfile
                {
                    UserId = sampleUsers[5].Id, // Jenny - Female
                    Gender = Gender.Female,
                    Preference = Gender.Male,
                    Bio = "Python developer with a love for automation and AI. I spend my free time contributing to open source and experimenting with new technologies. Let's build something amazing together! 🤖❤️",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new DatingProfile
                {
                    UserId = sampleUsers[6].Id, // Tom - Male
                    Gender = Gender.Male,
                    Preference = Gender.Female,
                    Bio = "Experienced Java developer who believes in the power of good architecture. I enjoy teaching, reading tech blogs, and weekend hackathons. Looking for my coding partner in crime! 📚💡",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new DatingProfile
                {
                    UserId = sampleUsers[7].Id, // Amy - Female
                    Gender = Gender.Female,
                    Preference = Gender.Male,
                    Bio = "Junior .NET developer eager to learn and grow! I love pair programming, tech meetups, and discovering new libraries. Seeking someone who can help me level up my skills! 🚀👩‍💻",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            // Add dating profiles and save
            _context.DatingProfiles.AddRange(datingProfiles);
            await _context.SaveChangesAsync();

            _logger.LogInformation("✅ Successfully seeded {UserCount} users and {ProfileCount} dating profiles",
                sampleUsers.Count, datingProfiles.Count);

            // Log some sample data for verification
            var totalUsers = await _context.Users.CountAsync();
            var totalProfiles = await _context.DatingProfiles.CountAsync();
            var activeProfiles = await _context.DatingProfiles.CountAsync(p => p.IsActive);

            _logger.LogInformation("📊 Database Summary:");
            _logger.LogInformation("   Total Users: {TotalUsers}", totalUsers);
            _logger.LogInformation("   Total Dating Profiles: {TotalProfiles}", totalProfiles);
            _logger.LogInformation("   Active Dating Profiles: {ActiveProfiles}", activeProfiles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error seeding dating profiles");
            throw;
        }
    }
}
