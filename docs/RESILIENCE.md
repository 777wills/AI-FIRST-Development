# 🛡️ Resiliencia - Polly, Reintentos y Circuit Breaker

Documentación de patrones de resiliencia con **Polly v8** para manejar fallos transitorios en llamadas HTTP y operaciones de BD.

---

## Visión General

La resiliencia es la capacidad de un sistema de **recuperarse de fallos** sin intervención manual.

### Tipos de Fallos

| Tipo | Ejemplo | Estrategia |
|------|---------|-----------|
| Transitorio | Timeout temporal, API sobrecargada | Reintentos |
| Semipermanente | Circuit breaker abierto | Fallback / Esperar |
| Permanente | Recurso no existe (404) | Fallar rápido |

---

## 1. Polly v8 - ResiliencePipeline

Polly v8 introduce `ResiliencePipeline` (antes `IAsyncPolicy`).

### Instalación

```bash
dotnet add package Polly
dotnet add package Polly.Extensions.Http
```

---

## 2. Retry (Reintentos)

### Estrategia Simple

```csharp
// Inicio código generado por GitHub Copilot
var retryPolicy = new ResiliencePipelineBuilder<HttpResponseMessage>()
    .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
    {
        MaxRetryAttempts = 3,
        Delay = TimeSpan.FromMilliseconds(200),  // Espera 200ms entre reintentos
    })
    .Build();

// Usar
var result = await retryPolicy.ExecuteAsync(
    async () => await httpClient.GetAsync("https://api.example.com/data"),
    CancellationToken.None);
// Fin código generado por GitHub Copilot
```

### Backoff Exponencial (Aumentar espera)

```csharp
// Inicio código generado por GitHub Copilot
var retryPolicy = new ResiliencePipelineBuilder<HttpResponseMessage>()
    .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
    {
        MaxRetryAttempts = 3,
        Delay = TimeSpan.FromMilliseconds(200),
        BackoffType = DelayBackoffType.Exponential,  // 200ms, 400ms, 800ms
        UseJitter = true,  // Agregar randomización para evitar "thundering herd"
    })
    .Build();

// Reintentos con backoff exponencial:
// Intento 1: Espera 200ms
// Intento 2: Espera 400ms (200ms * 2)
// Intento 3: Espera 800ms (200ms * 4)
// Total posible: 1400ms
// Fin código generado por GitHub Copilot
```

### Con Logging de Reintentos

```csharp
// Inicio código generado por GitHub Copilot
var retryPolicy = new ResiliencePipelineBuilder<HttpResponseMessage>()
    .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
    {
        MaxRetryAttempts = 3,
        Delay = TimeSpan.FromMilliseconds(200),
        BackoffType = DelayBackoffType.Exponential,
        OnRetry = args =>
        {
            var statusCode = args.Outcome.Result?.StatusCode;
            var exception = args.Outcome.Exception;
            
            _logger.LogWarning(
                "Reintento {AttemptNumber} de {MaxAttempts}. " +
                "StatusCode: {StatusCode}, Excepción: {Exception}, " +
                "Espera: {DelayMs}ms",
                args.AttemptNumber,
                3,
                statusCode,
                exception?.GetType().Name,
                (long)args.RetryDelay.TotalMilliseconds);
        }
    })
    .Build();
// Fin código generado por GitHub Copilot
```

### Qué Reintentar

```csharp
var retryPolicy = new ResiliencePipelineBuilder<HttpResponseMessage>()
    .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
    {
        MaxRetryAttempts = 3,
        Delay = TimeSpan.FromMilliseconds(200),
        ShouldHandle = args =>
        {
            // Reintentar si:
            // 1. HttpRequestException (conexión, DNS)
            if (args.Outcome.Exception is HttpRequestException)
                return true;

            // 2. Timeout
            if (args.Outcome.Exception is TaskCanceledException)
                return true;

            // 3. Status codes transitorios
            var statusCode = args.Outcome.Result?.StatusCode;
            return statusCode is
                System.Net.HttpStatusCode.RequestTimeout or      // 408
                System.Net.HttpStatusCode.TooManyRequests or     // 429
                System.Net.HttpStatusCode.InternalServerError or // 500
                System.Net.HttpStatusCode.BadGateway or          // 502
                System.Net.HttpStatusCode.ServiceUnavailable or  // 503
                System.Net.HttpStatusCode.GatewayTimeout;        // 504
        }
    })
    .Build();

// ❌ NO reintentar:
// - 400 Bad Request (error del cliente, no transitorio)
// - 401 Unauthorized (token inválido)
// - 403 Forbidden (no autorizado)
// - 404 Not Found (recurso no existe)
```

---

## 3. Circuit Breaker

Detiene solicitudes cuando una API está caída, para evitar "golpear" un servicio degradado.

### Flujo de Estados

