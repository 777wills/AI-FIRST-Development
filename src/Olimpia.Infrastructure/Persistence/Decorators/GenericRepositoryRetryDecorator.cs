// Inicio código generado por GitHub Copilot
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Olimpia.Domain.Repositories;
using Olimpia.Infrastructure.Configuration;
using Polly;
using Polly.Retry;

namespace Olimpia.Infrastructure.Persistence.Decorators;

/// <summary>
/// Decorador que agrega reintentos automáticos con Polly a un IGenericRepository.
/// Implementa el patrón Decorator para envolver cualquier repositorio genérico.
/// Los parámetros de reintento se leen desde la configuración (Repository:RetryEnabled, MaxRetryAttempts, InitialDelayMs).
/// </summary>
/// <typeparam name="T">Tipo de entidad del repositorio</typeparam>
public sealed class GenericRepositoryRetryDecorator<T> : IGenericRepository<T> where T : class
{
    private readonly IGenericRepository<T> _innerRepository;
    private readonly ILogger<GenericRepositoryRetryDecorator<T>> _logger;
    // Inicio refactorización/optimización por GitHub Copilot
    /// <summary>Pipeline de reintentos usado exclusivamente en operaciones de <b>lectura</b> (idempotentes).</summary>
    private readonly ResiliencePipeline _readPipeline;
    // Fin refactorización/optimización por GitHub Copilot

    // Método generado por GitHub Copilot
    public GenericRepositoryRetryDecorator(
        IGenericRepository<T> innerRepository,
        ILogger<GenericRepositoryRetryDecorator<T>> logger,
        IOptions<RepositoryRetryOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _innerRepository = innerRepository;
        _logger = logger;
        var retryOptions = options.Value;

        // Inicio refactorización/optimización por GitHub Copilot
        // Configurar pipeline de reintentos con Polly v8 — solo para operaciones de lectura
        _readPipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = retryOptions.MaxRetryAttempts,
                Delay = TimeSpan.FromMilliseconds(retryOptions.InitialDelayMs),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                OnRetry = args =>
                {
                    _logger.LogWarning(
                        "Reintento {Attempt} de {MaxAttempts} para operación en repositorio {Repository}. Excepción: {Exception}",
                        args.AttemptNumber,
                        retryOptions.MaxRetryAttempts,
                        typeof(T).Name,
                        args.Outcome.Exception?.Message);
                    return ValueTask.CompletedTask;
                },
                ShouldHandle = new PredicateBuilder().Handle<Exception>(ex =>
                {
                    // Solo reintentar en errores transitorios (timeouts, conexión, etc.)
                    var shouldRetry = ex is TimeoutException
                        || ex is Microsoft.Data.SqlClient.SqlException msEx && IsTransient(msEx);

                    if (!shouldRetry)
                    {
                        _logger.LogError(ex, "Error no transitorio en repositorio {Repository}, no se reintentará", typeof(T).Name);
                    }

                    return shouldRetry;
                })
            })
            .Build();
        // Fin refactorización/optimización por GitHub Copilot
    }

    // Método generado por GitHub Copilot
    public async Task<T?> GetByIdAsync(int id)
    {
        return await _readPipeline.ExecuteAsync(async ct =>
            await _innerRepository.GetByIdAsync(id), CancellationToken.None);
    }

    // Método generado por GitHub Copilot
    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _readPipeline.ExecuteAsync(async ct =>
            await _innerRepository.GetAllAsync(), CancellationToken.None);
    }

    // Inicio refactorización/optimización por GitHub Copilot
    // Las operaciones de escritura NO usan pipeline de retry (no son idempotentes).
    // Un timeout de SQL Server (código -2) puede significar que la operación completó
    // en el servidor; reintentar podría causar duplicados o doble ejecución.
    public async Task<int> AddAsync(T entity)
        => await _innerRepository.AddAsync(entity);

    public async Task<bool> UpdateAsync(T entity)
        => await _innerRepository.UpdateAsync(entity);

    public async Task<bool> DeleteAsync(int id)
        => await _innerRepository.DeleteAsync(id);
    // Fin refactorización/optimización por GitHub Copilot

    // Inicio código generado por GitHub Copilot
    // Método generado por GitHub Copilot
    public async Task<(IEnumerable<T> Data, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        IReadOnlyList<global::Olimpia.Domain.Common.FilterCriteria>? filters,
        IReadOnlyList<global::Olimpia.Domain.Common.SortCriteria>? sortFields)
        => await _readPipeline.ExecuteAsync(
            async _ => await _innerRepository.GetPagedAsync(pageNumber, pageSize, filters, sortFields));
    // Fin código generado por GitHub Copilot

    // Método generado por GitHub Copilot
    private static bool IsTransient(Microsoft.Data.SqlClient.SqlException ex)
    {
        // Códigos de error transitorios de SQL Server
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
