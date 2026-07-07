using System.Reflection;
using Microsoft.OpenApi;

namespace WeatherAI.Api.Swagger;

public static class SwaggerExtensions
{
    public static IServiceCollection AddWeatherAiSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "WeatherAI Integration API",
                Version = "v1",
                Description =
                    "ASP.NET Core 10 proxy for the WeatherAI v1/weather platform. " +
                    "Use this UI to explore and test endpoints. The upstream API key is kept server-side.",
                Contact = new OpenApiContact
                {
                    Name = "WeatherAI",
                    Url = new Uri("https://weather-ai.co/docs")
                }
            });

            options.EnableAnnotations();
            options.SchemaFilter<LocationWeatherQueryFilter>();
            options.OperationFilter<QueryParameterSchemaFilter>();

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }
        });

        return services;
    }

    public static WebApplication UseWeatherAiSwagger(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "WeatherAI Integration API v1");
            options.DocumentTitle = "WeatherAI Integration API";
            options.DisplayRequestDuration();
            options.EnableTryItOutByDefault();
        });

        return app;
    }
}