namespace backend_api.Models;

public class UserModel
{
    public int Id { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public bool IsMfaEnabled { get; set; }
}

public class LoginRequest
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}

public class LoginResponse
{
    public bool Success { get; set; }
    public string? AccessToken { get; set; }
    public required string Message { get; set; }
    public UserInfo? UserInfo { get; set; }
}

public class UserInfo
{
    public required string Id { get; set; }
    public required string DisplayName { get; set; }
    public required string Email { get; set; }
    public required string UserPrincipalName { get; set; }
}