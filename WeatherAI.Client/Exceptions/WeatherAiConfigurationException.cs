namespace WeatherAI.Client.Exceptions;

public sealed class WeatherAiConfigurationException : Exception
{
    public const string SetupMessage =
        "WeatherAI API key is not configured. Set WeatherAI:ApiKey in user secrets: " +
        "dotnet user-secrets set \"WeatherAI:ApiKey\" \"wai_your_key_here\"";

    public WeatherAiConfigurationException()
        : base(SetupMessage)
    {
    }
}