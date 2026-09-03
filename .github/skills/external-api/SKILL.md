---
name: external-api
description: "Llamadas a APIs externas con IExternalApiClient, token relay transparente (BearerTokenPropagationHandler), resiliencia con Polly y configuración por entorno."
---

# Skill: APIs Externas con Token Relay y Resiliencia

Patrón para consumir APIs externas desde Handlers con propagación automática del token JWT y reintentos con Polly.

---

## Arquitectura

```
Controller → Handler → IExternalApiClient → IHttpClientFactory → BearerTokenPropagationHandler → API Externa
                                                                        ↑
                                                              Token del request entrante
```

- El token JWT del request HTTP entrante se reenvía automáticamente a la API externa.
- No se requiere gestión manual de tokens en Handlers.

---

## IExternalApiClient — Contrato en Application

**Ubicación:** `src/Olimpia.Application/Contracts/IExternalApiClient.cs`

```csharp
namespace Olimpia.Application.Contracts;

public interface IExternalApiClient
{
    Task<T?> GetAsync<T>(string serviceName, string path, CancellationToken ct = default);
    Task<TResponse?> PostAsync<TRequest, TResponse>(string serviceName, string path, TRequest body, CancellationToken ct = default);
    Task<TResponse?> PutAsync<TRequest, TResponse>(string serviceName, string path, TRequest body, CancellationToken ct = default);
    Task DeleteAsync(string serviceName, string path, CancellationToken ct = default);
}
```

- `serviceName`: nombre lógico del servicio (ej. `"CatalogoService"`), mapeado a URL base en configuración.
- `path`: ruta relativa del endpoint (ej. `"api/items/42"`).

---

## Uso en Handlers

```csharp
using Cortex.Mediator.Queries;
using Olimpia.Application.Contracts;

namespace Olimpia.Application.Catalogs.Queries.GetCatalog;

public sealed class GetCatalogHandler : IQueryHandler<GetCatalogQuery, CatalogDto>
{
    private readonly IExternalApiClient _client;

    public GetCatalogHandler(IExternalApiClient client) => _client = client;

    public async Task<CatalogDto> Handle(GetCatalogQuery query, CancellationToken ct)
    {
        var catalog = await _client.GetAsync<CatalogDto>("CatalogoService", $"api/catalog/{query.Id}", ct)
            ?? throw new KeyNotFoundException($"Catalog {query.Id} not found.");

        return catalog;
    }
}
```

---

## Token Relay — BearerTokenPropagationHandler

Implementado como `DelegatingHandler` registrado en `IHttpClientFactory`. Extrae el token Bearer del `HttpContext` actual y lo reenvía en el header `Authorization` de la solicitud saliente.

```csharp
public sealed class BearerTokenPropagationHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public BearerTokenPropagationHandler(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = _httpContextAccessor.HttpContext?
            .Request.Headers.Authorization.FirstOrDefault()
            ?.Replace("Bearer ", "");

        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken);
    }
}
```

---

## Polly — Pipeline de resiliencia

Orden del pipeline: **Timeout > CircuitBreaker > Retry**.

```csharp
// Registro en DI (Infrastructure)
builder.Services.AddHttpClient("CatalogoService", client =>
{
    client.BaseAddress = new Uri(configuration["ExternalApis:CatalogoService:BaseUrl"]!);
})
.AddHttpMessageHandler<BearerTokenPropagationHandler>()
.AddResilienceHandler("standard", builder =>
{
    // Timeout: 30s máximo
    builder.AddTimeout(TimeSpan.FromSeconds(30));

    // Circuit Breaker: abre tras 50% fallos en ventana de 30s, cierra tras 60s
    builder.AddCircuitBreaker(new CircuitBreakerStrategyOptions<HttpResponseMessage>
    {
        FailureRatio = 0.5,
        SamplingDuration = TimeSpan.FromSeconds(30),
        MinimumThroughput = 10,
        BreakDuration = TimeSpan.FromSeconds(60)
    });

    // Retry: 3 intentos con backoff exponencial
    builder.AddRetry(new HttpRetryStrategyOptions
    {
        MaxRetryAttempts = 3,
        BackoffType = DelayBackoffType.Exponential,
        Delay = TimeSpan.FromMilliseconds(200),
        ShouldHandle = args => ValueTask.FromResult(
            args.Outcome.Result?.StatusCode is
                HttpStatusCode.RequestTimeout or        // 408
                HttpStatusCode.TooManyRequests or       // 429
                HttpStatusCode.InternalServerError or   // 500
                HttpStatusCode.BadGateway or            // 502
                HttpStatusCode.ServiceUnavailable or    // 503
                HttpStatusCode.GatewayTimeout           // 504
        )
    });
});
```

---

## Configuración por entorno

**appsettings.json:**
```json
{
  "ExternalApis": {
    "CatalogoService": {
      "BaseUrl": "https://catalogo-api.example.com"
    },
    "InventarioService": {
      "BaseUrl": "https://inventario-api.example.com"
    }
  }
}
```

**Variables de entorno (contenedores):**
```
ExternalApis__CatalogoService__BaseUrl=https://catalogo-api.production.com
ExternalApis__InventarioService__BaseUrl=https://inventario-api.production.com
```

---

## Reglas

- **Siempre** inyectar `IExternalApiClient` en Handlers — nunca `HttpClient` directamente.
- El token se propaga automáticamente — no pasar tokens manualmente.
- Si el servicio externo falla temporalmente, Polly reintenta automáticamente.
- No crear `HttpClient` con `new` — siempre vía `IHttpClientFactory`.
- Para agregar un nuevo servicio externo:
  1. Agregar URL en `appsettings.json` bajo `ExternalApis:{ServiceName}:BaseUrl`.
  2. Registrar `HttpClient` con nombre en `DependencyInjection.cs` de Infrastructure.
  3. Consumir desde Handler con `_client.GetAsync<T>("ServiceName", "path", ct)`.
