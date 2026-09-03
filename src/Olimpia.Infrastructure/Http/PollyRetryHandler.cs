// Inicio código generado por GitHub Copilot
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Olimpia.Infrastructure.Configuration;
using Polly;
using Polly.Retry;
using System.Net;

namespace Olimpia.Infrastructure.Http;

/// <summary>
/// DelegatingHandler que aplica reintentos automáticos con Polly a las llamadas HTTP.
/// Detecta errores transitorios de red y aplica backoff exponencial.
/// Los parámetros de reintento se leen desde la configuración (HttpClient:RetryEnabled, MaxRetryAttempts, InitialDelayMs).
/// </summary>
public sealed class PollyRetryHandler : DelegatingHandler
{
    private readonly ILogger<PollyRetryHandler> _logger;
    private readonly ResiliencePipeline<HttpResponseMessage> _retryPipeline;

    // Método generado por GitHub Copilot
    public PollyRetryHandler(
        ILogger<PollyRetryHandler> logger,
        IOptions<HttpClientRetryOptions> options)
    {
        _logger = logger;
        var retryOptions = options.Value;

        // Configurar política de reintentos con Polly v8
        _retryPipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
            {
                MaxRetryAttempts = retryOptions.MaxRetryAttempts,
                Delay = TimeSpan.FromMilliseconds(retryOptions.InitialDelayMs),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                OnRetry = args =>
                {
                    var exception = args.Outcome.Exception;
                    var statusCode = args.Outcome.Result?.StatusCode;

                    _logger.LogWarning(
                        "Reintento HTTP {Attempt} de {MaxAttempts}. StatusCode: {StatusCode}, Excepción: {Exception}",
                        args.AttemptNumber,
                        retryOptions.MaxRetryAttempts,
                        statusCode,
                        exception?.Message ?? "N/A");

                    return ValueTask.CompletedTask;
                },
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<HttpRequestException>()
                    .Handle<TaskCanceledException>()
                    .Handle<TimeoutException>()
                    .HandleResult(response => ShouldRetryHttpStatus(response.StatusCode))
            })
            .Build();
    }

    // Método generado por GitHub Copilot
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        return await _retryPipeline.ExecuteAsync(
            async ct => await base.SendAsync(request, ct),
            cancellationToken);
    }

    // Método generado por GitHub Copilot
    private static bool ShouldRetryHttpStatus(HttpStatusCode statusCode)
    {
        // Reintentar solo en errores transitorios
        return statusCode switch
        {
            HttpStatusCode.RequestTimeout => true,           // 408
            HttpStatusCode.TooManyRequests => true,          // 429
            HttpStatusCode.InternalServerError => true,      // 500
            HttpStatusCode.BadGateway => true,               // 502
            HttpStatusCode.ServiceUnavailable => true,       // 503
            HttpStatusCode.GatewayTimeout => true,           // 504
            _ => false
        };
    }
}
// Fin código generado por GitHub Copilot
