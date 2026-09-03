# 🌐 HTTP Clients - Token Propagation y Reintentos

Documentación de **IHttpClientFactory** con propagación transparente de tokens JWT y reintentos automáticos.

---

## Visión General

Los handlers pueden llamar APIs externas sin saber nada de autenticación. El token del request HTTP entrante se reenvía automáticamente.

```
┌────────────────────────────────────┐
│   Cliente SPA                      │
│   Authorization: Bearer <token>    │
└─────────────┬──────────────────────┘
              │ Request HTTP
              ▼
┌────────────────────────────────────┐
│   Olimpia API (Handler)            │
│   IExternalApiClient.GetAsync(...) │
└─────────┬──────────────────────────┘
          │
          ├─ BearerTokenPropagationHandler
          │  └─ Extrae token del HttpContext
          │
          ├─ PollyRetryHandler
          │  └─ Reintentos automáticos (3x)
          │
          ▼
┌────────────────────────────────────┐
│   API Externa                      │
│   Authorization: Bearer <token>    │
└────────────────────────────────────┘
```

---

## 1. Interfaz IExternalApiClient

Contrato en **Application** (neutral de implementación):

```csharp
// Olimpia.Application/Contracts/IExternalApiClient.cs
namespace Olimpia.Application.Contracts;

public interface IExternalApiClient
{
    /// <summary>
    /// GET a una API externa.
    /// El token del request HTTP entrante se propaga automáticamente.
    /// </summary>
    Task<TResponse?> GetAsync<TResponse>(
        string clientName,
        string relativeUri,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// POST a una API externa.
    /// </summary>
    Task<TResponse?> PostAsync<TRequest, TResponse>(
        string clientName,
        string relativeUri,
        TRequest payload,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// PUT a una API externa.
    /// </summary>
    Task<TResponse?> PutAsync<TRequest, TResponse>(
        string clientName,
        string relativeUri,
        TRequest payload,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// DELETE a una API externa.
    /// </summary>
    Task DeleteAsync(
        string clientName,
        string relativeUri,
        CancellationToken cancellationToken = default);
}
```

---

## 2. Implementación ExternalApiClient

En **Infrastructure**:

```csharp
// Olimpia.Infrastructure/Http/ExternalApiClient.cs
using Olimpia.Application.Contracts;

public sealed class ExternalApiClient : IExternalApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ExternalApiClient> _logger;

    public ExternalApiClient(
        IHttpClientFactory httpClientFactory,
        ILogger<ExternalApiClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    // Método generado por GitHub Copilot
    public async Task<TResponse?> GetAsync<TResponse>(
        string clientName,
        string relativeUri,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient(clientName);
            var response = await client.GetAsync(relativeUri, cancellationToken);
            
            response.EnsureSuccessStatusCode();
            
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<TResponse>(json);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error en GET {ClientName}/{RelativeUri}", clientName, relativeUri);
            throw;
        }
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(
        string clientName,
        string relativeUri,
        TRequest payload,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient(clientName);
            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");
            
            var response = await client.PostAsync(relativeUri, content, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<TResponse>(json);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error en POST {ClientName}/{RelativeUri}", clientName, relativeUri);
            throw;
        }
    }

    public async Task<TResponse?> PutAsync<TRequest, TResponse>(
        string clientName,
        string relativeUri,
        TRequest payload,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient(clientName);
            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");
            
            var response = await client.PutAsync(relativeUri, content, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<TResponse>(json);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error en PUT {ClientName}/{RelativeUri}", clientName, relativeUri);
            throw;
        }
    }

    public async Task DeleteAsync(
        string clientName,
        string relativeUri,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient(clientName);
            var response = await client.DeleteAsync(relativeUri, cancellationToken);
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error en DELETE {ClientName}/{RelativeUri}", clientName, relativeUri);
            throw;
        }
    }
    // Fin código generado por GitHub Copilot
}
```

---

## 3. BearerTokenPropagationHandler

Extrae el token del HttpContext actual y lo reenvía automáticamente.

