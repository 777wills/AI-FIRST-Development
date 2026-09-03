// Inicio código generado por GitHub Copilot
using System.Net;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Olimpia.Api.Middleware;

/// <summary>
/// Middleware para capturar excepciones no controladas en el pipeline HTTP.
/// Debe registrarse como el primer middleware en Program.cs para capturar
/// errores de todo el pipeline (incluyendo otros middlewares y controllers).
/// </summary>
public sealed class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    // Método generado por GitHub Copilot
    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    // Método generado por GitHub Copilot
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            context.Items["RequestDuration"] = stopwatch.ElapsedMilliseconds;
            await HandleExceptionAsync(context, ex);
        }
        finally
        {
            if (!stopwatch.IsRunning)
                stopwatch.Stop();
            context.Items["RequestDuration"] = stopwatch.ElapsedMilliseconds;
        }
    }

    // Método generado por GitHub Copilot
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Registrar la excepción como tipo Error con información completa del contexto
        _logger.LogError(
            exception,
            "Excepción no controlada capturada por ExceptionMiddleware. " +
            "TraceId: {TraceId}, Método: {Method}, Path: {Path}, QueryString: {QueryString}, " +
            "Usuario: {User}, IP: {RemoteIp}, Duración: {DurationMs}ms",
            context.TraceIdentifier,
            context.Request.Method,
            context.Request.Path,
            context.Request.QueryString.ToString(),
            context.User?.Identity?.Name ?? "Anonymous",
            context.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
            context.Items.TryGetValue("RequestDuration", out var duration) ? duration : 0
        );

        // Determinar el código de estado HTTP según el tipo de excepción
        var (statusCode, title) = GetStatusCodeAndTitle(exception);

        // Crear un ProblemDetails estándar RFC 7807
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Type = $"https://httpstatuses.com/{statusCode}",
            Instance = context.Request.Path,
            Detail = _environment.IsDevelopment()
                ? exception.ToString() // En desarrollo, mostrar el stack trace completo
                : exception.Message    // En producción, solo el mensaje
        };

        // Agregar el TraceId para correlación en logs
        problemDetails.Extensions["traceId"] = context.TraceIdentifier;

        // En desarrollo, agregar información adicional para debugging
        if (_environment.IsDevelopment())
        {
            problemDetails.Extensions["exceptionType"] = exception.GetType().Name;
            if (exception.InnerException != null)
            {
                problemDetails.Extensions["innerException"] = exception.InnerException.Message;
            }
        }

        // Configurar la respuesta HTTP
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = statusCode;

        // Serializar y enviar la respuesta
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _environment.IsDevelopment()
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, options));
    }

    // Método generado por GitHub Copilot
    private static (int statusCode, string title) GetStatusCodeAndTitle(Exception exception)
    {
        return exception switch
        {
            ValidationException =>
                ((int)HttpStatusCode.BadRequest, "Validación fallida"),

            ArgumentException or ArgumentNullException =>
                ((int)HttpStatusCode.BadRequest, "Argumento inválido"),

            InvalidOperationException =>
                ((int)HttpStatusCode.Conflict, "Conflicto"),

            UnauthorizedAccessException =>
                ((int)HttpStatusCode.Unauthorized, "No autorizado"),

            KeyNotFoundException =>
                ((int)HttpStatusCode.NotFound, "Recurso no encontrado"),

            NotImplementedException =>
                ((int)HttpStatusCode.NotImplemented, "Funcionalidad no implementada"),

            TimeoutException =>
                ((int)HttpStatusCode.RequestTimeout, "Tiempo de espera agotado"),

            _ => ((int)HttpStatusCode.InternalServerError, "Error interno del servidor")
        };
    }
}
// Fin código generado por GitHub Copilot

