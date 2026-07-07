using WeatherAI.Contracts.Models;

namespace WeatherAI.Api.Services;

public static class WeatherResponseEnricher
{
    public static WeatherResponse Enrich(WeatherResponse response, double requestLat, double requestLon)
    {
        if (response.Location.Latitude == 0 && response.Location.Longitude == 0)
        {
            response.Location.Latitude = requestLat;
            response.Location.Longitude = requestLon;
        }

        return response;
    }
}