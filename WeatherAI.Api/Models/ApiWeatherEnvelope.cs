using WeatherAI.Client.Models;
using WeatherAI.Contracts.Models;

namespace WeatherAI.Api.Models;

/// <summary>
/// Successful weather response wrapper including optional rate-limit metadata.
/// </summary>
public sealed class ApiWeatherEnvelope
{
    public WeatherResponse Data { get; init; } = new();

    public RateLimitInfo? RateLimit { get; init; }
}