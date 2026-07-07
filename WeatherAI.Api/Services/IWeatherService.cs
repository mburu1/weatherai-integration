using WeatherAI.Api.Models;
using WeatherAI.Contracts.Options;

namespace WeatherAI.Api.Services;

public interface IWeatherService
{
    Task<ServiceResult<Contracts.Models.WeatherResponse>> GetWeatherAsync(
        WeatherQueryOptions options,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<WeatherSummaryDto>> GetSummaryAsync(
        WeatherQueryOptions options,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<IReadOnlyList<WeatherSummaryDto>>> CompareAsync(
        IReadOnlyList<(double Lat, double Lon)> locations,
        WeatherQueryOptions template,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<Contracts.Models.UsageResponse>> GetUsageAsync(
        CancellationToken cancellationToken = default);
}

public sealed class ServiceResult<T>
{
    public required T Data { get; init; }

    public Client.Models.RateLimitInfo? RateLimit { get; init; }

    public bool ServedFromCache { get; init; }
}