using System.Text.Json.Serialization;

namespace WeatherAI.Contracts.Models;

public sealed class DailyWeather
{
    [JsonPropertyName("date")]
    public string Date { get; init; } = string.Empty;

    [JsonPropertyName("temp_max")]
    public double TempMax { get; init; }

    [JsonPropertyName("temp_min")]
    public double TempMin { get; init; }

    [JsonPropertyName("condition_code")]
    public string ConditionCode { get; init; } = string.Empty;

    [JsonPropertyName("icon")]
    public string Icon { get; init; } = string.Empty;

    [JsonPropertyName("precipitation_probability")]
    public double PrecipitationProbability { get; init; }

    [JsonPropertyName("precipitation_sum")]
    public double? PrecipitationSum { get; init; }

    [JsonPropertyName("wind_max")]
    public double? WindMax { get; init; }

    [JsonPropertyName("sunrise")]
    public string? Sunrise { get; init; }

    [JsonPropertyName("sunset")]
    public string? Sunset { get; init; }
}