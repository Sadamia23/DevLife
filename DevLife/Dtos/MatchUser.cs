using DevLife.Enums;

namespace DevLife.Dtos;

public class MatchUser
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public TechnologyStack TechStack { get; set; }
    public ExperienceLevel Experience { get; set; }
    public ZodiacSign ZodiacSign { get; set; }
}

public class MatchDto
{
    public int Id { get; set; }
    public int User1Id { get; set; }
    public int User2Id { get; set; }
    public DateTime MatchedAt { get; set; }
    public bool IsActive { get; set; }
    public MatchUser OtherUser { get; set; } = new();
}
