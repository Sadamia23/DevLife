using DevLife.Enums;
using System.ComponentModel.DataAnnotations;

namespace DevLife.Models.MeetingExcuse;

public class MeetingExcuse
{
    public int Id { get; set; }

    [Required]
    [StringLength(500)]
    public string ExcuseText { get; set; } = string.Empty;

    [Required]
    public MeetingCategory Category { get; set; }

    [Required]
    public ExcuseType Type { get; set; }

    /// <summary>
    /// Believability score from 1-10 (1 = obviously fake, 10 = totally believable)
    /// </summary>
    [Range(1, 10)]
    public int BelievabilityScore { get; set; }

    /// <summary>
    /// Tags for filtering (JSON array of strings)
    /// </summary>
    [StringLength(1000)]
    public string TagsJson { get; set; } = "[]";

    /// <summary>
    /// How often this excuse has been used by all users
    /// </summary>
    public int UsageCount { get; set; } = 0;

    /// <summary>
    /// Average rating from users (1-5 stars)
    /// </summary>
    public double AverageRating { get; set; } = 0.0;

    /// <summary>
    /// Number of users who rated this excuse
    /// </summary>
    public int RatingCount { get; set; } = 0;

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<MeetingExcuseFavorite> Favorites { get; set; } = new List<MeetingExcuseFavorite>();
    public ICollection<MeetingExcuseUsage> UsageHistory { get; set; } = new List<MeetingExcuseUsage>();
}
