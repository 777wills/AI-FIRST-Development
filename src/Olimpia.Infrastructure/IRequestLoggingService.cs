namespace Olimpia.Infrastructure;

/// <summary>
/// Servicio para registrar métricas de requests HTTP.
/// </summary>
public interface IRequestLoggingService
{
    /// <summary>Registra las métricas de un request HTTP completado.</summary>
    Task LogRequestAsync(
        string method,
        string path,
        int statusCode,
        long durationMs,
        string? userId = null,
        string? traceId = null,
        CancellationToken cancellationToken = default);
}
