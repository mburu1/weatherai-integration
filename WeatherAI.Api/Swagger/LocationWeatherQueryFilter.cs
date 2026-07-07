using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using WeatherAI.Api.Models.Queries;

namespace WeatherAI.Api.Swagger;

/// <summary>
/// Adds coordinate and forecast examples for weather query parameters in Swagger UI.
/// </summary>
public sealed class LocationWeatherQueryFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type != typeof(LocationWeatherQuery))
        {
            return;
        }

        if (schema is not OpenApiSchema openApiSchema || openApiSchema.Properties is null)
        {
            return;
        }

        if (openApiSchema.Properties.TryGetValue("lat", out var latSchema) &&
            latSchema is OpenApiSchema lat)
        {
            lat.Example = JsonValue.Create(-1.2921);
        }

        if (openApiSchema.Properties.TryGetValue("lon", out var lonSchema) &&
            lonSchema is OpenApiSchema lon)
        {
            lon.Example = JsonValue.Create(36.8219);
        }

        if (openApiSchema.Properties.TryGetValue("days", out var daysSchema) &&
            daysSchema is OpenApiSchema days)
        {
            days.Example = JsonValue.Create(7);
        }
    }
}