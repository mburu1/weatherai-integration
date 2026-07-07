using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace WeatherAI.Api.Models.Queries;

/// <summary>
/// Shared query parameters for location-based weather endpoints.
/// </summary>
public sealed class LocationWeatherQuery
{
    /// <summary>
    /// Latitude (-90 to 90). Example: -1.2921 for Nairobi.
    /// </summary>
    [FromQuery(Name = "lat")]
    [SwaggerParameter("Latitude (-90 to 90)", Required = true)]
    public double Lat { get; init; }

    /// <summary>
    /// Longitude (-180 to 180). Example: 36.8219 for Nairobi.
    /// </summary>
    [FromQuery(Name = "lon")]
    [SwaggerParameter("Longitude (-180 to 180)", Required = true)]
    public double Lon { get; init; }

    /// <summary>
    /// Forecast days (1–7 Free plan). Default: 7.
    /// </summary>
    [FromQuery(Name = "days")]
    [SwaggerParameter("Forecast days (plan-limited). Default: 7")]
    public int? Days { get; init; }

    /// <summary>
    /// Include Gemini AI summary. Default: true. Use false to save AI quota.
    /// </summary>
    [FromQuery(Name = "ai")]
    [SwaggerParameter("Include AI summary. Default: true")]
    public bool? Ai { get; init; }

    /// <summary>
    /// Unit system: metric (°C) or imperial (°F). Default: metric.
    /// </summary>
    [FromQuery(Name = "units")]
    [SwaggerParameter("Unit system: metric (°C) or imperial (°F). Default: metric.")]
    public string Units { get; init; } = "metric";

    /// <summary>
    /// Language code for AI summary, e.g. en, sw, fr. Default: en.
    /// </summary>
    [FromQuery(Name = "lang")]
    [SwaggerParameter("Language code for AI summary (e.g. en, sw, fr). Default: en.")]
    public string Lang { get; init; } = "en";
}