```
CLOSED (normal)
    │
    ├─ Fallos consecutivos > threshold
    │
    ▼
OPEN (rechaza requests)
    │
    ├─ Espera durationOfBreak segundos
    │
    ▼
HALF_OPEN (prueba 1 request)
    │
    ├─ Si éxito → volver a CLOSED
    ├─ Si fallo → volver a OPEN (reinicia timer)
```

### Implementación

```csharp
// Inicio código generado por GitHub Copilot
var circuitBreakerPolicy = new ResiliencePipelineBuilder<HttpResponseMessage>()
    .AddCircuitBreaker(new CircuitBreakerStrategyOptions<HttpResponseMessage>
    {
        FailureRatio = 0.5,  // 50% de fallos
        MinimumThroughput = 3,  // Mínimo 3 requests antes de evaluar
        SamplingDuration = TimeSpan.FromSeconds(30),  // Evaluar cada 30 segundos
        BreakDuration = TimeSpan.FromSeconds(60),  // Abrir circuito por 60 segundos
        ShouldHandle = args =>
        {
            // Contar como fallo si:
            return args.Outcome.Exception is HttpRequestException ||
                   args.Outcome.Result?.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable;
        },
        OnOpened = args =>
        {
            _logger.LogError(
                "Circuit breaker ABIERTO. Fallos: {FailureCount}/{MinThroughput}. " +
                "Reabriendo en {BreakDuration}s",
                args.FailureCount,
                3,
                args.BreakDuration.TotalSeconds);
        },
        OnClosed = args =>
        {
            _logger.LogInformation("Circuit breaker cerrado. Servicio recuperado.");
        }
    })
    .Build();

// Uso
try
{
    var result = await circuitBreakerPolicy.ExecuteAsync(
        async () => await httpClient.GetAsync("https://api.example.com/data"));
}
catch (BrokenCircuitException)
{
    // Servicio está degradado, usar fallback
    return GetCachedOrDefaultData();
}
// Fin código generado por GitHub Copilot
```

---

## 4. Timeout

Evitar que una solicitud cuelgue indefinidamente.

```csharp
var timeoutPolicy = new ResiliencePipelineBuilder<HttpResponseMessage>()
    .AddTimeout(new TimeoutStrategyOptions<HttpResponseMessage>
    {
        Timeout = TimeSpan.FromSeconds(30),
        OnTimeoutAsync = args =>
        {
            _logger.LogWarning("Timeout de {Timeout}s en {Request}", 
                args.Timeout.TotalSeconds,
                args.Context.OperationKey);
            return default;
        }
    })
    .Build();
```

---

## 5. Envolver Múltiples Políticas

Usar `Wrap` para combinar retry + circuit breaker + timeout.

```csharp
// Inicio código generado por GitHub Copilot
var retryPolicy = new ResiliencePipelineBuilder<HttpResponseMessage>()
    .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
    {
        MaxRetryAttempts = 3,
        Delay = TimeSpan.FromMilliseconds(200),
        BackoffType = DelayBackoffType.Exponential,
    })
    .Build();

var circuitBreakerPolicy = new ResiliencePipelineBuilder<HttpResponseMessage>()
    .AddCircuitBreaker(new CircuitBreakerStrategyOptions<HttpResponseMessage>
    {
        FailureRatio = 0.5,
        MinimumThroughput = 5,
        BreakDuration = TimeSpan.FromSeconds(60),
    })
    .Build();

var timeoutPolicy = new ResiliencePipelineBuilder<HttpResponseMessage>()
    .AddTimeout(TimeSpan.FromSeconds(30))
    .Build();

// Combinar: Retry > CircuitBreaker > Timeout
var combinedPolicy = ResiliencePipeline
    .Wrap(retryPolicy, circuitBreakerPolicy, timeoutPolicy);

// Flujo:
// 1. Timeout (aborta si > 30s)
// 2. CircuitBreaker (rechaza si degradado)
// 3. Retry (reintenta si transitorio)
// 4. HttpClient (realiza request)

var result = await combinedPolicy.ExecuteAsync(
    async () => await httpClient.GetAsync("https://api.example.com/data"));
// Fin código generado por GitHub Copilot
```

### Orden Importa

```
┌─────────────────────────────────┐
│  Timeout (límite absoluto)      │
├─────────────────────────────────┤
│  CircuitBreaker (evitar golpear)│
├─────────────────────────────────┤
│  Retry (recuperarse)            │
├─────────────────────────────────┤
│  HttpClient (ejecutar)          │
└─────────────────────────────────┘
```

Lectura: De adentro hacia afuera cuando se ejecuta.

---

## 6. Fallback

Usar un valor alternativo cuando la política falla.

