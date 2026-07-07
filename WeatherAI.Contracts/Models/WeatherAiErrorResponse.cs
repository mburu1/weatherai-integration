using System.Text.Json.Serialization;

namespace WeatherAI.Contracts.Models;

public sealed class WeatherAiErrorResponse
{
    [JsonPropertyName("error")]
    public string? Error { get; init; }

    [JsonPropertyName("message")]
    public string? Message { get; init; }
}