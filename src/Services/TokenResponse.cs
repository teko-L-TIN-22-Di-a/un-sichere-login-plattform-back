using System.Text.Json.Serialization;

namespace backend_api.Services;

public record TokenResponse(
    [property: JsonPropertyName("access_token")] string? access_token,
    [property: JsonPropertyName("token_type")] string? token_type,
    [property: JsonPropertyName("expires_in")] int expires_in,
    [property: JsonPropertyName("refresh_token")] string? refresh_token,
    [property: JsonPropertyName("scope")] string? scope
);
