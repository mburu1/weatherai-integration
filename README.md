# WeatherAI Integration API

ASP.NET Core 10 proxy API that integrates the [WeatherAI `v1/weather` platform](https://weather-ai.co/docs). Consumes real-time and forecast weather data, exposes a clean REST surface with Swagger UI for testing, and keeps the upstream API key server-side.

## Live demo

| Resource | URL |
|----------|-----|
| **GitHub** | https://github.com/mburu1/weatherai-integration |
| **Deployed API** | https://weatherai-integration.onrender.com |
| **Swagger UI** | https://weatherai-integration.onrender.com/swagger |
| **Sample request** | `GET /api/weather?lat=-1.2921&lon=36.8219&days=5&ai=false&units=metric&lang=en` |
| **Dashboard summary** | `GET /api/summary?lat=-1.2921&lon=36.8219&days=5&ai=false&units=metric` |
| **Compare locations** | `GET /api/compare?locations=-1.2921,36.8219\|40.7128,-74.0060&days=3&units=metric` |

## Tech stack

| Layer | Technology |
|-------|------------|
| Runtime | .NET 10 |
| API host | ASP.NET Core Minimal APIs |
| HTTP client | `IHttpClientFactory` + typed `WeatherAiClient` + Polly resilience |
| Caching | `IMemoryCache` (10-min TTL, keyed by lat/lon/days/units/ai/lang) |
| API docs | Swashbuckle.AspNetCore 10 (Swagger UI) |
| Testing | xUnit + RichardSzalay.MockHttp |
| Upstream API | [WeatherAI REST API](https://api.weather-ai.co) (`v1/weather`) |
| Deployment | Docker → Render (or Railway / Azure) |

## Solution structure

```
WeatherAI/
├── WeatherAI.Contracts/   # DTOs, query options, enums
├── WeatherAI.Client/      # Typed HTTP client + DI + Polly retry handler
├── WeatherAI.Api/         # REST surface, application services, Swagger UI
│   ├── Endpoints/         # Minimal API route handlers
│   ├── Services/          # WeatherService (cache), WeatherSummaryMapper
│   ├── Health/            # Upstream readiness probe
│   └── Middleware/        # Correlation ID + request timing
└── WeatherAI.Tests/       # Unit tests (mocked HTTP + mapper)
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
| `GET` | `/api/summary` | `/v1/weather` | Curated dashboard DTO (cached) |
| `GET` | `/api/compare` | `/v1/weather` × N | Side-by-side location comparison (up to 5) |
| `GET` | `/api/usage` | `/v1/usage` | Quota / billing usage |
| `GET` | `/health` | — | Liveness probe |
| `GET` | `/health/ready` | `/v1/usage` | Readiness probe (upstream connectivity) |

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
  },
  "servedFromCache": false
}
```

### Summary response (`/api/summary`)

Returns a consumer-friendly view — place label, temperature string, condition text, AI brief, and daily outlook — instead of the raw upstream payload. Responses include `servedFromCache` so clients know when data was served from the in-memory cache.

### Compare response (`/api/compare`)

Pass `locations=lat,lon|lat,lon` (pipe-separated, max 5 pairs) to fetch and compare summaries in one call. Example: Nairobi vs New York.

## Tools & packages

**WeatherAI.Api**
- `Swashbuckle.AspNetCore` 10.2.3 — Swagger UI
- `Swashbuckle.AspNetCore.Annotations` 10.2.3
- `Microsoft.OpenApi` 2.7.5

**WeatherAI.Client**
- `Microsoft.Extensions.Http` 10.0.0
- `Microsoft.Extensions.Http.Resilience` 10.0.0 — retry on transient failures
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
5. Deploy. Copy the service URL (e.g. `https://weatherai-integration.onrender.com`).
6. Open `https://weatherai-integration.onrender.com/swagger` to test live.

Alternatively, import `render.yaml` for infrastructure-as-code deploy.

## Configuration reference

| Setting | Source | Example |
|---------|--------|---------|
| `WeatherAI:ApiKey` | User secrets / env var | `wai_abc123` |
| `WeatherAI:BaseUrl` | appsettings.json | `https://api.weather-ai.co` |
| `WeatherAI:TimeoutSeconds` | appsettings.json | `30` |
| `Cache:Enabled` | appsettings.json | `true` |
| `Cache:WeatherTtlMinutes` | appsettings.json | `10` |
| `PORT` | Cloud host (Render/Railway) | `8080` |

## Architecture

This is more than a thin proxy — the API layer adds an application service that caches responses, maps raw upstream data into curated DTOs, and handles errors consistently.

```
Client (Swagger / Browser)
        │
        ▼
┌───────────────────────────────────────────────────────────┐
│  WeatherAI.Api                                            │
│  ┌─────────────┐   ┌────────────────┐   ┌──────────────┐  │
│  │ Endpoints   │──►│ WeatherService │──►│ IMemoryCache │  │
│  │ (Minimal)   │   │ + Mapper       │   │ (10 min TTL) │  │
│  └─────────────┘   └───────┬────────┘   └──────────────┘  │
│                            │                              │
│  Middleware: correlation ID, request timing               │
│  Health: /health (live) · /health/ready (upstream probe)  │
└────────────────────────────┼──────────────────────────────┘
                             ▼
                    WeatherAI.Client
                    (Bearer auth + Polly retry)
                             │
                             ▼
                    api.weather-ai.co/v1/*
```

**Design decisions**

- **Server-side API key** — clients never see the `wai_*` token; misconfiguration returns 503, not a crash.
- **Caching** — identical weather queries within the TTL skip upstream calls, preserving quota. `servedFromCache` is exposed in responses.
- **Resilience** — `AddStandardResilienceHandler` retries transient HTTP failures up to 3 times before surfacing 502/504.
- **Presentation layer** — `/api/summary` and `/api/compare` demonstrate consuming raw API data and translating it into purpose-built shapes for dashboards.
- **Observability** — correlation IDs on every request; structured logging of duration and status.

## Author

Built as a technical integration exercise for the WeatherAI developer platform.

## License

MIT