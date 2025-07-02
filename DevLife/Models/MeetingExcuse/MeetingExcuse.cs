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

    [Range(1, 10)]
    public int BelievabilityScore { get; set; }

    [StringLength(1000)]
    public string TagsJson { get; set; } = "[]";

    public int UsageCount { get; set; } = 0;

    public double AverageRating { get; set; } = 0.0;

    public int RatingCount { get; set; } = 0;

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<MeetingExcuseFavorite> Favorites { get; set; } = new List<MeetingExcuseFavorite>();
    public ICollection<MeetingExcuseUsage> UsageHistory { get; set; } = new List<MeetingExcuseUsage>();
}
