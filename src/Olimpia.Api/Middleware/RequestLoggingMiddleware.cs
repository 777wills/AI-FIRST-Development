// Inicio código generado por GitHub Copilot
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OlimpiaIT.Logging.Serilog.Extensions;

namespace Olimpia.Api.Middleware;

/// <summary>
/// Middleware que registra métricas de cada request HTTP como tipo "Request" en LogCentral.
/// </summary>
public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Intercepta cada request, mide su duración y registra como tipo Request en LogCentral.
    /// </summary>
    public async Task InvokeAsync(HttpContext context, ILogger<RequestLoggingMiddleware> logger)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            context.Items["RequestDuration"] = stopwatch.ElapsedMilliseconds;

            // Registrar request en LogCentral como tipo Request
            logger.LogRequest(
                endpoint: $"{context.Request.Method} {context.Request.Path}",
                method: context.Request.Method,
                statusCode: context.Response.StatusCode,
                durationMs: stopwatch.ElapsedMilliseconds
            );
        }
    }
}
// Fin código generado por GitHub Copilot
