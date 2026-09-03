using Microsoft.Extensions.Logging;

namespace Olimpia.Infrastructure;

/// <summary>
/// Método generado por GitHub Copilot
/// 
/// Implementación de <see cref="IRequestLoggingService"/> que registra métricas
/// de requests HTTP de forma centralizada.
/// </summary>
internal sealed class RequestLoggingService : IRequestLoggingService
{
    private readonly ILogger<RequestLoggingService> Logger;

    public RequestLoggingService(ILogger<RequestLoggingService> logger)
    {
        Logger = logger;
    }

    /// <summary>
    /// Registra las métricas de un request HTTP de forma asincrónica.
    /// </summary>
    public async Task LogRequestAsync(
        string method,
        string path,
        int statusCode,
        long durationMs,
        string? userId = null,
        string? traceId = null,
        CancellationToken cancellationToken = default)
    {
        // Inicio código generado por GitHub Copilot
        // Determinar LogLevel según HTTP status code
        var logLevel = statusCode switch
        {
            >= 500 => LogLevel.Error,
            >= 400 => LogLevel.Warning,
            _ => LogLevel.Information
        };

        // Registrar en el logger (será capturado por CustomLogger)
        Logger.Log(
            logLevel,
            "HTTP Request: {Method} {Path} => {StatusCode} ({DurationMs}ms) [UserId: {UserId}]",
            method,
            path,
            statusCode,
            durationMs,
            userId ?? "Anonymous");
        // Fin código generado por GitHub Copilot

        // Retornar task completado para mantener compatibilidad async
        await Task.CompletedTask;
    }
}
