using System.Text.Json.Serialization;

namespace WeatherAI.Contracts.Models;

public sealed class WeatherResponse
{
    [JsonPropertyName("location")]
    public WeatherLocation Location { get; init; } = new();

    [JsonPropertyName("current")]
    public CurrentWeather Current { get; init; } = new();

    [JsonPropertyName("hourly")]
    public IReadOnlyList<HourlyWeather>? Hourly { get; init; }

    [JsonPropertyName("daily")]
    public IReadOnlyList<DailyWeather>? Daily { get; init; }

    [JsonPropertyName("ai_summary")]
    public string? AiSummary { get; init; }
}