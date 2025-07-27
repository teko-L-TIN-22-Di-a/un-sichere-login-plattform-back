using Microsoft.Graph;
using backend_api.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace backend_api.Services;

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
            var tenantId = _configuration["AzureAd:TenantId"];
            var clientId = _configuration["AzureAd:ClientId"];
            var clientSecret = _configuration["AzureAd:ClientSecret"];
            var scope = _configuration["AzureAd:Scope"];

            if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                return new LoginResponse(
                    Success: false,
                    Message: "Azure AD configuration is missing"
                );
            }

            var tokenEndpoint = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";

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
            var tokenResponse = await _httpClient.PostAsync(tokenEndpoint, tokenRequestContent);
            var tokenResponseContent = await tokenResponse.Content.ReadAsStringAsync();

            if (!tokenResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning("Token request failed: {StatusCode} - {Content}", tokenResponse.StatusCode, tokenResponseContent);
                
                var errorResponse = JsonSerializer.Deserialize<TokenErrorResponse>(tokenResponseContent);
                return new LoginResponse(
                    Success: false,
                    Message: errorResponse?.error_description ?? "Authentication failed"
                );
            }

            var tokenData = JsonSerializer.Deserialize<TokenResponse>(tokenResponseContent);
            if (tokenData?.access_token == null)
            {
                return new LoginResponse(
                    Success: false,
                    Message: "Invalid token response"
                );
            }

            var userInfo = await GetUserInfoAsync(tokenData.access_token);

            return new LoginResponse(
                Success: true,
                Message: "Authentication successful",
                AccessToken: tokenData.access_token,
                UserInfo: userInfo
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during authentication");
            return new LoginResponse(
                Success: false,
                Message: "An error occurred during authentication"
            );
        }
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var tenantId = _configuration["AzureAd:TenantId"];
            var clientId = _configuration["AzureAd:ClientId"];
            var clientSecret = _configuration["AzureAd:ClientSecret"];

            if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                return new RegisterResponse(
                    Success: false,
                    Message: "Azure AD configuration is missing"
                );
            }

            var appToken = await GetApplicationTokenAsync();
            if (string.IsNullOrEmpty(appToken))
            {
                return new RegisterResponse(
                    Success: false,
                    Message: "Failed to get application token"
                );
            }

            var userId = await CreateUserInAzureAdAsync(appToken, request);
            if (string.IsNullOrEmpty(userId))
            {
                return new RegisterResponse(
                    Success: false,
                    Message: "Failed to create user in Azure AD"
                );
            }

            var userInfo = await GetUserInfoByIdAsync(appToken, userId);

            return new RegisterResponse(
                Success: true,
                Message: "User registered successfully. Please check your email to verify your account.",
                UserId: userId,
                UserInfo: userInfo
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration");
            return new RegisterResponse(
                Success: false,
                Message: "An error occurred during registration"
            );
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

            return new UserInfo(
                Id: userData.id ?? "",
                Username: userData.userPrincipalName ?? "",
                Email: userData.mail ?? userData.userPrincipalName ?? "",
                Name: userData.displayName ?? "",
                GivenName: userData.givenName ?? "",
                FamilyName: userData.surname ?? ""
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user info");
            return null;
        }
    }

    private async Task<string?> GetApplicationTokenAsync()
    {
        try
        {
            var tenantId = _configuration["AzureAd:TenantId"];
            var clientId = _configuration["AzureAd:ClientId"];
            var clientSecret = _configuration["AzureAd:ClientSecret"];

            var tokenEndpoint = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";

            var tokenRequest = new List<KeyValuePair<string, string>>
            {
                new("grant_type", "client_credentials"),
                new("client_id", clientId ?? ""),
                new("client_secret", clientSecret ?? ""),
                new("scope", "https://graph.microsoft.com/.default")
            };

            var tokenRequestContent = new FormUrlEncodedContent(tokenRequest);
            var tokenResponse = await _httpClient.PostAsync(tokenEndpoint, tokenRequestContent);
            var tokenResponseContent = await tokenResponse.Content.ReadAsStringAsync();

            if (!tokenResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning("Application token request failed: {StatusCode} - {Content}", 
                    tokenResponse.StatusCode, tokenResponseContent);
                return null;
            }

            var tokenData = JsonSerializer.Deserialize<TokenResponse>(tokenResponseContent);
            return tokenData?.access_token;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting application token");
            return null;
        }
    }

    private async Task<string?> CreateUserInAzureAdAsync(string appToken, RegisterRequest request)
    {
        try
        {
            var createUserEndpoint = "https://graph.microsoft.com/v1.0/users";
            
            var domain = _configuration["AzureAd:Domain"] ?? "yourdomain.onmicrosoft.com";
            var userPrincipalName = $"{request.Username}@{domain}";

            var userPayload = new
            {
                accountEnabled = true,
                displayName = request.DisplayName,
                mailNickname = request.Username,
                userPrincipalName = userPrincipalName,
                givenName = request.FirstName ?? request.DisplayName.Split(' ').FirstOrDefault() ?? request.Username,
                surname = request.LastName ?? request.DisplayName.Split(' ').LastOrDefault() ?? "",
                mail = request.Email,
                passwordProfile = new
                {
                    forceChangePasswordNextSignIn = true,
                    password = request.Password
                }
            };

            var json = JsonSerializer.Serialize(userPayload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", appToken);
            
            var response = await _httpClient.PostAsync(createUserEndpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("User creation failed: {StatusCode} - {Content}", 
                    response.StatusCode, responseContent);
                return null;
            }

            var userData = JsonSerializer.Deserialize<JsonElement>(responseContent);
            return userData.GetProperty("id").GetString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user in Azure AD");
            return null;
        }
    }

    private async Task<UserInfo?> GetUserInfoByIdAsync(string appToken, string userId)
    {
        try
        {
            var userEndpoint = $"https://graph.microsoft.com/v1.0/users/{userId}";
            
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", appToken);
            
            var response = await _httpClient.GetAsync(userEndpoint);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get user info: {StatusCode}", response.StatusCode);
                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var userData = JsonSerializer.Deserialize<GraphUserResponse>(responseContent);
            
            if (userData == null) return null;

            return new UserInfo(
                Id: userData.id ?? "",
                Username: userData.userPrincipalName ?? "",
                Email: userData.mail ?? userData.userPrincipalName ?? "",
                Name: userData.displayName ?? "",
                GivenName: userData.givenName ?? "",
                FamilyName: userData.surname ?? ""
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user info by ID");
            return null;
        }
    }
}
