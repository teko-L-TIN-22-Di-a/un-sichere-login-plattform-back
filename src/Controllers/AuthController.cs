using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using dotnet_backend_api.Models;
using dotnet_backend_api.Services;

namespace dotnet_backend_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserModel userModel)
        {
            var token = await _authService.AuthenticateUser(userModel.Email, userModel.Username); // Username als Passwort? Vermutlich userModel.Password!
            if (token == null)
            {
                return Unauthorized();
            }
            return Ok(new { Token = token });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // Implement logout logic here
            return Ok("Logged out successfully");
        }

        [HttpPost("validate-token")]
        public IActionResult ValidateToken([FromBody] string token)
        {
            var isValid = _authService.ValidateToken(token);
            if (!isValid)
            {
                return Unauthorized();
            }
            return Ok("Token is valid");
        }
    }
}