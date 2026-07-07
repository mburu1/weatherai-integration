using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WeatherAI.Api.Swagger;

/// <summary>
/// Configures Swagger UI controls for weather query parameters.
/// </summary>
public sealed class QueryParameterSchemaFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Parameters is null)
        {
            return;
        }

        foreach (var parameter in operation.Parameters)
        {
            if (string.IsNullOrEmpty(parameter.Name))
            {
                continue;
            }

            if (parameter.Name.Equals("units", StringComparison.OrdinalIgnoreCase))
            {
                parameter.Description = "Unit system: metric (°C) or imperial (°F). Default: metric.";
                ApplyStringEnumSchema(parameter, ["metric", "imperial"], "metric");
            }

            if (parameter.Name.Equals("lang", StringComparison.OrdinalIgnoreCase))
            {
                parameter.Description =
                    "Language code for AI summary. Common values: en, sw, fr, de, es, ar. Default: en.";
                ApplyStringExample(parameter, "en");
            }
        }
    }

    private static void ApplyStringEnumSchema(IOpenApiParameter parameter, string[] values, string defaultValue)
    {
        if (parameter.Schema is not OpenApiSchema schema)
        {
            return;
        }

        schema.Type = JsonSchemaType.String;
        schema.Format = null;
        schema.Enum = values.Select(v => (JsonNode)JsonValue.Create(v)!).ToList();
        schema.Default = JsonValue.Create(defaultValue);
        schema.Example = JsonValue.Create(defaultValue);
    }

    private static void ApplyStringExample(IOpenApiParameter parameter, string example)
    {
        if (parameter.Schema is not OpenApiSchema schema)
        {
            return;
        }

        schema.Type = JsonSchemaType.String;
        schema.Format = null;
        schema.Enum = null;
        schema.Default = JsonValue.Create(example);
        schema.Example = JsonValue.Create(example);
    }
}