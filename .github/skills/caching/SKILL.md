---
name: caching
description: "Implementación de caché distribuida con Redis usando el patrón cache-aside en Olimpia. IDistributedCache, invalidación, TTL y configuración."
---

# Caching con Redis — Olimpia

Patrón cache-aside (lazy loading) con `IDistributedCache` para la arquitectura Olimpia.

## Configuración

**appsettings.json:**
```json
{
  "RedisCache": {
    "Enabled": true,
    "ConnectionString": "localhost:6379",
    "InstanceName": "OlimpiaPrefix:"
  }
}
```

**Program.cs:**
```csharp
if (builder.Configuration.GetValue<bool>("RedisCache:Enabled"))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = builder.Configuration["RedisCache:ConnectionString"];
        options.InstanceName  = builder.Configuration["RedisCache:InstanceName"];
    });
}
```

## Patrón Cache-Aside en Query Handlers

```csharp
public sealed class GetProductHandler : IQueryHandler<GetProductQuery, ProductDto>
{
    private readonly IProductRepository _productRepository;
    private readonly IDistributedCache _cache;

    public GetProductHandler(IProductRepository productRepository, IDistributedCache cache)
    {
        _productRepository = productRepository;
        _cache             = cache;
    }

    public async Task<ProductDto> Handle(GetProductQuery query, CancellationToken cancellationToken)
    {
        var cacheKey = $"product:{query.Id}";

        var cached = await _cache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cached))
            return JsonSerializer.Deserialize<ProductDto>(cached)!;

        var product = await _productRepository.GetByIdAsync(query.Id)
            ?? throw new KeyNotFoundException($"Product {query.Id} not found.");

        var dto = new ProductDto(product.Id, product.Name, product.Price);

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
        };
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(dto), options, cancellationToken);

        return dto;
    }
}
```

## Invalidación en Command Handlers

Invalidar **después** del `CommitAsync`, nunca antes. Invalidar todas las claves relacionadas.

```csharp
public sealed class UpdateProductHandler : ICommandHandler<UpdateProductCommand, int>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDistributedCache _cache;

    public UpdateProductHandler(IProductRepository productRepository, IUnitOfWork unitOfWork, IDistributedCache cache)
    {
        _productRepository = productRepository;
        _unitOfWork        = unitOfWork;
        _cache             = cache;
    }

    public async Task<int> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(command.Id)
            ?? throw new KeyNotFoundException($"Product {command.Id} not found.");

        product.Name  = command.Name;
        product.Price = command.Price;

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var affected = await _productRepository.UpdateAsync(product);
            await _unitOfWork.CommitAsync();

            await _cache.RemoveAsync($"product:{command.Id}", cancellationToken);
            await _cache.RemoveAsync("products:all", cancellationToken);

            return affected;
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }
}
```

## Naming de claves de caché

| Patrón | Ejemplo |
|--------|---------|
| `{entity}:{id}` | `product:42` |
| `{entity}:all` | `products:all` |
| `{entity}:list:{filter}` | `products:list:category:5` |
| `{entity}:page:{n}:{size}` | `products:page:1:20` |

## Estrategias de expiración

```csharp
new DistributedCacheEntryOptions
{
    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
};

new DistributedCacheEntryOptions
{
    SlidingExpiration = TimeSpan.FromMinutes(10)
};

new DistributedCacheEntryOptions
{
    SlidingExpiration = TimeSpan.FromMinutes(10),
    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
};
```

## Reglas

- Usar `IDistributedCache` — nunca acceder a Redis directamente.
- Invalidar caché después del `CommitAsync`, nunca antes.
- Invalidar todas las claves relacionadas (individual + listas).
- Si Redis no está disponible, la app debe seguir funcionando (graceful degradation).
- No cachear datos sensibles (tokens, passwords, PII).
- Serializar con `System.Text.Json` — no `Newtonsoft.Json`.
