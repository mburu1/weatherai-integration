namespace WeatherAI.Client;

public sealed class WeatherAiClientOptions
{
    public const string SectionName = "WeatherAI";

    public string BaseUrl { get; set; } = "https://api.weather-ai.co";

    public string ApiKey { get; set; } = string.Empty;

    public int TimeoutSeconds { get; set; } = 30;
}