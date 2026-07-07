using WeatherAI.Api.Health;
using WeatherAI.Api.Options;
using WeatherAI.Api.Services;

namespace WeatherAI.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWeatherApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CacheOptions>(configuration.GetSection(CacheOptions.SectionName));
        services.AddMemoryCache();
        services.AddScoped<IWeatherService, WeatherService>();

        services.AddHealthChecks()
            .AddCheck<WeatherAiHealthCheck>("weatherai", tags: ["ready", "upstream"]);

        return services;
    }
}