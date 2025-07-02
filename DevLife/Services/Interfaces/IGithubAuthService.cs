namespace DevLife.Services.Interfaces;

public interface IGitHubAuthService
{
    string GetAuthorizationUrl(string state);
    Task<string> ExchangeCodeForTokenAsync(string code, string state);
    Task<bool> ValidateTokenAsync(string accessToken);
    Task<string> GetUserInfoAsync(string accessToken);
}
