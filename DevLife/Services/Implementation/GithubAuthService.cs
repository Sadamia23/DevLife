using DevLife.Services.Interfaces;

namespace DevLife.Services.Implementation;

public class GitHubAuthService : IGitHubAuthService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly string _clientId;
    private readonly string _clientSecret;

    public GitHubAuthService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _clientId = _configuration["GitHub:ClientId"] ?? throw new InvalidOperationException("GitHub ClientId not configured");
        _clientSecret = _configuration["GitHub:ClientSecret"] ?? throw new InvalidOperationException("GitHub ClientSecret not configured");
    }

    public string GetAuthorizationUrl(string state)
    {
        var scopes = "read:user,repo"; // Adjust scopes as needed
        return $"https://github.com/login/oauth/authorize?client_id={_clientId}&scope={scopes}&state={state}&redirect_uri={GetRedirectUri()}";
    }

    public async Task<string> ExchangeCodeForTokenAsync(string code, string state)
    {
        var tokenRequest = new
        {
            client_id = _clientId,
            client_secret = _clientSecret,
            code = code,
            redirect_uri = GetRedirectUri()
        };

        var response = await _httpClient.PostAsJsonAsync("https://github.com/login/oauth/access_token", tokenRequest);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();

        // Parse the response (GitHub returns form-encoded data)
        var tokenData = ParseTokenResponse(responseContent);

        if (tokenData.ContainsKey("access_token"))
        {
            return tokenData["access_token"];
        }

        throw new InvalidOperationException("Failed to obtain access token from GitHub");
    }

    public async Task<bool> ValidateTokenAsync(string accessToken)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "DevLife-App");

            var response = await _httpClient.GetAsync("https://api.github.com/user");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> GetUserInfoAsync(string accessToken)
    {
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "DevLife-App");

        var response = await _httpClient.GetAsync("https://api.github.com/user");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }

    private string GetRedirectUri()
    {
        return _configuration["GitHub:RedirectUri"] ?? "https://localhost:7276/api/github/callback";
    }

    private Dictionary<string, string> ParseTokenResponse(string response)
    {
        var result = new Dictionary<string, string>();
        var pairs = response.Split('&');

        foreach (var pair in pairs)
        {
            var keyValue = pair.Split('=');
            if (keyValue.Length == 2)
            {
                result[keyValue[0]] = Uri.UnescapeDataString(keyValue[1]);
            }
        }

        return result;
    }
}
