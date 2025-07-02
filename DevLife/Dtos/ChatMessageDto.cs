namespace DevLife.Dtos;

public class ChatMessageDto
{
    public int Id { get; set; }
    public int SenderId { get; set; }
    public string SenderUsername { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsAIGenerated { get; set; }
    public DateTime SentAt { get; set; }
}
