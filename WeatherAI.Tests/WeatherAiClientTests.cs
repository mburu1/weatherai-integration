using System.Net;
using System.Text;
using Microsoft.Extensions.Options;
using RichardSzalay.MockHttp;
using WeatherAI.Client;
using WeatherAI.Contracts.Enums;
using WeatherAI.Contracts.Options;
using Xunit;

namespace WeatherAI.Tests;

public sealed class WeatherAiClientTests
{
    private const string SampleWeatherJson = """
        {
          "location": {
            "lat": -1.2921,
            "lon": 36.8219,
            "timezone": "Africa/Nairobi",
            "country": "KE",
            "city": "Nairobi",
            "region": "Nairobi County"
          },
          "current": {
            "time": "2026-07-07T12:00:00Z",
            "temperature": 24.5,
            "feels_like": 25.1,
            "wind_speed": 12.3,
            "wind_direction": 180,
            "condition_code": "2",
            "icon": "partly-cloudy-day",
            "humidity": 62,
            "uv_index": 6,
            "visibility": 10,
            "pressure": 1012
          },
          "daily": [
            {
              "date": "2026-07-07",
              "temp_max": 27,
              "temp_min": 18,
              "condition_code": "2",
              "icon": "partly-cloudy-day",
              "precipitation_probability": 20
            }
          ],
          "ai_summary": "Partly cloudy with mild temperatures in Nairobi today."
        }
        """;

    [Fact]
    public async Task GetWeatherAsync_ReturnsParsedResponse_AndRateLimitHeaders()
    {
        using var mockHttp = new MockHttpMessageHandler();
        mockHttp.When(HttpMethod.Get, "https://api.weather-ai.co/v1/weather*")
            .Respond(request =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(SampleWeatherJson, Encoding.UTF8, "application/json")
                };

                response.Headers.Add("X-RateLimit-Limit", "1000");
                response.Headers.Add("X-RateLimit-Remaining", "999");
                response.Headers.Add("X-RateLimit-Reset", "1717977600");
                return response;
            });

        var client = CreateClient(mockHttp);

        var result = await client.GetWeatherAsync(new WeatherQueryOptions
        {
            Latitude = -1.2921,
            Longitude = 36.8219,
            Days = 7,
            IncludeAiSummary = true,
            Units = WeatherUnits.Metric,
            Language = "en"
        }, TestContext.Current.CancellationToken);

        Assert.Equal("Nairobi", result.Data.Location.City);
        Assert.Equal(24.5, result.Data.Current.Temperature);
        Assert.Single(result.Data.Daily!);
        Assert.NotNull(result.Data.AiSummary);
        Assert.Equal(1000, result.RateLimit!.Limit);
        Assert.Equal(999, result.RateLimit.Remaining);
        Assert.Equal(1717977600, result.RateLimit.ResetUnixEpoch);
    }

    [Fact]
    public async Task GetWeatherAsync_ThrowsWeatherAiApiException_OnUnauthorized()
    {
        using var mockHttp = new MockHttpMessageHandler();
        mockHttp.When(HttpMethod.Get, "https://api.weather-ai.co/v1/weather*")
            .Respond(HttpStatusCode.Unauthorized, "application/json", """{"error":"Invalid API key"}""");

        var client = CreateClient(mockHttp);

        var exception = await Assert.ThrowsAsync<Client.Exceptions.WeatherAiApiException>(() =>
            client.GetWeatherAsync(new WeatherQueryOptions
            {
                Latitude = -1.2921,
                Longitude = 36.8219
            }, TestContext.Current.CancellationToken));

        Assert.Equal(401, exception.StatusCode);
        Assert.Equal("Invalid API key", exception.Message);
    }

    [Fact]
    public async Task GetWeatherAsync_BuildsExpectedQueryString()
    {
        using var mockHttp = new MockHttpMessageHandler();
        string? requestedUri = null;

        mockHttp.When(HttpMethod.Get, "https://api.weather-ai.co/v1/weather*")
            .Respond(request =>
            {
                requestedUri = request.RequestUri?.ToString();
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(SampleWeatherJson, Encoding.UTF8, "application/json")
                };
            });

        var client = CreateClient(mockHttp);

        await client.GetWeatherAsync(new WeatherQueryOptions
        {
            Latitude = -1.2921,
            Longitude = 36.8219,
            Days = 3,
            IncludeAiSummary = false,
            Units = WeatherUnits.Imperial,
            Language = "sw"
        }, TestContext.Current.CancellationToken);

        Assert.NotNull(requestedUri);
        Assert.Contains("lat=-1.2921", requestedUri);
        Assert.Contains("lon=36.8219", requestedUri);
        Assert.Contains("days=3", requestedUri);
        Assert.Contains("ai=false", requestedUri);
        Assert.Contains("units=imperial", requestedUri);
        Assert.Contains("lang=sw", requestedUri);
        Assert.StartsWith("https://api.weather-ai.co/v1/weather?", requestedUri);
    }

    private static WeatherAiClient CreateClient(MockHttpMessageHandler mockHttp)
    {
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://api.weather-ai.co/");

        return new WeatherAiClient(
            httpClient,
            Options.Create(new WeatherAiClientOptions
            {
                ApiKey = "wai_test_key",
                BaseUrl = "https://api.weather-ai.co"
            }));
    }
}