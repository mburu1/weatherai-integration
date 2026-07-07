using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using WeatherAI.Client.Abstractions;
using WeatherAI.Client.Exceptions;
using WeatherAI.Client.Models;
using WeatherAI.Contracts.Enums;
using WeatherAI.Contracts.Models;
using WeatherAI.Contracts.Options;

namespace WeatherAI.Client;

public sealed class WeatherAiClient : IWeatherAiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    private readonly HttpClient _httpClient;
    private readonly WeatherAiClientOptions _options;

    public WeatherAiClient(HttpClient httpClient, IOptions<WeatherAiClientOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public Task<WeatherAiApiResult<WeatherResponse>> GetWeatherAsync(
        WeatherQueryOptions options,
        CancellationToken cancellationToken = default) =>
        SendWeatherRequestAsync("/v1/weather", options, cancellationToken);

    public Task<WeatherAiApiResult<WeatherResponse>> GetCurrentAsync(
        WeatherQueryOptions options,
        CancellationToken cancellationToken = default) =>
        SendWeatherRequestAsync("/v1/current", options, cancellationToken);

    public Task<WeatherAiApiResult<WeatherResponse>> GetHourlyAsync(
        WeatherQueryOptions options,
        CancellationToken cancellationToken = default) =>
        SendWeatherRequestAsync("/v1/hourly", options, cancellationToken);

    public Task<WeatherAiApiResult<WeatherResponse>> GetDailyAsync(
        WeatherQueryOptions options,
        CancellationToken cancellationToken = default) =>
        SendWeatherRequestAsync("/v1/daily", options, cancellationToken);

    public Task<WeatherAiApiResult<WeatherResponse>> GetForecastAsync(
        WeatherQueryOptions options,
        CancellationToken cancellationToken = default) =>
        SendWeatherRequestAsync("/v1/forecast", options, cancellationToken);

    public async Task<WeatherAiApiResult<WeatherResponse>> GetWeatherGeoAsync(
        WeatherGeoQueryOptions options,
        CancellationToken cancellationToken = default)
    {
        var query = BuildGeoQueryString(options);
        using var request = CreateRequest(HttpMethod.Get, $"/v1/weather-geo{query}");
        return await SendAsync<WeatherResponse>(request, cancellationToken);
    }

    public async Task<WeatherAiApiResult<UsageResponse>> GetUsageAsync(
        CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Get, "/v1/usage");
        return await SendAsync<UsageResponse>(request, cancellationToken);
    }

    private async Task<WeatherAiApiResult<WeatherResponse>> SendWeatherRequestAsync(
        string path,
        WeatherQueryOptions options,
        CancellationToken cancellationToken)
    {
        var query = BuildWeatherQueryString(options);
        using var request = CreateRequest(HttpMethod.Get, $"{path}{query}");
        return await SendAsync<WeatherResponse>(request, cancellationToken);
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string relativeUri)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new WeatherAiConfigurationException();
        }

        var request = new HttpRequestMessage(method, relativeUri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        return request;
    }

    private async Task<WeatherAiApiResult<T>> SendAsync<T>(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await TryReadErrorAsync(response, cancellationToken);
            var message = error?.Error ?? error?.Message ?? response.ReasonPhrase ?? "WeatherAI request failed.";
            throw new WeatherAiApiException(message, (int)response.StatusCode);
        }

        var data = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken)
            ?? throw new WeatherAiApiException("WeatherAI returned an empty response body.", (int)response.StatusCode);

        return new WeatherAiApiResult<T>
        {
            Data = data,
            RateLimit = ParseRateLimit(response.Headers)
        };
    }

    private static async Task<WeatherAiErrorResponse?> TryReadErrorAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        try
        {
            return await response.Content.ReadFromJsonAsync<WeatherAiErrorResponse>(JsonOptions, cancellationToken);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static RateLimitInfo? ParseRateLimit(HttpResponseHeaders headers)
    {
        var limit = TryParseHeaderInt(headers, "X-RateLimit-Limit");
        var remaining = TryParseHeaderInt(headers, "X-RateLimit-Remaining");
        var reset = TryParseHeaderLong(headers, "X-RateLimit-Reset");

        if (limit is null && remaining is null && reset is null)
        {
            return null;
        }

        return new RateLimitInfo
        {
            Limit = limit,
            Remaining = remaining,
            ResetUnixEpoch = reset
        };
    }

    private static int? TryParseHeaderInt(HttpResponseHeaders headers, string name) =>
        headers.TryGetValues(name, out var values) &&
        int.TryParse(values.FirstOrDefault(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;

    private static long? TryParseHeaderLong(HttpResponseHeaders headers, string name) =>
        headers.TryGetValues(name, out var values) &&
        long.TryParse(values.FirstOrDefault(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;

    private static string BuildWeatherQueryString(WeatherQueryOptions options)
    {
        var parameters = new List<string>
        {
            $"lat={options.Latitude.ToString(CultureInfo.InvariantCulture)}",
            $"lon={options.Longitude.ToString(CultureInfo.InvariantCulture)}"
        };

        if (options.Days is not null)
        {
            parameters.Add($"days={options.Days.Value}");
        }

        if (options.IncludeAiSummary is not null)
        {
            parameters.Add($"ai={options.IncludeAiSummary.Value.ToString().ToLowerInvariant()}");
        }

        parameters.Add($"units={ToUnitsValue(options.Units)}");

        if (!string.IsNullOrWhiteSpace(options.Language))
        {
            parameters.Add($"lang={Uri.EscapeDataString(options.Language)}");
        }

        return "?" + string.Join('&', parameters);
    }

    private static string BuildGeoQueryString(WeatherGeoQueryOptions options)
    {
        var parameters = new List<string>
        {
            $"ip={Uri.EscapeDataString(options.Ip)}"
        };

        if (options.Latitude is not null)
        {
            parameters.Add($"lat={options.Latitude.Value.ToString(CultureInfo.InvariantCulture)}");
        }

        if (options.Longitude is not null)
        {
            parameters.Add($"lon={options.Longitude.Value.ToString(CultureInfo.InvariantCulture)}");
        }

        if (options.Days is not null)
        {
            parameters.Add($"days={options.Days.Value}");
        }

        if (options.IncludeAiSummary is not null)
        {
            parameters.Add($"ai={options.IncludeAiSummary.Value.ToString().ToLowerInvariant()}");
        }

        return "?" + string.Join('&', parameters);
    }

    private static string ToUnitsValue(WeatherUnits units) =>
        units == WeatherUnits.Imperial ? "imperial" : "metric";
}