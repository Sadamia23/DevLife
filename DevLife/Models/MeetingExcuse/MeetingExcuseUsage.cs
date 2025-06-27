using System.ComponentModel.DataAnnotations;

namespace DevLife.Models.MeetingExcuse;

public class MeetingExcuseUsage
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public int MeetingExcuseId { get; set; }

    /// <summary>
    /// Where/how the excuse was used (optional context)
    /// </summary>
    [StringLength(500)]
    public string? Context { get; set; }

    /// <summary>
    /// Did the excuse work? (for fun stats)
    /// </summary>
    public bool? WasSuccessful { get; set; }

    public DateTime UsedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public MeetingExcuse MeetingExcuse { get; set; } = null!;
}
