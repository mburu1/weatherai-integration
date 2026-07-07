namespace WeatherAI.Api.Models;

/// <summary>
/// Consumer-friendly weather view derived from raw WeatherAI payload.
/// </summary>
public sealed class WeatherSummaryDto
{
    public string Place { get; init; } = string.Empty;

    public double Latitude { get; init; }

    public double Longitude { get; init; }

    public string TemperatureLabel { get; init; } = string.Empty;

    public double Temperature { get; init; }

    public double? FeelsLike { get; init; }

    public string Condition { get; init; } = string.Empty;

    public string? AiBrief { get; init; }

    public IReadOnlyList<DailyOutlookDto> Outlook { get; init; } = [];

    public string Units { get; init; } = "metric";
}

public sealed class DailyOutlookDto
{
    public string Date { get; init; } = string.Empty;

    public double High { get; init; }

    public double Low { get; init; }

    public double RainChance { get; init; }
}