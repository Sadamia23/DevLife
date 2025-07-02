namespace DevLife.Dtos;

public class PotentialMatchDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string TechStack { get; set; } = string.Empty;
    public string Experience { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string ZodiacSign { get; set; } = string.Empty;
}