```csharp
// Olimpia.Infrastructure/Http/BearerTokenPropagationHandler.cs
using Microsoft.AspNetCore.Authentication;

public sealed class BearerTokenPropagationHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<BearerTokenPropagationHandler> _logger;

    public BearerTokenPropagationHandler(
        IHttpContextAccessor httpContextAccessor,
        ILogger<BearerTokenPropagationHandler> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    // Método generado por GitHub Copilot
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        
        if (httpContext != null)
        {
            // Intentar obtener token desde HttpContext
            var token = await httpContext.GetTokenAsync("access_token");
            
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = 
                    new AuthenticationHeaderValue("Bearer", token);
                
                _logger.LogDebug("Token propagado a {RequestUri}", request.RequestUri);
            }
            else
            {
                _logger.LogDebug("No hay token en HttpContext para {RequestUri}", request.RequestUri);
            }
        }
        else
        {
            _logger.LogDebug("No hay HttpContext activo (background job)");
        }

        return await base.SendAsync(request, cancellationToken);
    }
    // Fin código generado por GitHub Copilot
}
```

---

## 4. PollyRetryHandler

Reintentos automáticos con backoff exponencial (Polly v8).

```csharp
// Olimpia.Infrastructure/Http/PollyRetryHandler.cs
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

public sealed class PollyRetryHandler : DelegatingHandler
{
    private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy;
    private readonly IAsyncPolicy<HttpResponseMessage> _circuitBreakerPolicy;
    private readonly IAsyncPolicy<HttpResponseMessage> _combinedPolicy;
    private readonly ILogger<PollyRetryHandler> _logger;
    private readonly PollyConfiguration _config;

    public PollyRetryHandler(
        IOptions<PollyConfiguration> config,
        ILogger<PollyRetryHandler> logger)
    {
        _logger = logger;
        _config = config.Value;

        // Política de reintentos
        _retryPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .Or<TimeoutException>()
            .OrResult<HttpResponseMessage>(r => IsTransient(r.StatusCode))
            .WaitAndRetryAsync(
                retryCount: _config.MaxRetryAttempts,
                sleepDurationProvider: retryAttempt =>
                    TimeSpan.FromMilliseconds(
                        _config.InitialDelayMs * Math.Pow(2, retryAttempt - 1)),
                onRetry: (outcome, timespan, attemptNumber, context) =>
                {
                    _logger.LogWarning(
                        "Reintento HTTP {AttemptNumber} de {MaxAttempts} tras {DelayMs}ms. " +
                        "StatusCode: {StatusCode}",
                        attemptNumber,
                        _config.MaxRetryAttempts,
                        (long)timespan.TotalMilliseconds,
                        outcome.Result?.StatusCode ?? System.Net.HttpStatusCode.RequestTimeout);
                });

        // Política de circuit breaker (opcional)
        _circuitBreakerPolicy = Policy
            .Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(r => r.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (outcome, duration) =>
                {
                    _logger.LogError(
                        "Circuit breaker abierto por {Duration}s. Excepciones consecutivas alcanzadas.",
                        duration.TotalSeconds);
                });

        // Combinar políticas
        _combinedPolicy = Policy.WrapAsync(_retryPolicy, _circuitBreakerPolicy);
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (!_config.RetryEnabled)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        return await _combinedPolicy.ExecuteAsync(
            async ct => await base.SendAsync(request, ct),
            cancellationToken);
    }

    private static bool IsTransient(System.Net.HttpStatusCode statusCode) =>
        statusCode switch
        {
            System.Net.HttpStatusCode.RequestTimeout => true,                // 408
            System.Net.HttpStatusCode.TooManyRequests => true,               // 429
            System.Net.HttpStatusCode.InternalServerError => true,           // 500
            System.Net.HttpStatusCode.BadGateway => true,                    // 502
            System.Net.HttpStatusCode.ServiceUnavailable => true,            // 503
            System.Net.HttpStatusCode.GatewayTimeout => true,                // 504
            _ => false
        };
}

public sealed class PollyConfiguration
{
    public bool RetryEnabled { get; set; } = true;
    public int MaxRetryAttempts { get; set; } = 3;
    public int InitialDelayMs { get; set; } = 200;
}
```

