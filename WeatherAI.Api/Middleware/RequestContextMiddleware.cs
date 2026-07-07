using System.Diagnostics;

namespace WeatherAI.Api.Middleware;

public sealed class RequestContextMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestContextMiddleware> _logger;

    public RequestContextMiddleware(RequestDelegate next, ILogger<RequestContextMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue("X-Correlation-Id", out var existing) &&
                            !string.IsNullOrWhiteSpace(existing)
            ? existing.ToString()
            : Guid.NewGuid().ToString("N");

        context.Response.Headers["X-Correlation-Id"] = correlationId;
        context.Items["CorrelationId"] = correlationId;

        var stopwatch = Stopwatch.StartNew();
        await _next(context);
        stopwatch.Stop();

        _logger.LogInformation(
            "{Method} {Path} responded {StatusCode} in {ElapsedMs}ms (correlation={CorrelationId})",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds,
            correlationId);
    }
}