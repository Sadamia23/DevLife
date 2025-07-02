namespace DevLife.Dtos;

public class SendMessageResponse
{
    public ChatMessageDto UserMessage { get; set; } = null!;
    public ChatMessageDto? AIResponse { get; set; }
}
