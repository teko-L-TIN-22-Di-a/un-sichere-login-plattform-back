using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend_api.Models;
using backend_api.Services;
using System.Linq;

namespace backend_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAzureAuthService _authService;

    public AuthController(IAzureAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
        {
            return BadRequest(new LoginResponse
            {
                Success = false,
                Message = "Username and password are required"
            });
        }

        var result = await _authService.AuthenticateAsync(request);
        
        if (result.Success)
        {
            return Ok(result);
        }
        else
        {
            return Unauthorized(result);
        }
    }

    [Authorize]
    [HttpGet("profile")]
    public IActionResult GetProfile()
    {
        var name = User.Identity?.Name;
        var email = User.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
        var oid = User.Claims.FirstOrDefault(c => c.Type == "oid")?.Value; // Object ID (Azure AD)
        var roles = User.Claims.Where(c => c.Type == "roles").Select(c => c.Value).ToList();

        return Ok(new
        {
            Message = "Authenticated!",
            Name = name,
            Email = email,
            ObjectId = oid,
            Roles = roles
        });
    }
}