---

## 5. Registración en DependencyInjection.cs

```csharp
// Olimpia.Infrastructure/DependencyInjection.cs
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 1. Registrar handlers
        services.AddScoped<BearerTokenPropagationHandler>();
        services.AddScoped<PollyRetryHandler>();

        // 2. Configurar Polly
        services.Configure<PollyConfiguration>(configuration.GetSection("HttpClient"));

        // 3. Registrar HttpClients para cada API externa
        // Orden importa: PollyRetryHandler PRIMERO (decorador más externo)
        services
            .AddHttpClient<IExternalApiClient, ExternalApiClient>("CatalogoService")
            .ConfigureHttpClient(client =>
            {
                var baseUrl = configuration["ExternalApis:CatalogoService:BaseUrl"];
                client.BaseAddress = new Uri(baseUrl ?? "https://catalogo.internal/");
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddHttpMessageHandler<PollyRetryHandler>()
            .AddHttpMessageHandler<BearerTokenPropagationHandler>();

        services
            .AddHttpClient<IExternalApiClient, ExternalApiClient>("NotificacionesService")
            .ConfigureHttpClient(client =>
            {
                var baseUrl = configuration["ExternalApis:NotificacionesService:BaseUrl"];
                client.BaseAddress = new Uri(baseUrl ?? "https://notificaciones.internal/");
                client.Timeout = TimeSpan.FromSeconds(20);
            })
            .AddHttpMessageHandler<PollyRetryHandler>()
            .AddHttpMessageHandler<BearerTokenPropagationHandler>();

        // 4. Registrar IHttpContextAccessor (necesario para token propagation)
        services.AddHttpContextAccessor();

        return services;
    }
}
```

---

## 6. Configuración en appsettings.json

```json
{
  "ExternalApis": {
    "CatalogoService": {
      "BaseUrl": "https://catalogo.company.com"
    },
    "NotificacionesService": {
      "BaseUrl": "https://notificaciones.company.com"
    }
  },
  "HttpClient": {
    "RetryEnabled": true,
    "MaxRetryAttempts": 3,
    "InitialDelayMs": 200
  }
}
```

---

## 7. Uso en Handlers

### GET Simple

```csharp
public sealed class GetProductCatalogHandler : IQueryHandler<GetProductCatalogQuery, CatalogDto>
{
    private readonly IExternalApiClient _externalApiClient;

    public GetProductCatalogHandler(IExternalApiClient externalApiClient)
    {
        _externalApiClient = externalApiClient;
    }

    public async Task<CatalogDto> Handle(GetProductCatalogQuery query, CancellationToken ct)
    {
        // Token del request entrante se propaga automáticamente
        var catalog = await _externalApiClient.GetAsync<CatalogDto>(
            clientName: "CatalogoService",
            relativeUri: $"api/catalog/{query.CategoryId}",
            cancellationToken: ct);

        if (catalog == null)
            throw new KeyNotFoundException("Catálogo no encontrado");

        return catalog;
    }
}
```

### POST con Reintentos Automáticos

```csharp
public sealed class CreateProductWithNotificationHandler : ICommandHandler<CreateProductCommand, int>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IExternalApiClient _externalApiClient;
    private readonly ILogger<CreateProductWithNotificationHandler> _logger;

    public async Task<int> Handle(CreateProductCommand command, CancellationToken ct)
    {
        // 1. Crear producto localmente
        var product = new Product { Name = command.Name, Price = command.Price };
        var id = await _productRepository.AddAsync(product);
        await _unitOfWork.CommitAsync();

        // 2. Notificar a servicio externo (con reintentos automáticos)
        try
        {
            await _externalApiClient.PostAsync<ProductCreatedEvent, object>(
                clientName: "NotificacionesService",
                relativeUri: "api/events/product-created",
                payload: new ProductCreatedEvent { ProductId = id, Name = command.Name },
                cancellationToken: ct);
        }
        catch (HttpRequestException ex)
        {
            // Si después de 3 reintentos falla, loguear y continuar
            // (no fallar la creación del producto)
            _logger.LogError(ex, "Error notificando servicio externo");
        }

        return id;
    }
}
```

