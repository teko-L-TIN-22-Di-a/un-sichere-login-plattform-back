using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend_api.Models;
using backend_api.Services;
using backend_api.Attributes;
using System.Security.Claims;

namespace backend_api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAzureAuthService _azureAuthService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAzureAuthService azureAuthService, ILogger<AuthController> logger)
    {
        _azureAuthService = azureAuthService;
        _logger = logger;
    }

    /// <summary>
    /// Authenticate user with Azure AD using username/password
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>Authentication result with tokens</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), 200)]
    [ProducesResponseType(typeof(LoginResponse), 400)]
    [ProducesResponseType(typeof(LoginResponse), 401)]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new LoginResponse(
                Success: false,
                Message: "Invalid request data"
            ));
        }

        try
        {
            var result = await _azureAuthService.AuthenticateAsync(request);
            
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return Unauthorized(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login error for user: {Username}", request.Username);
            return StatusCode(500, new LoginResponse(
                Success: false,
                Message: "Internal server error"
            ));
        }
    }

    /// <summary>
    /// Register a new user in Azure AD
    /// </summary>
    /// <param name="request">Registration details</param>
    /// <returns>Registration result</returns>
    [HttpPost("register")]
    [RateLimit(maxRequests: 5, windowHours: 1, action: "register")]
    [ProducesResponseType(typeof(RegisterResponse), 200)]
    [ProducesResponseType(typeof(RegisterResponse), 400)]
    [ProducesResponseType(typeof(RegisterResponse), 409)]
    [ProducesResponseType(429)] // Too Many Requests
    public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new RegisterResponse(
                Success: false,
                Message: "Invalid request data"
            ));
        }

        try
        {
            var result = await _azureAuthService.RegisterAsync(request);
            
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                // Check if it's a conflict (user already exists)
                if (result.Message?.Contains("already exists") == true)
                {
                    return Conflict(result);
                }
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration error for user: {Username}", request.Username);
            return StatusCode(500, new RegisterResponse(
                Success: false,
                Message: "Internal server error"
            ));
        }
    }

    /// <summary>
    /// Get current user profile information
    /// </summary>
    /// <returns>User profile data</returns>
    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(typeof(UserInfo), 200)]
    [ProducesResponseType(401)]
    public ActionResult<UserInfo> GetProfile()
    {
        try
        {
            var claims = HttpContext.User.Claims;
            
            var userInfo = new UserInfo(
                Id: claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? "",
                Username: claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value ?? "",
                Email: claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? "",
                Name: claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? "",
                GivenName: claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value ?? "",
                FamilyName: claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value ?? ""
            );

            return Ok(userInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user profile");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}
