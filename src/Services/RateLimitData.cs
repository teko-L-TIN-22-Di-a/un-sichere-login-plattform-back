namespace backend_api.Services;

internal record RateLimitData(
    int RequestCount,
    DateTime WindowStart,
    DateTime WindowEnd
);
