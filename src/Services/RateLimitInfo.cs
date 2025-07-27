namespace backend_api.Services;

public record RateLimitInfo(
    int RequestsRemaining,
    DateTime WindowResetTime,
    bool IsAllowed
);
