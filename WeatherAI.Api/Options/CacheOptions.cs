namespace WeatherAI.Api.Options;

public sealed class CacheOptions
{
    public const string SectionName = "Cache";

    public int WeatherTtlMinutes { get; set; } = 10;

    public bool Enabled { get; set; } = true;
}