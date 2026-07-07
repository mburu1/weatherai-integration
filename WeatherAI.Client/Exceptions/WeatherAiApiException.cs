namespace WeatherAI.Client.Exceptions;

public sealed class WeatherAiApiException : Exception
{
    public WeatherAiApiException(
        string message,
        int statusCode,
        string? errorCode = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }

    public int StatusCode { get; }

    public string? ErrorCode { get; }
}