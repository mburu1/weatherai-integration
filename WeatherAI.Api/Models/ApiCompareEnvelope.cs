using WeatherAI.Client.Models;

namespace WeatherAI.Api.Models;

public sealed class ApiCompareEnvelope
{
    public IReadOnlyList<WeatherSummaryDto> Locations { get; init; } = [];

    public RateLimitInfo? RateLimit { get; init; }

    public bool ServedFromCache { get; init; }
}