using Microsoft.AspNetCore.Mvc;
using WeatherAI.Api.Models;
using WeatherAI.Api.Models.Queries;
using WeatherAI.Api.Services;
using WeatherAI.Client.Abstractions;
using WeatherAI.Client.Exceptions;
using WeatherAI.Contracts.Enums;
using WeatherAI.Contracts.Options;

namespace WeatherAI.Api.Endpoints;

public static class WeatherEndpoints
{
    public static IEndpointRouteBuilder MapWeatherEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api")
            .WithTags("Weather");

        group.MapGet("/weather", GetWeatherAsync)
            .WithName("GetWeather")
            .WithSummary("Current conditions, forecast, and optional AI summary")
            .WithDescription("Proxies WeatherAI GET /v1/weather. Requires lat and lon query parameters.")
            .Produces<ApiWeatherEnvelope>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiErrorResponse>(StatusCodes.Status503ServiceUnavailable)
            .Produces<ApiErrorResponse>(StatusCodes.Status429TooManyRequests)
            .Produces<ApiErrorResponse>(StatusCodes.Status502BadGateway)
            .Produces<ApiErrorResponse>(StatusCodes.Status504GatewayTimeout);

        group.MapGet("/current", GetCurrentAsync)
            .WithName("GetCurrent")
            .WithSummary("Current conditions only")
            .WithDescription("Proxies WeatherAI GET /v1/current.")
            .Produces<ApiWeatherEnvelope>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized);

        group.MapGet("/hourly", GetHourlyAsync)
            .WithName("GetHourly")
            .WithSummary("Hourly forecast breakdown")
            .WithDescription("Proxies WeatherAI GET /v1/hourly.")
            .Produces<ApiWeatherEnvelope>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized);

        group.MapGet("/daily", GetDailyAsync)
            .WithName("GetDaily")
            .WithSummary("Daily forecast breakdown")
            .WithDescription("Proxies WeatherAI GET /v1/daily.")
            .Produces<ApiWeatherEnvelope>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized);

        group.MapGet("/geo", GetGeoAsync)
            .WithName("GetGeoWeather")
            .WithSummary("Weather with IP-based geo detection")
            .WithDescription("Proxies WeatherAI GET /v1/weather-geo with ip=auto.")
            .Produces<ApiWeatherEnvelope>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized);

        group.MapGet("/summary", GetSummaryAsync)
            .WithName("GetWeatherSummary")
            .WithSummary("Curated weather summary for dashboards")
            .WithDescription("Transforms raw WeatherAI data into a clean, consumer-friendly summary. Uses in-memory cache to reduce upstream calls.")
            .Produces<ApiSummaryEnvelope>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status503ServiceUnavailable);

        group.MapGet("/compare", CompareAsync)
            .WithName("CompareLocations")
            .WithSummary("Compare weather across multiple coordinates")
            .WithDescription("Fetches and compares up to 5 locations. Format: locations=lat,lon|lat,lon (e.g. Nairobi vs New York).")
            .Produces<ApiCompareEnvelope>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status503ServiceUnavailable);

        group.MapGet("/usage", GetUsageAsync)
            .WithName("GetUsage")
            .WithSummary("Billing period usage and quota")
            .WithDescription("Proxies WeatherAI GET /v1/usage.")
            .Produces<ApiUsageEnvelope>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized);

        return app;
    }

    private static async Task<IResult> GetWeatherAsync(
        [AsParameters] LocationWeatherQuery query,
        IWeatherService weatherService,
        CancellationToken cancellationToken)
    {
        if (!TryCreateWeatherOptions(query, includeDays: true, defaultAi: true, out var options, out var validationError))
        {
            return Results.BadRequest(new ApiErrorResponse { Error = validationError!, Status = StatusCodes.Status400BadRequest });
        }

        return await ExecuteAsync(async () =>
        {
            var result = await weatherService.GetWeatherAsync(options!, cancellationToken);
            WeatherResponseEnricher.Enrich(result.Data, query.Lat, query.Lon);

            return Results.Ok(new ApiWeatherEnvelope
            {
                Data = result.Data,
                RateLimit = result.RateLimit,
                ServedFromCache = result.ServedFromCache
            });
        });
    }

    private static async Task<IResult> GetSummaryAsync(
        [AsParameters] LocationWeatherQuery query,
        IWeatherService weatherService,
        CancellationToken cancellationToken)
    {
        if (!TryCreateWeatherOptions(query, includeDays: true, defaultAi: true, out var options, out var validationError))
        {
            return Results.BadRequest(new ApiErrorResponse { Error = validationError!, Status = StatusCodes.Status400BadRequest });
        }

        return await ExecuteAsync(async () =>
        {
            var result = await weatherService.GetSummaryAsync(options!, cancellationToken);
            return Results.Ok(new ApiSummaryEnvelope
            {
                Summary = result.Data,
                RateLimit = result.RateLimit,
                ServedFromCache = result.ServedFromCache
            });
        });
    }

    private static async Task<IResult> CompareAsync(
        [FromQuery] string locations,
        [FromQuery] int? days,
        [FromQuery] string? units,
        IWeatherService weatherService,
        CancellationToken cancellationToken)
    {
        if (!TryParseCompareLocations(locations, out var parsed, out var validationError))
        {
            return Results.BadRequest(new ApiErrorResponse { Error = validationError!, Status = StatusCodes.Status400BadRequest });
        }

        if (!TryParseUnits(units, out var parsedUnits, out validationError))
        {
            return Results.BadRequest(new ApiErrorResponse { Error = validationError!, Status = StatusCodes.Status400BadRequest });
        }

        var template = new WeatherQueryOptions
        {
            Latitude = 0,
            Longitude = 0,
            Days = days ?? 3,
            IncludeAiSummary = false,
            Units = parsedUnits,
            Language = "en"
        };

        return await ExecuteAsync(async () =>
        {
            var result = await weatherService.CompareAsync(parsed!, template, cancellationToken);
            return Results.Ok(new ApiCompareEnvelope
            {
                Locations = result.Data,
                RateLimit = result.RateLimit,
                ServedFromCache = result.ServedFromCache
            });
        });
    }

    private static async Task<IResult> GetCurrentAsync(
        [AsParameters] LocationWeatherQuery query,
        IWeatherAiClient client,
        CancellationToken cancellationToken)
    {
        if (!TryCreateWeatherOptions(query, includeDays: false, defaultAi: false, out var options, out var validationError))
        {
            return Results.BadRequest(new ApiErrorResponse { Error = validationError!, Status = StatusCodes.Status400BadRequest });
        }

        return await ExecuteWeatherAsync(query, () => client.GetCurrentAsync(options!, cancellationToken));
    }

    private static async Task<IResult> GetHourlyAsync(
        [AsParameters] LocationWeatherQuery query,
        IWeatherAiClient client,
        CancellationToken cancellationToken)
    {
        if (!TryCreateWeatherOptions(query, includeDays: true, defaultAi: false, out var options, out var validationError))
        {
            return Results.BadRequest(new ApiErrorResponse { Error = validationError!, Status = StatusCodes.Status400BadRequest });
        }

        return await ExecuteWeatherAsync(query, () => client.GetHourlyAsync(options!, cancellationToken));
    }

    private static async Task<IResult> GetDailyAsync(
        [AsParameters] LocationWeatherQuery query,
        IWeatherAiClient client,
        CancellationToken cancellationToken)
    {
        if (!TryCreateWeatherOptions(query, includeDays: true, defaultAi: false, out var options, out var validationError))
        {
            return Results.BadRequest(new ApiErrorResponse { Error = validationError!, Status = StatusCodes.Status400BadRequest });
        }

        return await ExecuteWeatherAsync(query, () => client.GetDailyAsync(options!, cancellationToken));
    }

    private static async Task<IResult> GetGeoAsync(
        [AsParameters] GeoWeatherQuery query,
        IWeatherAiClient client,
        CancellationToken cancellationToken)
    {
        var options = new WeatherGeoQueryOptions
        {
            Ip = "auto",
            Days = query.Days ?? 1,
            IncludeAiSummary = query.Ai ?? false
        };

        return await ExecuteWeatherAsync(query: null, () => client.GetWeatherGeoAsync(options, cancellationToken));
    }

    private static async Task<IResult> GetUsageAsync(
        IWeatherAiClient client,
        CancellationToken cancellationToken) =>
        await ExecuteUsageAsync(() => client.GetUsageAsync(cancellationToken));

    private static bool TryCreateWeatherOptions(
        LocationWeatherQuery query,
        bool includeDays,
        bool defaultAi,
        out WeatherQueryOptions? options,
        out string? validationError)
    {
        if (query.Lat is < -90 or > 90)
        {
            options = null;
            validationError = "lat must be between -90 and 90.";
            return false;
        }

        if (query.Lon is < -180 or > 180)
        {
            options = null;
            validationError = "lon must be between -180 and 180.";
            return false;
        }

        if (!TryParseUnits(query.Units, out var parsedUnits, out validationError))
        {
            options = null;
            return false;
        }

        if (!TryParseLanguage(query.Lang, out var language, out validationError))
        {
            options = null;
            return false;
        }

        options = new WeatherQueryOptions
        {
            Latitude = query.Lat,
            Longitude = query.Lon,
            Days = includeDays ? query.Days : null,
            IncludeAiSummary = query.Ai ?? defaultAi,
            Units = parsedUnits,
            Language = language
        };

        validationError = null;
        return true;
    }

    private static bool TryParseLanguage(string? lang, out string language, out string? validationError)
    {
        if (string.IsNullOrWhiteSpace(lang))
        {
            language = "en";
            validationError = null;
            return true;
        }

        if (lang.Length is < 2 or > 10 || !lang.All(c => char.IsLetter(c) || c == '-'))
        {
            language = "en";
            validationError = "lang must be a language code such as 'en' or 'sw'.";
            return false;
        }

        language = lang;
        validationError = null;
        return true;
    }

    private static bool TryParseUnits(string? units, out WeatherUnits parsedUnits, out string? validationError)
    {
        if (string.IsNullOrWhiteSpace(units) || units.Equals("metric", StringComparison.OrdinalIgnoreCase))
        {
            parsedUnits = WeatherUnits.Metric;
            validationError = null;
            return true;
        }

        if (units.Equals("imperial", StringComparison.OrdinalIgnoreCase))
        {
            parsedUnits = WeatherUnits.Imperial;
            validationError = null;
            return true;
        }

        parsedUnits = WeatherUnits.Metric;
        validationError = "units must be 'metric' or 'imperial'.";
        return false;
    }

    private static bool TryParseCompareLocations(
        string? locations,
        out IReadOnlyList<(double Lat, double Lon)>? parsed,
        out string? validationError)
    {
        parsed = null;

        if (string.IsNullOrWhiteSpace(locations))
        {
            validationError = "locations is required. Format: lat,lon|lat,lon (e.g. -1.2921,36.8219|40.7128,-74.0060).";
            return false;
        }

        var segments = locations.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (segments.Length is < 1 or > 5)
        {
            validationError = "locations must contain 1 to 5 coordinate pairs separated by '|'.";
            return false;
        }

        var result = new List<(double Lat, double Lon)>(segments.Length);

        foreach (var segment in segments)
        {
            var parts = segment.Split(',', StringSplitOptions.TrimEntries);

            if (parts.Length != 2
                || !double.TryParse(parts[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var lat)
                || !double.TryParse(parts[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var lon))
            {
                validationError = $"Invalid coordinate pair '{segment}'. Each segment must be lat,lon.";
                return false;
            }

            if (lat is < -90 or > 90)
            {
                validationError = $"Latitude {lat} in '{segment}' must be between -90 and 90.";
                return false;
            }

            if (lon is < -180 or > 180)
            {
                validationError = $"Longitude {lon} in '{segment}' must be between -180 and 180.";
                return false;
            }

            result.Add((lat, lon));
        }

        parsed = result;
        validationError = null;
        return true;
    }

    private static async Task<IResult> ExecuteAsync(Func<Task<IResult>> action)
    {
        try
        {
            return await action();
        }
        catch (WeatherAiConfigurationException ex)
        {
            return Results.Json(
                new ApiErrorResponse { Error = ex.Message, Status = StatusCodes.Status503ServiceUnavailable },
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
        catch (WeatherAiApiException ex)
        {
            return Results.Json(
                new ApiErrorResponse { Error = ex.Message, Status = ex.StatusCode },
                statusCode: ex.StatusCode);
        }
        catch (TaskCanceledException)
        {
            return Results.Json(
                new ApiErrorResponse
                {
                    Error = "WeatherAI request timed out. Try again.",
                    Status = StatusCodes.Status504GatewayTimeout
                },
                statusCode: StatusCodes.Status504GatewayTimeout);
        }
        catch (HttpRequestException ex)
        {
            return Results.Json(
                new ApiErrorResponse { Error = ex.Message, Status = StatusCodes.Status502BadGateway },
                statusCode: StatusCodes.Status502BadGateway);
        }
    }

    private static async Task<IResult> ExecuteWeatherAsync(
        LocationWeatherQuery? query,
        Func<Task<Client.Models.WeatherAiApiResult<Contracts.Models.WeatherResponse>>> action)
    {
        try
        {
            var result = await action();

            if (query is not null)
            {
                WeatherResponseEnricher.Enrich(result.Data, query.Lat, query.Lon);
            }

            return Results.Ok(new ApiWeatherEnvelope
            {
                Data = result.Data,
                RateLimit = result.RateLimit
            });
        }
        catch (WeatherAiConfigurationException ex)
        {
            return Results.Json(
                new ApiErrorResponse { Error = ex.Message, Status = StatusCodes.Status503ServiceUnavailable },
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
        catch (WeatherAiApiException ex)
        {
            return Results.Json(
                new ApiErrorResponse { Error = ex.Message, Status = ex.StatusCode },
                statusCode: ex.StatusCode);
        }
        catch (TaskCanceledException)
        {
            return Results.Json(
                new ApiErrorResponse
                {
                    Error = "WeatherAI request timed out. Try again.",
                    Status = StatusCodes.Status504GatewayTimeout
                },
                statusCode: StatusCodes.Status504GatewayTimeout);
        }
        catch (HttpRequestException ex)
        {
            return Results.Json(
                new ApiErrorResponse { Error = ex.Message, Status = StatusCodes.Status502BadGateway },
                statusCode: StatusCodes.Status502BadGateway);
        }
    }

    private static async Task<IResult> ExecuteUsageAsync(
        Func<Task<Client.Models.WeatherAiApiResult<Contracts.Models.UsageResponse>>> action)
    {
        try
        {
            var result = await action();
            return Results.Ok(new ApiUsageEnvelope
            {
                Data = result.Data,
                RateLimit = result.RateLimit
            });
        }
        catch (WeatherAiConfigurationException ex)
        {
            return Results.Json(
                new ApiErrorResponse { Error = ex.Message, Status = StatusCodes.Status503ServiceUnavailable },
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
        catch (WeatherAiApiException ex)
        {
            return Results.Json(
                new ApiErrorResponse { Error = ex.Message, Status = ex.StatusCode },
                statusCode: ex.StatusCode);
        }
        catch (TaskCanceledException)
        {
            return Results.Json(
                new ApiErrorResponse
                {
                    Error = "WeatherAI request timed out. Try again.",
                    Status = StatusCodes.Status504GatewayTimeout
                },
                statusCode: StatusCodes.Status504GatewayTimeout);
        }
        catch (HttpRequestException ex)
        {
            return Results.Json(
                new ApiErrorResponse { Error = ex.Message, Status = StatusCodes.Status502BadGateway },
                statusCode: StatusCodes.Status502BadGateway);
        }
    }
}