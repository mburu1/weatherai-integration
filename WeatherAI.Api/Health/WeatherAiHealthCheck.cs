using Microsoft.Extensions.Diagnostics.HealthChecks;
using WeatherAI.Client;
using WeatherAI.Client.Abstractions;

namespace WeatherAI.Api.Health;

public sealed class WeatherAiHealthCheck : IHealthCheck
{
    private readonly IWeatherAiClient _client;
    private readonly IConfiguration _configuration;

    public WeatherAiHealthCheck(IWeatherAiClient client, IConfiguration configuration)
    {
        _client = client;
        _configuration = configuration;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var apiKey = _configuration[$"{WeatherAiClientOptions.SectionName}:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return HealthCheckResult.Degraded("WeatherAI API key is not configured.");
        }

        try
        {
            await _client.GetUsageAsync(cancellationToken);
            return HealthCheckResult.Healthy("WeatherAI upstream is reachable.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("WeatherAI upstream check failed.", ex);
        }
    }
}