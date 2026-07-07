using WeatherAI.Contracts.Enums;

namespace WeatherAI.Contracts.Options;

public sealed class WeatherQueryOptions
{
    public required double Latitude { get; init; }
    public required double Longitude { get; init; }
    public int? Days { get; init; }
    public bool? IncludeAiSummary { get; init; }
    public WeatherUnits Units { get; init; } = WeatherUnits.Metric;
    public string Language { get; init; } = "en";
}