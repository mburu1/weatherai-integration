using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using WeatherAI.Client.Abstractions;

namespace WeatherAI.Client.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWeatherAiClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<WeatherAiClientOptions>(
            configuration.GetSection(WeatherAiClientOptions.SectionName));

        services.AddHttpClient<IWeatherAiClient, WeatherAiClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<
                Microsoft.Extensions.Options.IOptions<WeatherAiClientOptions>>().Value;

            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
        })
        .AddStandardResilienceHandler(options =>
        {
            options.Retry.MaxRetryAttempts = 3;
            options.Retry.Delay = TimeSpan.FromSeconds(1);
            options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(30);
            options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(60);
        });

        return services;
    }

    public static IServiceCollection AddWeatherAiClient(
        this IServiceCollection services,
        Action<WeatherAiClientOptions> configure)
    {
        services.Configure(configure);

        services.AddHttpClient<IWeatherAiClient, WeatherAiClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<
                Microsoft.Extensions.Options.IOptions<WeatherAiClientOptions>>().Value;

            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
        })
        .AddStandardResilienceHandler();

        return services;
    }
}