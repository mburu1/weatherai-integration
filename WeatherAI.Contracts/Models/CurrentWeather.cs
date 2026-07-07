using System.Text.Json.Serialization;

namespace WeatherAI.Contracts.Models;

public sealed class CurrentWeather
{
    [JsonPropertyName("time")]
    public string Time { get; init; } = string.Empty;

    [JsonPropertyName("temperature")]
    public double Temperature { get; init; }

    [JsonPropertyName("feels_like")]
    public double? FeelsLike { get; init; }

    [JsonPropertyName("wind_speed")]
    public double WindSpeed { get; init; }

    [JsonPropertyName("wind_direction")]
    public double WindDirection { get; init; }

    [JsonPropertyName("condition_code")]
    public string ConditionCode { get; init; } = string.Empty;

    [JsonPropertyName("icon")]
    public string Icon { get; init; } = string.Empty;

    [JsonPropertyName("humidity")]
    public double? Humidity { get; init; }

    [JsonPropertyName("uv_index")]
    public double? UvIndex { get; init; }

    [JsonPropertyName("visibility")]
    public double? Visibility { get; init; }

    [JsonPropertyName("pressure")]
    public double? Pressure { get; init; }
}