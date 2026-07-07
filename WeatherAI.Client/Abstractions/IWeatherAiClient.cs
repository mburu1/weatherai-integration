using WeatherAI.Client.Models;
using WeatherAI.Contracts.Models;
using WeatherAI.Contracts.Options;

namespace WeatherAI.Client.Abstractions;

public interface IWeatherAiClient
{
    Task<WeatherAiApiResult<WeatherResponse>> GetWeatherAsync(
        WeatherQueryOptions options,
        CancellationToken cancellationToken = default);

    Task<WeatherAiApiResult<WeatherResponse>> GetCurrentAsync(
        WeatherQueryOptions options,
        CancellationToken cancellationToken = default);

    Task<WeatherAiApiResult<WeatherResponse>> GetHourlyAsync(
        WeatherQueryOptions options,
        CancellationToken cancellationToken = default);

    Task<WeatherAiApiResult<WeatherResponse>> GetDailyAsync(
        WeatherQueryOptions options,
        CancellationToken cancellationToken = default);

    Task<WeatherAiApiResult<WeatherResponse>> GetForecastAsync(
        WeatherQueryOptions options,
        CancellationToken cancellationToken = default);

    Task<WeatherAiApiResult<WeatherResponse>> GetWeatherGeoAsync(
        WeatherGeoQueryOptions options,
        CancellationToken cancellationToken = default);

    Task<WeatherAiApiResult<UsageResponse>> GetUsageAsync(
        CancellationToken cancellationToken = default);
}