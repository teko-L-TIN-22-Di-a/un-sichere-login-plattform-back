using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using backend_api.Services;

namespace backend_api.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RateLimitAttribute : ActionFilterAttribute
{
    private readonly int _maxRequests;
    private readonly TimeSpan _window;
    private readonly string _action;

    public RateLimitAttribute(int maxRequests = 5, int windowHours = 1, string action = "")
    {
        _maxRequests = maxRequests;
        _window = TimeSpan.FromHours(windowHours);
        _action = action;
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var rateLimitService = context.HttpContext.RequestServices.GetRequiredService<IRateLimitService>();
        
        // Get client IP
        var clientIp = GetClientIpAddress(context.HttpContext);
        
        // Use action name if not specified
        var actionName = !string.IsNullOrEmpty(_action) ? _action : 
            $"{context.ActionDescriptor.RouteValues["controller"]}_{context.ActionDescriptor.RouteValues["action"]}";
        
        var isAllowed = await rateLimitService.IsAllowedAsync(clientIp, actionName, _maxRequests, _window);
        
        if (!isAllowed)
        {
            var rateLimitInfo = await rateLimitService.GetRateLimitInfoAsync(clientIp, actionName);
            
            context.Result = new ObjectResult(new
            {
                success = false,
                message = $"Rate limit exceeded. Maximum {_maxRequests} requests per hour allowed.",
                retryAfter = rateLimitInfo.WindowResetTime,
                requestsRemaining = rateLimitInfo.RequestsRemaining
            })
            {
                StatusCode = 429 // Too Many Requests
            };
            
            // Add rate limit headers
            context.HttpContext.Response.Headers["X-RateLimit-Limit"] = _maxRequests.ToString();
            context.HttpContext.Response.Headers["X-RateLimit-Remaining"] = rateLimitInfo.RequestsRemaining.ToString();
            context.HttpContext.Response.Headers["X-RateLimit-Reset"] = ((DateTimeOffset)rateLimitInfo.WindowResetTime).ToUnixTimeSeconds().ToString();
            
            return;
        }

        await next();
    }

    private static string GetClientIpAddress(HttpContext context)
    {
        // Check for forwarded IP first (for load balancers/proxies)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        // Fall back to remote IP
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
