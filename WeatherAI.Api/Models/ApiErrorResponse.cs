namespace WeatherAI.Api.Models;

/// <summary>
/// Standard error payload returned by the proxy API.
/// </summary>
public sealed class ApiErrorResponse
{
    public string Error { get; init; } = string.Empty;

    public int Status { get; init; }
}