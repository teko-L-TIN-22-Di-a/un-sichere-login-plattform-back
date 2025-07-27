using System.Collections.Concurrent;

namespace backend_api.Services;

public class RateLimitService : IRateLimitService
{
    private readonly ConcurrentDictionary<string, RateLimitData> _rateLimits = new();
    private readonly ILogger<RateLimitService> _logger;

    public RateLimitService(ILogger<RateLimitService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> IsAllowedAsync(string clientId, string action, int maxRequests = 5, TimeSpan? window = null)
    {
        var timeWindow = window ?? TimeSpan.FromHours(1);
        var key = $"{clientId}:{action}";
        var now = DateTime.UtcNow;

        var rateLimitData = _rateLimits.AddOrUpdate(key, 
            new RateLimitData(1, now, now.Add(timeWindow)),
            (k, existing) =>
            {
                // Reset window if expired
                if (now >= existing.WindowEnd)
                {
                    return new RateLimitData(1, now, now.Add(timeWindow));
                }
                
                // Increment counter within window
                return existing with { RequestCount = existing.RequestCount + 1 };
            });

        var isAllowed = rateLimitData.RequestCount <= maxRequests;
        
        if (!isAllowed)
        {
            _logger.LogWarning("Rate limit exceeded for {ClientId} on action {Action}. Count: {Count}/{Max}", 
                clientId, action, rateLimitData.RequestCount, maxRequests);
        }

        return await Task.FromResult(isAllowed);
    }

    public async Task<RateLimitInfo> GetRateLimitInfoAsync(string clientId, string action)
    {
        var key = $"{clientId}:{action}";
        var now = DateTime.UtcNow;
        
        if (_rateLimits.TryGetValue(key, out var data))
        {
            if (now >= data.WindowEnd)
            {
                // Window expired, reset
                return new RateLimitInfo(5, now.AddHours(1), true);
            }
            
            var remaining = Math.Max(0, 5 - data.RequestCount);
            return new RateLimitInfo(remaining, data.WindowEnd, remaining > 0);
        }
        
        return await Task.FromResult(new RateLimitInfo(5, now.AddHours(1), true));
    }
}
