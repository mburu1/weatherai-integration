namespace WeatherAI.Client.Models;

public sealed class WeatherAiApiResult<T>
{
    public required T Data { get; init; }
    public RateLimitInfo? RateLimit { get; init; }
}