using DevLife.Enums;
using System.ComponentModel.DataAnnotations;

namespace DevLife.Dtos;

public class DatingSetupRequest
{
    [Required]
    public Gender Gender { get; set; }

    [Required]
    public Gender Preference { get; set; }

    [Required]
    [StringLength(500, MinimumLength = 10)]
    public string Bio { get; set; } = string.Empty;
}