---

## 8. Flujo de Llamada HTTP

```
Request entrante con Bearer token
    │
    ▼
[Authorize] middleware valida token
    │
    ▼
Handler llama _externalApiClient.GetAsync("CatalogoService", "api/items")
    │
    ▼
IHttpClientFactory crea cliente HTTP
    │
    ├─ HttpMessageHandler pipeline:
    │  │
    │  ├─> PollyRetryHandler
    │  │   - Prepara para reintentos
    │  │   - Pasa a siguiente handler
    │  │
    │  └─> BearerTokenPropagationHandler
    │      - Extrae token de HttpContext (IHttpContextAccessor)
    │      - Añade: Authorization: Bearer <token>
    │      - Pasa a HttpClientHandler
    │
    ├─ HttpClientHandler envía GET
    │   GET /api/items HTTP/1.1
    │   Host: catalogo.company.com
    │   Authorization: Bearer eyJhbGc...
    │
    ├─ API Externa responde
    │   ├─ 200 OK ──────────────────────> Retorna resultado ✓
    │   ├─ 429 Too Many Requests ──────> Reintento (200ms)
    │   ├─ 500 Internal Server Error ──> Reintento (400ms)
    │   └─ 503 Service Unavailable ────> Reintento (800ms)
    │
    └─ PollyRetryHandler reintentos
       (3 intentos máximo)

Response → Handler → Controller → Cliente
```

---

## 9. Manejo de Errores

```csharp
public sealed class GetProductAnalyticsHandler : IQueryHandler<GetProductAnalyticsQuery, AnalyticsDto>
{
    private readonly IExternalApiClient _externalApiClient;
    private readonly ILogger<GetProductAnalyticsHandler> _logger;

    public async Task<AnalyticsDto> Handle(GetProductAnalyticsQuery query, CancellationToken ct)
    {
        try
        {
            var analytics = await _externalApiClient.GetAsync<AnalyticsDto>(
                "AnalyticsService",
                $"api/analytics/{query.ProductId}",
                ct);

            return analytics ?? new AnalyticsDto { /* defaults */ };
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Analytics no encontrado para producto {ProductId}", query.ProductId);
            return new AnalyticsDto { /* defaults */ };
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            _logger.LogError("Token no válido o expirado para Analytics");
            throw new UnauthorizedAccessException("No autorizado para acceder a Analytics");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
        {
            _logger.LogError("Analytics service no disponible");
            throw new InvalidOperationException("Servicio temporalmente no disponible");
        }
        catch (TaskCanceledException)
        {
            _logger.LogError("Timeout llamando a Analytics después de 3 reintentos");
            throw new TimeoutException("Tiempo de espera agotado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado llamando a Analytics");
            throw;
        }
    }
}
```

---

## 10. Testing

```csharp
[TestClass]
public sealed class ExternalApiClientTests
{
    private Mock<IHttpClientFactory> _httpClientFactoryMock;
    private ExternalApiClient _client;

    [TestInitialize]
    public void Setup()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        var logger = new Mock<ILogger<ExternalApiClient>>();
        _client = new ExternalApiClient(_httpClientFactoryMock.Object, logger.Object);
    }

    [TestMethod]
    public async Task GetAsync_ReturnsDeserializedResponse()
    {
        // Arrange
        var httpClientMock = new Mock<HttpClient>();
        var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(new { Id = 1, Name = "Test" }))
        };

        _httpClientFactoryMock
            .Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns(httpClientMock.Object);

        // Act & Assert
        var result = await _client.GetAsync<dynamic>("TestService", "api/test");
        Assert.IsNotNull(result);
    }
}
```

---

## Próximos Pasos

- **[AUTHENTICATION.md](AUTHENTICATION.md)** - JWT y token validation
- **[RESILIENCE.md](RESILIENCE.md)** - Detalles de Polly
