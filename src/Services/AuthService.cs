using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace dotnet_backend_api.Services
{
    public class AuthService
    {
        private readonly string _secretKey;
        private readonly IDictionary<string, string> _userStore; // Simulated user store for demonstration

        public AuthService(IConfiguration configuration)
        {
            _secretKey = configuration["Jwt:Key"];
            _userStore = new Dictionary<string, string>
            {
                { "user@example.com", "password123" } // Example user
            };
        }

        public async Task<string> AuthenticateUser(string email, string password)
        {
            if (_userStore.TryGetValue(email, out var storedPassword) && storedPassword == password)
            {
                return GenerateToken(email);
            }
            return null;
        }

        private string GenerateToken(string email)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Email, email)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<bool> ValidateMfaToken(string email, string mfaToken)
        {
            // Simulated MFA validation logic
            return mfaToken == "123456"; // Example MFA token
        }

        public bool ValidateToken(string token)
        {
            if (string.IsNullOrEmpty(token)) return false;
            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                tokenHandler.ReadJwtToken(token);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}