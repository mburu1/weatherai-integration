using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace WeatherAI.Api.Models.Queries;

/// <summary>
/// Query parameters for IP-based geo weather lookup.
/// </summary>
public sealed class GeoWeatherQuery
{
    [FromQuery(Name = "days")]
    [SwaggerParameter("Forecast days. Default: 1")]
    public int? Days { get; init; }

    [FromQuery(Name = "ai")]
    [SwaggerParameter("Include AI summary. Default: false")]
    public bool? Ai { get; init; }
}