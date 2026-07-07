namespace WeatherAI.Contracts.Options;

public sealed class WeatherGeoQueryOptions
{
    public string Ip { get; init; } = "auto";
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public int? Days { get; init; }
    public bool? IncludeAiSummary { get; init; }
}