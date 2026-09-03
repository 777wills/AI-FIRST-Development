// Inicio código generado por GitHub Copilot
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OlimpiaIT.Logging.Serilog.Extensions;
using System.Text;

namespace Olimpia.Api.Middleware;

/// <summary>
/// Middleware que registra auditoría estructurada de todas las acciones de usuario.
/// Captura: IP, UserAgent, método HTTP, path, tipo de contenido, duración y estado.
/// </summary>
public sealed class AuditMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditMiddleware> _logger;

    public AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Registra la acción del usuario como auditoría en LogCentral.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var startTime = DateTime.Now;
        var requestInfo = CaptureRequestInfo(context);

        try
        {
            await _next(context);
        }
        finally
        {
            var duration = (DateTime.Now - startTime).TotalMilliseconds;
            context.Items["RequestDuration"] = (long)duration;
            var responseInfo = CaptureResponseInfo(context, (long)duration);

            _logger.LogAudit(
                action: BuildAction(context),
                parameter: BuildParameter(requestInfo, responseInfo),
                beforeValue: requestInfo,
                afterValue: responseInfo
            );
        }
    }

    private static string BuildAction(HttpContext context)
    {
        var pathAndQuery = context.Request.QueryString.HasValue
            ? $"{context.Request.Path}{context.Request.QueryString}"
            : context.Request.Path.ToString();
        return $"{context.Request.Method} {pathAndQuery}";
    }

    private static string CaptureRequestInfo(HttpContext context)
    {
        var sb = new StringBuilder();

        // Capturar IP del cliente (considerando proxy reverso).
        var clientIp = context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
            ?? context.Connection.RemoteIpAddress?.ToString()
            ?? "Unknown";
        sb.Append($"IP: {clientIp}");

        var userAgent = context.Request.Headers.UserAgent.ToString();
        if (!string.IsNullOrEmpty(userAgent))
        {
            // Truncar para evitar headers excesivamente largos en los logs.
            var truncatedAgent = userAgent.Length > 100 ? userAgent[..100] + "..." : userAgent;
            sb.Append($" | User-Agent: {truncatedAgent}");
        }

        var contentType = context.Request.ContentType;
        if (!string.IsNullOrEmpty(contentType))
            sb.Append($" | Content-Type: {contentType}");

        if (context.Request.ContentLength.HasValue)
            sb.Append($" | Content-Length: {context.Request.ContentLength.Value} bytes");

        var referer = context.Request.Headers.Referer.ToString();
        if (!string.IsNullOrEmpty(referer))
            sb.Append($" | Referer: {referer}");

        return sb.ToString();
    }

    private static string BuildParameter(string requestInfo, string responseInfo)
        => $"{requestInfo} → {responseInfo}";

    private static string CaptureResponseInfo(HttpContext context, long durationMs)
    {
        var statusCode = context.Response.StatusCode;
        var statusExecution = statusCode switch
        {
            >= 200 and < 300 => "Success",
            >= 400 and < 500 => "Client Error",
            >= 500 => "Server Error",
            _ => "Unknown"
        };

        var sb = new StringBuilder();
        sb.Append($"Status: {statusCode} ({statusExecution})");
        sb.Append($" | Duration: {durationMs}ms");

        var contentType = context.Response.ContentType;
        if (!string.IsNullOrEmpty(contentType))
            sb.Append($" | Response-Type: {contentType}");

        var userName = context.User?.Identity is { IsAuthenticated: true } identity
            ? identity.Name ?? "Anonymous"
            : "Anonymous";
        sb.Append($" | User: {userName}");

        return sb.ToString();
    }
}
// Fin código generado por GitHub Copilot

