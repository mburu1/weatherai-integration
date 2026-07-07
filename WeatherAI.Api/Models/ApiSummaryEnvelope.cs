using WeatherAI.Client.Models;

namespace WeatherAI.Api.Models;

public sealed class ApiSummaryEnvelope
{
    public WeatherSummaryDto Summary { get; init; } = new();

    public bool ServedFromCache { get; init; }

    public RateLimitInfo? RateLimit { get; init; }
}