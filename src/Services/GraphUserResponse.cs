using System.Text.Json.Serialization;

namespace backend_api.Services;

public record GraphUserResponse(
    [property: JsonPropertyName("id")] string? id,
    [property: JsonPropertyName("displayName")] string? displayName,
    [property: JsonPropertyName("mail")] string? mail,
    [property: JsonPropertyName("userPrincipalName")] string? userPrincipalName,
    [property: JsonPropertyName("givenName")] string? givenName,
    [property: JsonPropertyName("surname")] string? surname
);
