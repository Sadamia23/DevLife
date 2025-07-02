using System.ComponentModel.DataAnnotations;

namespace DevLife.Dtos;

public class SendMessageRequest
{
    [Required]
    [StringLength(1000, MinimumLength = 1)]
    public string Message { get; set; } = string.Empty;
}
