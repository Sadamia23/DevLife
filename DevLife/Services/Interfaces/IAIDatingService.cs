namespace DevLife.Services.Interfaces;

public interface IAIDatingService
{
    Task<string> GenerateResponseAsync(string userMessage, string context, string personality);
    Task<string> GetPersonalityForUserAsync(int userId);
}
