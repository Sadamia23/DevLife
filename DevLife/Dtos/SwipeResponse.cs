namespace DevLife.Dtos;

public class SwipeResponse
{
    public bool IsMatch { get; set; }
    public int? MatchId { get; set; }
    public string Message { get; set; } = string.Empty;
}
