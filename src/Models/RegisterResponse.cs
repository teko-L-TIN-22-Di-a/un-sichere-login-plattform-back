namespace backend_api.Models;

public record RegisterResponse(
    bool Success,
    string Message,
    string? UserId = null,
    UserInfo? UserInfo = null,
    bool RequiresEmailVerification = true
);
