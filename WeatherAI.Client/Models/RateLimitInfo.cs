namespace WeatherAI.Client.Models;

public sealed class RateLimitInfo
{
    public int? Limit { get; init; }
    public int? Remaining { get; init; }
    public long? ResetUnixEpoch { get; init; }
}