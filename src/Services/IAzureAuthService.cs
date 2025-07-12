using backend_api.Models;

namespace backend_api.Services;

public interface IAzureAuthService
{
    Task<LoginResponse> AuthenticateAsync(LoginRequest request);
    Task<RegisterResponse> RegisterAsync(RegisterRequest request);
}
