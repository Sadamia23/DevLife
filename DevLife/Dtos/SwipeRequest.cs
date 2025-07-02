using System.ComponentModel.DataAnnotations;

namespace DevLife.Dtos;

public class SwipeRequest
{
    [Required]
    public int TargetUserId { get; set; }

    [Required]
    public bool IsLike { get; set; }
}
