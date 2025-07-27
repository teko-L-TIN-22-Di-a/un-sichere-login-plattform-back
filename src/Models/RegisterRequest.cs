namespace backend_api.Models;

public record RegisterRequest(
    string Username,
    string Email,
    string Password,
    string DisplayName,
    string? FirstName = null,
    string? LastName = null
);
