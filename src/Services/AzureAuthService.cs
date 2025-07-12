using Microsoft.Graph;
using backend_api.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace backend_api.Services;

public interface IAzureAuthService
{
    Task<LoginResponse> AuthenticateAsync(LoginRequest request);
}

public class AzureAuthService : IAzureAuthService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly ILogger<AzureAuthService> _logger;

    public AzureAuthService(IConfiguration configuration, HttpClient httpClient, ILogger<AzureAuthService> logger)
    {
        _configuration = configuration;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<LoginResponse> AuthenticateAsync(LoginRequest request)
    {
        try
        {
            // Azure AD Token-Endpoint für Resource Owner Password Credentials Flow
            var tenantId = _configuration["AzureAd:TenantId"];
            var clientId = _configuration["AzureAd:ClientId"];
            var clientSecret = _configuration["AzureAd:ClientSecret"];
            var scope = _configuration["AzureAd:Scope"];

            if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "Azure AD configuration is missing"
                };
            }

            var tokenEndpoint = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";

            // Prepare token request
            var tokenRequest = new List<KeyValuePair<string, string>>
            {
                new("grant_type", "password"),
                new("client_id", clientId),
                new("client_secret", clientSecret),
                new("scope", scope ?? "https://graph.microsoft.com/.default"),
                new("username", request.Username),
                new("password", request.Password)
            };

            var tokenRequestContent = new FormUrlEncodedContent(tokenRequest);

            // Make token request
            var tokenResponse = await _httpClient.PostAsync(tokenEndpoint, tokenRequestContent);
            var tokenResponseContent = await tokenResponse.Content.ReadAsStringAsync();

            if (!tokenResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning("Token request failed: {StatusCode} - {Content}", tokenResponse.StatusCode, tokenResponseContent);
                
                // Parse error response
                var errorResponse = JsonSerializer.Deserialize<TokenErrorResponse>(tokenResponseContent);
                return new LoginResponse
                {
                    Success = false,
                    Message = errorResponse?.error_description ?? "Authentication failed"
                };
            }

            // Parse token response
            var tokenData = JsonSerializer.Deserialize<TokenResponse>(tokenResponseContent);
            if (tokenData?.access_token == null)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "Invalid token response"
                };
            }

            // Get user info from Microsoft Graph
            var userInfo = await GetUserInfoAsync(tokenData.access_token);

            return new LoginResponse
            {
                Success = true,
                AccessToken = tokenData.access_token,
                Message = "Authentication successful",
                UserInfo = userInfo
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during authentication");
            return new LoginResponse
            {
                Success = false,
                Message = "An error occurred during authentication"
            };
        }
    }

    private async Task<UserInfo?> GetUserInfoAsync(string accessToken)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            
            var userResponse = await _httpClient.GetAsync("https://graph.microsoft.com/v1.0/me");
            if (!userResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get user info: {StatusCode}", userResponse.StatusCode);
                return null;
            }

            var userContent = await userResponse.Content.ReadAsStringAsync();
            var userData = JsonSerializer.Deserialize<GraphUserResponse>(userContent);
            
            if (userData == null) return null;

            return new UserInfo
            {
                Id = userData.id ?? "",
                DisplayName = userData.displayName ?? "",
                Email = userData.mail ?? userData.userPrincipalName ?? "",
                UserPrincipalName = userData.userPrincipalName ?? ""
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user info");
            return null;
        }
    }
}

// DTOs for token response
public class TokenResponse
{
    public string? access_token { get; set; }
    public string? token_type { get; set; }
    public int expires_in { get; set; }
    public string? refresh_token { get; set; }
    public string? scope { get; set; }
}

public class TokenErrorResponse
{
    public string? error { get; set; }
    public string? error_description { get; set; }
    public int[]? error_codes { get; set; }
}

public class GraphUserResponse
{
    public string? id { get; set; }
    public string? displayName { get; set; }
    public string? mail { get; set; }
    public string? userPrincipalName { get; set; }
}
