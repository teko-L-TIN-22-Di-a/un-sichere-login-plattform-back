using Microsoft.AspNetCore.Mvc;

namespace backend_api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;

    public HealthController(ILogger<HealthController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Health check endpoint to verify API status
    /// </summary>
    /// <returns>API health status</returns>
    [HttpGet]
    [ProducesResponseType(200)]
    public ActionResult GetHealth()
    {
        return Ok(new 
        { 
            status = "healthy", 
            timestamp = DateTime.UtcNow,
            service = "backend-api",
            version = "1.0.0"
        });
    }

    /// <summary>
    /// Detailed health check with system information
    /// </summary>
    /// <returns>Detailed health status</returns>
    [HttpGet("detailed")]
    [ProducesResponseType(200)]
    public ActionResult GetDetailedHealth()
    {
        var configuration = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        
        // Check Azure AD configuration
        var tenantId = configuration["AzureAd:TenantId"];
        var clientId = configuration["AzureAd:ClientId"];
        var clientSecret = configuration["AzureAd:ClientSecret"];
        var scope = configuration["AzureAd:Scope"];
        var domain = configuration["AzureAd:Domain"];
        
        var azureAdConfigStatus = "healthy";
        var azureAdDetails = new Dictionary<string, object>();
        
        if (string.IsNullOrEmpty(tenantId))
        {
            azureAdConfigStatus = "unhealthy";
            azureAdDetails["tenantId"] = "missing";
        }
        else
        {
            azureAdDetails["tenantId"] = "configured";
        }
        
        if (string.IsNullOrEmpty(clientId))
        {
            azureAdConfigStatus = "unhealthy";
            azureAdDetails["clientId"] = "missing";
        }
        else
        {
            azureAdDetails["clientId"] = "configured";
        }
        
        if (string.IsNullOrEmpty(clientSecret))
        {
            azureAdConfigStatus = "unhealthy";
            azureAdDetails["clientSecret"] = "missing";
        }
        else
        {
            azureAdDetails["clientSecret"] = "configured";
        }
        
        if (string.IsNullOrEmpty(scope))
        {
            azureAdDetails["scope"] = "missing (using default)";
        }
        else
        {
            azureAdDetails["scope"] = "configured";
        }
        
        if (string.IsNullOrEmpty(domain))
        {
            azureAdDetails["domain"] = "missing (using default)";
        }
        else
        {
            azureAdDetails["domain"] = "configured";
        }
        
        // Check JWT Bearer configuration
        var jwtAuthority = configuration["Authentication:JwtBearer:Authority"];
        var jwtAudience = configuration["Authentication:JwtBearer:Audience"];
        
        var jwtConfigStatus = "healthy";
        var jwtDetails = new Dictionary<string, object>();
        
        if (string.IsNullOrEmpty(jwtAuthority))
        {
            jwtConfigStatus = "unhealthy";
            jwtDetails["authority"] = "missing";
        }
        else
        {
            jwtDetails["authority"] = "configured";
        }
        
        if (string.IsNullOrEmpty(jwtAudience))
        {
            jwtConfigStatus = "unhealthy";
            jwtDetails["audience"] = "missing";
        }
        else
        {
            jwtDetails["audience"] = "configured";
        }
        
        var overallStatus = (azureAdConfigStatus == "healthy" && jwtConfigStatus == "healthy") ? "healthy" : "unhealthy";
        
        var response = new
        {
            status = overallStatus,
            timestamp = DateTime.UtcNow,
            service = "backend-api",
            version = "1.0.0",
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development",
            uptime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            checks = new
            {
                azureAd = new
                {
                    status = azureAdConfigStatus,
                    details = azureAdDetails
                },
                jwtBearer = new
                {
                    status = jwtConfigStatus,
                    details = jwtDetails
                },
                api = new
                {
                    status = "healthy",
                    details = new { message = "API is responding" }
                }
            }
        };

        return Ok(response);
    }
}
