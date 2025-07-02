using System.ComponentModel.DataAnnotations;

namespace DevLife.Models.DevDating;

public class SwipeAction
{
    public int Id { get; set; }

    [Required]
    public int SwiperId { get; set; }
    public User Swiper { get; set; } = null!;

    [Required]
    public int SwipedUserId { get; set; }
    public User SwipedUser { get; set; } = null!;

    [Required]
    public bool IsLike { get; set; }

    public DateTime SwipedAt { get; set; } = DateTime.UtcNow;
}
