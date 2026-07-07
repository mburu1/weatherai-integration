using WeatherAI.Api.Services;
using WeatherAI.Contracts.Models;
using Xunit;

namespace WeatherAI.Tests;

public sealed class WeatherSummaryMapperTests
{
    [Fact]
    public void Map_BuildsPlaceLabel_AndTemperatureLabel()
    {
        var source = new WeatherResponse
        {
            Location = new WeatherLocation
            {
                Latitude = -1.2921,
                Longitude = 36.8219,
                City = "Nairobi",
                Region = "Nairobi County",
                Country = "KE"
            },
            Current = new CurrentWeather
            {
                Temperature = 24.5,
                FeelsLike = 25.1,
                ConditionCode = "2"
            },
            Daily =
            [
                new DailyWeather
                {
                    Date = "2026-07-07",
                    TempMax = 27,
                    TempMin = 18,
                    PrecipitationProbability = 20
                }
            ],
            AiSummary = "Partly cloudy with mild temperatures."
        };

        var summary = WeatherSummaryMapper.Map(source, "metric");

        Assert.Equal("Nairobi, Nairobi County, KE", summary.Place);
        Assert.Equal("24.5°C", summary.TemperatureLabel);
        Assert.Equal("Partly cloudy", summary.Condition);
        Assert.Equal("Partly cloudy with mild temperatures.", summary.AiBrief);
        Assert.Single(summary.Outlook);
        Assert.Equal(27, summary.Outlook[0].High);
        Assert.Equal(18, summary.Outlook[0].Low);
        Assert.Equal(20, summary.Outlook[0].RainChance);
    }

    [Fact]
    public void Map_FallsBackToCoordinates_WhenPlaceNamesMissing()
    {
        var source = new WeatherResponse
        {
            Location = new WeatherLocation
            {
                Latitude = 40.7128,
                Longitude = -74.0060
            },
            Current = new CurrentWeather
            {
                Temperature = 72,
                ConditionCode = "0"
            }
        };

        var summary = WeatherSummaryMapper.Map(source, "imperial");

        Assert.Equal("40.7128, -74.0060", summary.Place);
        Assert.Equal("72.0°F", summary.TemperatureLabel);
        Assert.Equal("Clear sky", summary.Condition);
        Assert.Equal("imperial", summary.Units);
    }
}