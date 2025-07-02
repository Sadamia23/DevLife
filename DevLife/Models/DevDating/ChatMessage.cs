using System.ComponentModel.DataAnnotations;

namespace DevLife.Models.DevDating;

public class ChatMessage
{
    public int Id { get; set; }

    [Required]
    public int MatchId { get; set; }
    public Match Match { get; set; } = null!;

    [Required]
    public int SenderId { get; set; }
    public User Sender { get; set; } = null!;

    [Required]
    [StringLength(1000)]
    public string Message { get; set; } = string.Empty;

    public bool IsAIGenerated { get; set; } = false;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
