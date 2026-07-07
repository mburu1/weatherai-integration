using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using WeatherAI.Api.Models;
using WeatherAI.Api.Options;
using WeatherAI.Client.Abstractions;
using WeatherAI.Client.Models;
using WeatherAI.Contracts.Options;

namespace WeatherAI.Api.Services;

public sealed class WeatherService : IWeatherService
{
    private readonly IWeatherAiClient _client;
    private readonly IMemoryCache _cache;
    private readonly CacheOptions _cacheOptions;
    private readonly ILogger<WeatherService> _logger;

    public WeatherService(
        IWeatherAiClient client,
        IMemoryCache cache,
        IOptions<CacheOptions> cacheOptions,
        ILogger<WeatherService> logger)
    {
        _client = client;
        _cache = cache;
        _cacheOptions = cacheOptions.Value;
        _logger = logger;
    }

    public async Task<ServiceResult<Contracts.Models.WeatherResponse>> GetWeatherAsync(
        WeatherQueryOptions options,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = BuildCacheKey("weather", options);

        if (TryGetCached<Contracts.Models.WeatherResponse>(cacheKey, out var cached, out var cachedRateLimit))
        {
            _logger.LogDebug("Cache hit for {CacheKey}", cacheKey);
            return new ServiceResult<Contracts.Models.WeatherResponse>
            {
                Data = cached!,
                RateLimit = cachedRateLimit,
                ServedFromCache = true
            };
        }

        var result = await _client.GetWeatherAsync(options, cancellationToken);
        SetCache(cacheKey, result.Data, result.RateLimit);

        return new ServiceResult<Contracts.Models.WeatherResponse>
        {
            Data = result.Data,
            RateLimit = result.RateLimit,
            ServedFromCache = false
        };
    }

    public async Task<ServiceResult<WeatherSummaryDto>> GetSummaryAsync(
        WeatherQueryOptions options,
        CancellationToken cancellationToken = default)
    {
        var weather = await GetWeatherAsync(options, cancellationToken);
        var units = options.Units == Contracts.Enums.WeatherUnits.Imperial ? "imperial" : "metric";

        WeatherResponseEnricher.Enrich(weather.Data, options.Latitude, options.Longitude);

        return new ServiceResult<WeatherSummaryDto>
        {
            Data = WeatherSummaryMapper.Map(weather.Data, units),
            RateLimit = weather.RateLimit,
            ServedFromCache = weather.ServedFromCache
        };
    }

    public async Task<ServiceResult<IReadOnlyList<WeatherSummaryDto>>> CompareAsync(
        IReadOnlyList<(double Lat, double Lon)> locations,
        WeatherQueryOptions template,
        CancellationToken cancellationToken = default)
    {
        var summaries = new List<WeatherSummaryDto>(locations.Count);
        RateLimitInfo? lastRateLimit = null;
        var anyCached = false;

        foreach (var (lat, lon) in locations)
        {
            var options = new WeatherQueryOptions
            {
                Latitude = lat,
                Longitude = lon,
                Days = template.Days,
                IncludeAiSummary = false,
                Units = template.Units,
                Language = template.Language
            };

            var result = await GetSummaryAsync(options, cancellationToken);
            summaries.Add(result.Data);
            lastRateLimit = result.RateLimit;
            anyCached |= result.ServedFromCache;
        }

        return new ServiceResult<IReadOnlyList<WeatherSummaryDto>>
        {
            Data = summaries,
            RateLimit = lastRateLimit,
            ServedFromCache = anyCached
        };
    }

    public async Task<ServiceResult<Contracts.Models.UsageResponse>> GetUsageAsync(
        CancellationToken cancellationToken = default)
    {
        var result = await _client.GetUsageAsync(cancellationToken);
        return new ServiceResult<Contracts.Models.UsageResponse>
        {
            Data = result.Data,
            RateLimit = result.RateLimit
        };
    }

    private string BuildCacheKey(string prefix, WeatherQueryOptions options) =>
        $"{prefix}:{options.Latitude:F4}:{options.Longitude:F4}:{options.Days}:{options.Units}:{options.IncludeAiSummary}:{options.Language}";

    private bool TryGetCached<T>(string key, out T? value, out RateLimitInfo? rateLimit)
    {
        value = default;
        rateLimit = null;

        if (!_cacheOptions.Enabled || !_cache.TryGetValue(key, out CachedWeatherEntry? entry) || entry is null)
        {
            return false;
        }

        value = (T?)entry.Payload;
        rateLimit = entry.RateLimit;
        return value is not null;
    }

    private void SetCache(string key, Contracts.Models.WeatherResponse data, RateLimitInfo? rateLimit)
    {
        if (!_cacheOptions.Enabled)
        {
            return;
        }

        var ttl = TimeSpan.FromMinutes(Math.Max(1, _cacheOptions.WeatherTtlMinutes));
        _cache.Set(key, new CachedWeatherEntry { Payload = data, RateLimit = rateLimit }, ttl);
    }

    private sealed class CachedWeatherEntry
    {
        public required object Payload { get; init; }

        public RateLimitInfo? RateLimit { get; init; }
    }
}