using WeatherAI.Client.Models;
using WeatherAI.Contracts.Models;

namespace WeatherAI.Api.Models;

/// <summary>
/// Successful usage/quota response wrapper.
/// </summary>
public sealed class ApiUsageEnvelope
{
    public UsageResponse Data { get; init; } = new();

    public RateLimitInfo? RateLimit { get; init; }
}