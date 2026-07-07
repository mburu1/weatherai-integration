using System.Text.Json.Serialization;

namespace WeatherAI.Contracts.Models;

public sealed class WeatherLocation
{
    [JsonPropertyName("lat")]
    public double Latitude { get; set; }

    [JsonPropertyName("lon")]
    public double Longitude { get; set; }

    [JsonPropertyName("latitude")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? LatitudeAlias
    {
        get => null;
        set
        {
            if (value.HasValue)
            {
                Latitude = value.Value;
            }
        }
    }

    [JsonPropertyName("longitude")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? LongitudeAlias
    {
        get => null;
        set
        {
            if (value.HasValue)
            {
                Longitude = value.Value;
            }
        }
    }

    [JsonPropertyName("timezone")]
    public string Timezone { get; set; } = string.Empty;

    [JsonPropertyName("country")]
    public string Country { get; set; } = string.Empty;

    [JsonPropertyName("city")]
    public string? City { get; set; }

    [JsonPropertyName("region")]
    public string? Region { get; set; }
}