using System.Text.Json.Serialization;

namespace WeatherAI.Contracts.Models;

public sealed class UsageResponse
{
    [JsonPropertyName("plan")]
    public string Plan { get; init; } = string.Empty;

    [JsonPropertyName("requests_used")]
    public int RequestsUsed { get; init; }

    [JsonPropertyName("requests_limit")]
    public int RequestsLimit { get; init; }

    [JsonPropertyName("ai_requests_used")]
    public int AiRequestsUsed { get; init; }

    [JsonPropertyName("ai_requests_limit")]
    public int AiRequestsLimit { get; init; }

    [JsonPropertyName("period_start")]
    public string? PeriodStart { get; init; }

    [JsonPropertyName("period_end")]
    public string? PeriodEnd { get; init; }
}