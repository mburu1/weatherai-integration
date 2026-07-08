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

        var timeoutSeconds = configuration
            .GetSection(WeatherAiClientOptions.SectionName)
            .GetValue(nameof(WeatherAiClientOptions.TimeoutSeconds), 30);

        services.AddHttpClient<IWeatherAiClient, WeatherAiClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<
                Microsoft.Extensions.Options.IOptions<WeatherAiClientOptions>>().Value;

            ConfigureHttpClient(client, options);
        })
        .AddStandardResilienceHandler(options => ConfigureResilience(options, timeoutSeconds));

        return services;
    }

    public static IServiceCollection AddWeatherAiClient(
        this IServiceCollection services,
        Action<WeatherAiClientOptions> configure)
    {
        var clientOptions = new WeatherAiClientOptions();
        configure(clientOptions);
        services.Configure(configure);

        services.AddHttpClient<IWeatherAiClient, WeatherAiClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<
                Microsoft.Extensions.Options.IOptions<WeatherAiClientOptions>>().Value;

            ConfigureHttpClient(client, options);
        })
        .AddStandardResilienceHandler(options => ConfigureResilience(options, clientOptions.TimeoutSeconds));

        return services;
    }

    private static void ConfigureHttpClient(HttpClient client, WeatherAiClientOptions options)
    {
        client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
    }

    private static void ConfigureResilience(HttpStandardResilienceOptions options, int timeoutSeconds)
    {
        var attemptTimeout = TimeSpan.FromSeconds(Math.Clamp(timeoutSeconds, 5, 120));

        // Circuit breaker sampling must be >= 2x attempt timeout (Polly validation rule).
        var samplingDuration = TimeSpan.FromTicks(attemptTimeout.Ticks * 2);
        var totalRequestTimeout = TimeSpan.FromSeconds(Math.Max(attemptTimeout.TotalSeconds * 2, 60));

        options.Retry.MaxRetryAttempts = 3;
        options.Retry.Delay = TimeSpan.FromSeconds(1);
        options.AttemptTimeout.Timeout = attemptTimeout;
        options.CircuitBreaker.SamplingDuration = samplingDuration;
        options.TotalRequestTimeout.Timeout = totalRequestTimeout;
    }
}