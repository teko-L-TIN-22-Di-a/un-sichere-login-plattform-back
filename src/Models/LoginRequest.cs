namespace backend_api.Models;

public record LoginRequest(
    string Username,
    string Password
);
