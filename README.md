# WeatherAI Integration API

ASP.NET Core 10 proxy API that integrates the [WeatherAI `v1/weather` platform](https://weather-ai.co/docs). Consumes real-time and forecast weather data, exposes a clean REST surface with Swagger UI for testing, and keeps the upstream API key server-side.

## Live demo

| Resource | URL |
|----------|-----|
| **GitHub** | https://github.com/mburu1/weatherai-integration |
| **Deployed API** | _Deploy to Render — see [Deploy to Render](#deploy-to-render-recommended)_ |
| **Swagger UI** | `https://your-deploy-url/swagger` |
| **Sample request** | `GET /api/weather?lat=-1.2921&lon=36.8219&days=5&ai=false&units=metric&lang=en` |

## Tech stack

| Layer | Technology |
|-------|------------|
| Runtime | .NET 10 |
| API host | ASP.NET Core Minimal APIs |
| HTTP client | `IHttpClientFactory` + typed `WeatherAiClient` |
| API docs | Swashbuckle.AspNetCore 10 (Swagger UI) |
| Testing | xUnit + RichardSzalay.MockHttp |
| Upstream API | [WeatherAI REST API](https://api.weather-ai.co) (`v1/weather`) |
| Deployment | Docker → Render (or Railway / Azure) |

## Solution structure

```
WeatherAI/
├── WeatherAI.Contracts/   # DTOs, query options, enums
├── WeatherAI.Client/      # Typed HTTP client + DI extensions
├── WeatherAI.Api/         # REST proxy + Swagger UI
└── WeatherAI.Tests/       # Unit tests (mocked HTTP)
```

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- A WeatherAI API key (`wai_…`) from [weather-ai.co](https://weather-ai.co/docs) → Dashboard → API Keys

## Local setup

### 1. Clone

```bash
git clone https://github.com/mburu1/weatherai-integration.git
cd weatherai-integration
```

### 2. Configure API key (never commit this)

```bash
cd WeatherAI.Api
dotnet user-secrets set "WeatherAI:ApiKey" "wai_your_key_here"
```

Or set an environment variable:

```bash
# PowerShell
$env:WeatherAI__ApiKey = "wai_your_key_here"

# bash
export WeatherAI__ApiKey=wai_your_key_here
```

### 3. Run

```bash
cd WeatherAI.Api
dotnet run
```

Open **http://localhost:5180/swagger**

### 4. Test

```bash
dotnet test
```

## API endpoints (proxy)

All routes are prefixed with `/api`. The WeatherAI key is never exposed to clients.

| Method | Endpoint | Upstream | Description |
|--------|----------|----------|-------------|
| `GET` | `/api/weather` | `/v1/weather` | Current + forecast + optional AI summary |
| `GET` | `/api/current` | `/v1/current` | Current conditions only |
| `GET` | `/api/hourly` | `/v1/hourly` | Hourly breakdown |
| `GET` | `/api/daily` | `/v1/daily` | Daily breakdown |
| `GET` | `/api/geo` | `/v1/weather-geo` | Weather via IP geo-detection |
| `GET` | `/api/usage` | `/v1/usage` | Quota / billing usage |

### Query parameters (`/api/weather`)

| Param | Type | Required | Description |
|-------|------|----------|-------------|
| `lat` | float | yes | Latitude (-90 to 90) |
| `lon` | float | yes | Longitude (-180 to 180) |
| `days` | int | no | Forecast days (1–7 Free plan). Default: 7 |
| `ai` | bool | no | Include AI summary. Default: true. Use `false` to save quota |
| `units` | string | no | `metric` (°C) or `imperial` (°F). Default: metric |
| `lang` | string | no | Language code for AI summary (e.g. `en`, `sw`). Default: en |

### Example response

```json
{
  "data": {
    "location": {
      "lat": -1.2921,
      "lon": 36.8219,
      "timezone": "Africa/Nairobi",
      "country": "KE",
      "city": "Nairobi",
      "region": "Nairobi County"
    },
    "current": {
      "temperature": 24.5,
      "condition_code": "2",
      "wind_speed": 12.3
    },
    "daily": [ "..." ],
    "ai_summary": "Partly cloudy with mild temperatures..."
  },
  "rateLimit": {
    "limit": 1000,
    "remaining": 999
  }
}
```

## Tools & packages

**WeatherAI.Api**
- `Swashbuckle.AspNetCore` 10.2.3 — Swagger UI
- `Swashbuckle.AspNetCore.Annotations` 10.2.3
- `Microsoft.OpenApi` 2.7.5

**WeatherAI.Client**
- `Microsoft.Extensions.Http` 10.0.0
- `Microsoft.Extensions.Options.ConfigurationExtensions` 10.0.0

**WeatherAI.Tests**
- `xunit` 2.9.3
- `RichardSzalay.MockHttp` 7.0.0

## Deploy to Render (recommended)

1. Push this repo to a **public** GitHub repository.
2. Create a [Render](https://render.com) account → **New Web Service** → connect the repo.
3. Set **Environment** to **Docker** (uses included `Dockerfile`).
4. Add environment variable:
   - `WeatherAI__ApiKey` = `wai_your_key_here`
5. Deploy. Copy the service URL (e.g. `https://weatherai-api.onrender.com`).
6. Open `https://your-url/swagger` to test live.

Alternatively, import `render.yaml` for infrastructure-as-code deploy.

## Configuration reference

| Setting | Source | Example |
|---------|--------|---------|
| `WeatherAI:ApiKey` | User secrets / env var | `wai_abc123` |
| `WeatherAI:BaseUrl` | appsettings.json | `https://api.weather-ai.co` |
| `WeatherAI:TimeoutSeconds` | appsettings.json | `30` |
| `PORT` | Cloud host (Render/Railway) | `8080` |

## Architecture

```
Client (Swagger / Browser)
        │
        ▼
WeatherAI.Api  ──►  WeatherAI.Client  ──►  api.weather-ai.co/v1/weather
   (Swagger UI)         (Bearer auth)            (upstream data)
```

## Author

Built as a technical integration exercise for the WeatherAI developer platform.

## License

MIT