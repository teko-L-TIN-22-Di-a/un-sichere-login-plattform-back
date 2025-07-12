using System.Text.Json.Serialization;

namespace backend_api.Services;

public record TokenErrorResponse(
    [property: JsonPropertyName("error")] string? error,
    [property: JsonPropertyName("error_description")] string? error_description,
    [property: JsonPropertyName("error_codes")] int[]? error_codes
);
