// Inicio código generado por GitHub Copilot
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Olimpia.Domain.Repositories;
using Olimpia.Infrastructure.Configuration;
using Polly;
using Polly.Retry;

namespace Olimpia.Infrastructure.Persistence.Decorators;

/// <summary>
/// Decorador que agrega reintentos automáticos con Polly a IViewRepository.
/// Implementa el patrón Decorator para envolver operaciones de consulta a vistas.
/// Los parámetros de reintento se leen desde la configuración (Repository:RetryEnabled, MaxRetryAttempts, InitialDelayMs).
/// </summary>
public sealed class ViewRepositoryRetryDecorator : IViewRepository
{
    private readonly IViewRepository _innerRepository;
    private readonly ILogger<ViewRepositoryRetryDecorator> _logger;
    private readonly ResiliencePipeline _retryPipeline;

    // Método generado por GitHub Copilot
    public ViewRepositoryRetryDecorator(
        IViewRepository innerRepository,
        ILogger<ViewRepositoryRetryDecorator> logger,
        IOptions<RepositoryRetryOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _innerRepository = innerRepository;
        _logger = logger;
        var retryOptions = options.Value;

        // Configurar política de reintentos con Polly v8
        _retryPipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = retryOptions.MaxRetryAttempts,
                Delay = TimeSpan.FromMilliseconds(retryOptions.InitialDelayMs),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                OnRetry = args =>
                {
                    _logger.LogWarning(
                        "Reintento {Attempt} de {MaxAttempts} para consulta a vista. Excepción: {Exception}",
                        args.AttemptNumber,
                        retryOptions.MaxRetryAttempts,
                        args.Outcome.Exception?.Message);
                    return ValueTask.CompletedTask;
                },
                ShouldHandle = new PredicateBuilder().Handle<Exception>(ex =>
                {
                    var shouldRetry = ex is TimeoutException
                        || ex is Microsoft.Data.SqlClient.SqlException msEx && IsTransient(msEx);

                    if (!shouldRetry)
                    {
                        _logger.LogError(ex, "Error no transitorio en consulta a vista, no se reintentará");
                    }

                    return shouldRetry;
                })
            })
            .Build();
    }

    // Método generado por GitHub Copilot
    public async Task<IEnumerable<T>> QueryAsync<T>(string viewName, object? filters = null)
    {
        return await _retryPipeline.ExecuteAsync(async ct =>
            await _innerRepository.QueryAsync<T>(viewName, filters), CancellationToken.None);
    }

    // Método generado por GitHub Copilot
    public async Task<T?> QuerySingleAsync<T>(string viewName, object? filters = null)
    {
        return await _retryPipeline.ExecuteAsync(async ct =>
            await _innerRepository.QuerySingleAsync<T>(viewName, filters), CancellationToken.None);
    }

    // Método generado por GitHub Copilot
    public async Task<IEnumerable<T>> QueryPagedAsync<T>(
        string viewName,
        int pageNumber,
        int pageSize,
        object? filters = null)
    {
        return await _retryPipeline.ExecuteAsync(async ct =>
            await _innerRepository.QueryPagedAsync<T>(viewName, pageNumber, pageSize, filters), CancellationToken.None);
    }

    // Método generado por GitHub Copilot
    private static bool IsTransient(Microsoft.Data.SqlClient.SqlException ex)
    {
        return ex.Number switch
        {
            -2 => true,      // Timeout
            -1 => true,      // Connection broken
            2 => true,       // Network error
            53 => true,      // Connection could not be established
            64 => true,      // Error in connection
            233 => true,     // Connection initialization error
            10053 => true,   // Transport-level error
            10054 => true,   // Connection forcibly closed
            10060 => true,   // Network timeout
            40197 => true,   // Service error processing request
            40501 => true,   // Service is busy
            40613 => true,   // Database unavailable
            49918 => true,   // Cannot process request
            49919 => true,   // Cannot process create or update request
            49920 => true,   // Cannot process request - too many operations
            _ => false
        };
    }
}
// Fin código generado por GitHub Copilot
