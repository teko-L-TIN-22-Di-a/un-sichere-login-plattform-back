namespace backend_api.Models;

public record UserInfo(
    string Id,
    string Username,
    string Email,
    string Name,
    string GivenName,
    string FamilyName
);
