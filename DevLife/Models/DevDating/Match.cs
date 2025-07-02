using System.ComponentModel.DataAnnotations;

namespace DevLife.Models.DevDating;

public class Match
{
    public int Id { get; set; }

    [Required]
    public int User1Id { get; set; }
    public User User1 { get; set; } = null!;

    [Required]
    public int User2Id { get; set; }
    public User User2 { get; set; } = null!;

    public DateTime MatchedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    public ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
}
