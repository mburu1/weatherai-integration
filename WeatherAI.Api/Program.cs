using WeatherAI.Api.Endpoints;
using WeatherAI.Api.Extensions;
using WeatherAI.Api.Middleware;
using WeatherAI.Api.Swagger;
using WeatherAI.Client;
using WeatherAI.Client.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

builder.Services.AddWeatherAiSwagger();
builder.Services.AddWeatherAiClient(builder.Configuration);
builder.Services.AddWeatherApplication(builder.Configuration);

var app = builder.Build();

var apiKey = app.Configuration[$"{WeatherAiClientOptions.SectionName}:ApiKey"];
if (string.IsNullOrWhiteSpace(apiKey))
{
    app.Logger.LogWarning(
        "WeatherAI API key is not configured. Set WeatherAI__ApiKey environment variable or user secrets.");
}

app.UseMiddleware<RequestContextMiddleware>();
app.UseWeatherAiSwagger();

app.MapGet("/", () => Results.Redirect("/swagger"))
    .ExcludeFromDescription();

app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.MapWeatherEndpoints();

app.Run();