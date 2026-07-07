using WeatherAI.Api.Models;
using WeatherAI.Contracts.Models;

namespace WeatherAI.Api.Services;

public static class WeatherSummaryMapper
{
    private static readonly Dictionary<int, string> ConditionLabels = new()
    {
        [0] = "Clear sky",
        [1] = "Mainly clear",
        [2] = "Partly cloudy",
        [3] = "Overcast",
        [45] = "Foggy",
        [61] = "Light rain",
        [63] = "Rain",
        [65] = "Heavy rain",
        [95] = "Thunderstorm"
    };

    public static WeatherSummaryDto Map(WeatherResponse source, string units)
    {
        var code = int.TryParse(source.Current.ConditionCode, out var parsed) ? parsed : 0;
        var unitSymbol = units.Equals("imperial", StringComparison.OrdinalIgnoreCase) ? "°F" : "°C";

        var placeParts = new[] { source.Location.City, source.Location.Region, source.Location.Country }
            .Where(p => !string.IsNullOrWhiteSpace(p));
        var place = string.Join(", ", placeParts);
        if (string.IsNullOrWhiteSpace(place))
        {
            place = $"{source.Location.Latitude:F4}, {source.Location.Longitude:F4}";
        }

        return new WeatherSummaryDto
        {
            Place = place,
            Latitude = source.Location.Latitude,
            Longitude = source.Location.Longitude,
            Temperature = source.Current.Temperature,
            FeelsLike = source.Current.FeelsLike,
            TemperatureLabel = $"{source.Current.Temperature:F1}{unitSymbol}",
            Condition = ConditionLabels.GetValueOrDefault(code, "Clear sky"),
            AiBrief = source.AiSummary,
            Units = units,
            Outlook = source.Daily?
                .Select(d => new DailyOutlookDto
                {
                    Date = d.Date,
                    High = d.TempMax,
                    Low = d.TempMin,
                    RainChance = d.PrecipitationProbability
                })
                .ToArray() ?? []
        };
    }
}