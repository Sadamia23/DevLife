using System.ComponentModel.DataAnnotations;

namespace DevLife.Models.MeetingExcuse;

public class MeetingExcuseFavorite
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public int MeetingExcuseId { get; set; }

    [StringLength(200)]
    public string? CustomName { get; set; }

    [Range(1, 5)]
    public int? UserRating { get; set; }

    public DateTime SavedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public MeetingExcuse MeetingExcuse { get; set; } = null!;
}
