namespace backend_api.Models;

public record LoginResponse(
    bool Success,
    string Message,
    string? AccessToken = null,
    UserInfo? UserInfo = null
);
