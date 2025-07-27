namespace backend_api.Services;

public interface IRateLimitService
{
    Task<bool> IsAllowedAsync(string clientId, string action, int maxRequests = 5, TimeSpan? window = null);
    Task<RateLimitInfo> GetRateLimitInfoAsync(string clientId, string action);
}
