using System.ComponentModel.DataAnnotations;

namespace DevLife.Models.MeetingExcuse;

public class MeetingExcuseFavorite
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public int MeetingExcuseId { get; set; }

    /// <summary>
    /// Optional custom name/note for this favorite
    /// </summary>
    [StringLength(200)]
    public string? CustomName { get; set; }

    /// <summary>
    /// User's personal rating for this excuse (1-5 stars)
    /// </summary>
    [Range(1, 5)]
    public int? UserRating { get; set; }

    public DateTime SavedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public MeetingExcuse MeetingExcuse { get; set; } = null!;
}
