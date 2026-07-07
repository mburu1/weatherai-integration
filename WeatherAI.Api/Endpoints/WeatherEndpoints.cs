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
        IWeatherAiClient client,
        CancellationToken cancellationToken)
    {
        if (!TryCreateWeatherOptions(query, includeDays: true, defaultAi: true, out var options, out var validationError))
        {
            return Results.BadRequest(new ApiErrorResponse { Error = validationError!, Status = StatusCodes.Status400BadRequest });
        }

        return await ExecuteWeatherAsync(query, () => client.GetWeatherAsync(options!, cancellationToken));
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