```csharp
// Inicio código generado por GitHub Copilot
public sealed class GetProductAnalyticsHandler : IQueryHandler<GetProductAnalyticsQuery, AnalyticsDto>
{
    private readonly IExternalApiClient _client;
    private readonly IDistributedCache _cache;
    private readonly ILogger<GetProductAnalyticsHandler> _logger;
    private readonly ResiliencePipeline<AnalyticsDto> _policy;

    public GetProductAnalyticsHandler(
        IExternalApiClient client,
        IDistributedCache cache,
        ILogger<GetProductAnalyticsHandler> logger)
    {
        _client = client;
        _cache = cache;
        _logger = logger;

        // Política: Retry > CircuitBreaker > Fallback
        _policy = new ResiliencePipelineBuilder<AnalyticsDto>()
            .AddRetry(new RetryStrategyOptions<AnalyticsDto>
            {
                MaxRetryAttempts = 2,
                Delay = TimeSpan.FromMilliseconds(200),
            })
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions<AnalyticsDto>
            {
                FailureRatio = 0.5,
                BreakDuration = TimeSpan.FromSeconds(60),
            })
            .AddFallback(new FallbackStrategyOptions<AnalyticsDto>
            {
                FallbackAction = args =>
                {
                    _logger.LogWarning("Usando fallback para analytics");
                    return Outcome.FromResultAsValueTask(
                        new AnalyticsDto { IsFromFallback = true });
                }
            })
            .Build();
    }

    public async Task<AnalyticsDto> Handle(GetProductAnalyticsQuery query, CancellationToken ct)
    {
        return await _policy.ExecuteAsync(
            async () => await _client.GetAsync<AnalyticsDto>(
                "AnalyticsService",
                $"api/analytics/{query.ProductId}",
                ct));
    }
}
// Fin código generado por GitHub Copilot
```

---

## 7. Polly en PollyRetryHandler

Ver [**HTTP_CLIENTS.md**](HTTP_CLIENTS.md) para implementación completa de `PollyRetryHandler`.

El handler decora `HttpClient` y aplica políticas automáticamente.

---

## 8. Testing

### Mock de Polly

```csharp
[TestClass]
public sealed class ResilienceTests
{
    [TestMethod]
    public async Task RetryPolicy_RetriesOnTransientError()
    {
        // Arrange
        var attempts = 0;
        var policy = new ResiliencePipelineBuilder<string>()
            .AddRetry(new RetryStrategyOptions<string>
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromMilliseconds(10),
            })
            .Build();

        // Act
        var result = await policy.ExecuteAsync(async () =>
        {
            attempts++;
            if (attempts < 3)
                throw new HttpRequestException("Transitorio");
            return "Éxito";
        });

        // Assert
        Assert.AreEqual("Éxito", result);
        Assert.AreEqual(3, attempts);
    }

    [TestMethod]
    public async Task CircuitBreakerPolicy_OpensOnRepeatedFailures()
    {
        // Arrange
        var policy = new ResiliencePipelineBuilder<string>()
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions<string>
            {
                FailureRatio = 0.5,
                MinimumThroughput = 2,
                SamplingDuration = TimeSpan.FromMilliseconds(100),
                BreakDuration = TimeSpan.FromSeconds(1),
            })
            .Build();

        // Act & Assert
        for (int i = 0; i < 3; i++)
        {
            try
            {
                await policy.ExecuteAsync(async () =>
                    throw new HttpRequestException("Fallo"));
            }
            catch (BrokenCircuitException)
            {
                Assert.IsTrue(i >= 2);  // Abre después de 2 fallos
            }
        }
    }
}
```

---

## 9. Métricas y Monitoreo

```csharp
public sealed class ResilienceMetrics
{
    public long TotalRequests { get; set; }
    public long SuccessfulRequests { get; set; }
    public long FailedRequests { get; set; }
    public long RetriedRequests { get; set; }
    public long CircuitBreakerOpenings { get; set; }

    public double SuccessRate => TotalRequests > 0 
        ? (double)SuccessfulRequests / TotalRequests 
        : 0;
}

// Logging de métricas
services.AddLogging(config =>
{
    config.AddConsole();
    config.SetMinimumLevel(LogLevel.Information);
});

// En handler
_logger.LogInformation(
    "API call - Total: {Total}, Success: {Success}, Retries: {Retries}, " +
    "Success Rate: {Rate:P}",
    metrics.TotalRequests,
    metrics.SuccessfulRequests,
    metrics.RetriedRequests,
    metrics.SuccessRate);
```

---

## 10. Buenas Prácticas

| Recomendación | Razón |
|---------------|-------|
| ✅ Usar backoff exponencial | Evitar "thundering herd" |
| ✅ Agregar jitter | Distribuir reintentos |
| ✅ Timeout > Retry | No reintentar lo que ya timed out |
| ✅ Circuit breaker en APIs críticas | Fallar rápido |
| ✅ Loguear reintentos | Auditoría y debugging |
| ❌ Reintentar todo | 404 no es transitorio |
| ❌ Reintentos infinitos | Sistema puede colgarse |
| ✅ Fallback razonable | Mejor degradado que caído |

---

## Próximos Pasos

- **[HTTP_CLIENTS.md](HTTP_CLIENTS.md)** - Polly en handlers HTTP
- **[CONFIGURATION.md](CONFIGURATION.md)** - Configurar Polly
