namespace backend_api.Services;

public class TokenErrorResponse
{
    public string? error { get; set; }
    public string? error_description { get; set; }
    public int[]? error_codes { get; set; }
}
