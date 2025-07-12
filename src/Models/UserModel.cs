namespace backend_api.Models;

public record UserModel(
    int Id,
    string Username,
    string Email,
    bool IsMfaEnabled
);