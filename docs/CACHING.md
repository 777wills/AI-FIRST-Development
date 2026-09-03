# 💾 Caching Distribuido - Redis y IDistributedCache

Documentación de caché distribuida usando **Redis** y la abstracción estándar **IDistributedCache** de .NET.

---

## Ventajas del Caching

- ⚡ Reducir latencia (caché es más rápido que BD)
- 📊 Disminuir carga en base de datos
- 💰 Menor costo de infraestructura
- 🌍 Escalabilidad horizontal (Redis compartido)

---

## 1. Configuración

### appsettings.json

```json
{
  "RedisCache": {
    "Enabled": true,
    "ConnectionString": "localhost:6379,abortConnect=false",
    "InstanceName": "OlimpiaPrefix_",
    "DefaultExpirationMinutes": 60
  }
}
```

### DependencyInjection.cs

```csharp
// Olimpia.Infrastructure/DependencyInjection.cs
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var cacheConfig = configuration.GetSection("RedisCache");
        var enabled = cacheConfig.GetValue<bool>("Enabled", false);

        if (enabled)
        {
            // Redis habilitado
            var connectionString = cacheConfig.GetValue<string>("ConnectionString");
            var instanceName = cacheConfig.GetValue<string>("InstanceName", "OlimpiaPrefix_");

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = connectionString;
                options.InstanceName = instanceName;
            });

            // Opcionalmente: decorador de caché
            services.AddScoped<CacheDecoratorBehavior>();
        }
        else
        {
            // Fallback: memoria local (no distribuida, solo desarrollo)
            services.AddDistributedMemoryCache();
        }

        return services;
    }
}
```

### Program.cs

```csharp
var builder = WebApplicationBuilder.CreateBuilder(args);

// Registrar servicios
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();
app.Run();
```

---

## 2. Patrón Cache-Aside (Lazy Loading)

Implementar en Query Handlers: **obtener del caché, si no existe obtener de BD**.

```csharp
// Inicio código generado por GitHub Copilot
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

public record GetProductQuery(int Id) : IQuery<ProductDto>;

public sealed class GetProductHandler : IQueryHandler<GetProductQuery, ProductDto>
{
    private readonly IProductRepository _repository;
    private readonly IDistributedCache _cache;
    private readonly ILogger<GetProductHandler> _logger;

    public GetProductHandler(
        IProductRepository repository,
        IDistributedCache cache,
        ILogger<GetProductHandler> logger)
    {
        _repository = repository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<ProductDto> Handle(GetProductQuery query, CancellationToken ct)
    {
        var cacheKey = $"product:{query.Id}";

        // 1. Intentar obtener del caché
        var cachedData = await _cache.GetStringAsync(cacheKey, ct);
        if (!string.IsNullOrEmpty(cachedData))
        {
            _logger.LogInformation("Cache HIT para {CacheKey}", cacheKey);
            return JsonSerializer.Deserialize<ProductDto>(cachedData)!;
        }

        _logger.LogInformation("Cache MISS para {CacheKey}", cacheKey);

        // 2. Obtener de base de datos
        var product = await _repository.GetByIdAsync(query.Id);
        if (product == null)
            throw new KeyNotFoundException($"Producto {query.Id} no encontrado");

        // 3. Mapear a DTO
        var dto = new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock
        };

        // 4. Guardar en caché
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
        };

        await _cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(dto),
            options,
            ct);

        _logger.LogInformation("Producto {ProductId} guardado en caché", product.Id);

        return dto;
    }
}
// Fin código generado por GitHub Copilot
```

---

## 3. Estrategias de Expiración

### AbsoluteExpirationRelativeToNow

```csharp
// Expira exactamente 30 minutos después de guardar
var options = new DistributedCacheEntryOptions
{
    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
};

await _cache.SetStringAsync("key", "value", options);
// Expira a: DateTimeOffset.UtcNow.AddMinutes(30)
```

### AbsoluteExpiration

```csharp
// Expira a una fecha/hora específica
var options = new DistributedCacheEntryOptions
{
    AbsoluteExpiration = DateTimeOffset.UtcNow.AddHours(2)
};

await _cache.SetStringAsync("key", "value", options);
```

### SlidingExpiration

```csharp
// Expira si no se accede por 10 minutos (se renueva con cada acceso)
var options = new DistributedCacheEntryOptions
{
    SlidingExpiration = TimeSpan.FromMinutes(10)
};

await _cache.SetStringAsync("key", "value", options);

// Cada vez que se lee, la expiración se renueva 10 minutos más
```

### Combinada

```csharp
var options = new DistributedCacheEntryOptions
{
    // Máximo 2 horas desde creación
    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2),
    
    // Pero también expira después de 30 min sin acceso
    SlidingExpiration = TimeSpan.FromMinutes(30)
};
```

---

## 4. Invalidación de Caché en Commands

Cuando se modifica un recurso, invalidar su caché.

```csharp
// Método generado por GitHub Copilot
public sealed class UpdateProductCommandHandler : ICommandHandler<UpdateProductCommand, bool>
{
    private readonly IProductRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDistributedCache _cache;
    private readonly ILogger<UpdateProductCommandHandler> _logger;

    public async Task<bool> Handle(UpdateProductCommand command, CancellationToken ct)
    {
        // 1. Obtener producto actual
        var product = await _repository.GetByIdAsync(command.Id);
        if (product == null)
            throw new KeyNotFoundException($"Producto {command.Id} no encontrado");

        // 2. Actualizar entidad
        product.Name = command.Name;
        product.Description = command.Description;
        product.Price = command.Price;

        // 3. Persistir
        var result = await _repository.UpdateAsync(product);
        await _unitOfWork.CommitAsync();

        // 4. Invalidar caché
        var cacheKey = $"product:{command.Id}";
        await _cache.RemoveAsync(cacheKey, ct);
        
        _logger.LogInformation("Caché invalidado para {CacheKey}", cacheKey);

        return result;
    }
}
// Fin código generado por GitHub Copilot
```

### Invalidación en Cascada

```csharp
public sealed class DeleteProductCommandHandler : ICommandHandler<DeleteProductCommand, bool>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDistributedCache _cache;

    public async Task<bool> Handle(DeleteProductCommand command, CancellationToken ct)
    {
        var product = await _productRepository.GetByIdAsync(command.Id);
        if (product == null)
            throw new KeyNotFoundException();

        var result = await _productRepository.DeleteAsync(command.Id);
        await _unitOfWork.CommitAsync();

        // Invalidar caches relacionados
        await _cache.RemoveAsync($"product:{command.Id}", ct);
        await _cache.RemoveAsync($"products:by-category:{product.CategoryId}", ct);
        await _cache.RemoveAsync("products:all", ct);

        return result;
    }
}
```

---

## 5. Métodos de IDistributedCache

### Obtener Datos

```csharp
// Obtener como bytes
byte[] data = await _cache.GetAsync("key");

// Obtener como string
string value = await _cache.GetStringAsync("key");

// Obtener y deserializar
var json = await _cache.GetStringAsync("key");
var obj = JsonSerializer.Deserialize<MyClass>(json);
```

### Guardar Datos

```csharp
// Guardar bytes
await _cache.SetAsync("key", bytes);
await _cache.SetAsync("key", bytes, new DistributedCacheEntryOptions { ... });

// Guardar string
await _cache.SetStringAsync("key", "value");
await _cache.SetStringAsync("key", JsonSerializer.Serialize(obj), options);
```

### Actualizar Expiración

```csharp
// Renovar la expiración sin cambiar el valor
await _cache.RefreshAsync("key");
```

### Eliminar Datos

```csharp
// Eliminar una clave
await _cache.RemoveAsync("key");

// Para eliminar múltiples, hacerlo en loop
var keys = new[] { "key1", "key2", "key3" };
foreach (var key in keys)
{
    await _cache.RemoveAsync(key);
}
```

---

## 6. Caché de Listas

```csharp
public sealed class ListProductsHandler : IQueryHandler<ListProductsQuery, PagedResult<ProductDto>>
{
    private readonly IProductRepository _repository;
    private readonly IDistributedCache _cache;

    public async Task<PagedResult<ProductDto>> Handle(ListProductsQuery query, CancellationToken ct)
    {
        // Construir clave única basada en parámetros
        var cacheKey = $"products:page:{query.PageNumber}:size:{query.PageSize}";
        if (!string.IsNullOrEmpty(query.Filter))
            cacheKey += $":filter:{query.Filter}";

        // Intentar obtener del caché
        var cached = await _cache.GetStringAsync(cacheKey, ct);
        if (!string.IsNullOrEmpty(cached))
        {
            return JsonSerializer.Deserialize<PagedResult<ProductDto>>(cached)!;
        }

        // Obtener de BD
        var skip = (query.PageNumber - 1) * query.PageSize;
        var products = await _repository.GetPagedAsync(skip, query.PageSize, query.Filter);
        var total = await _repository.CountAsync(query.Filter);

        var result = new PagedResult<ProductDto>
        {
            Items = products.Select(MapToDto).ToList(),
            Total = total,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };

        // Guardar con expiración más corta
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        };
        
        await _cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(result),
            options,
            ct);

        return result;
    }
}
```

---

## 7. Caché Tag-Based (Simular)

Para invalidar múltiples claves relacionadas:

```csharp
public static class CacheKeyPatterns
{
    public const string ProductPrefix = "product:";
    public const string CategoryPrefix = "category:";
    public const string ProductsByCategoryPrefix = "products:category:";
    public const string AllProductsKey = "products:all";

    public static string Product(int id) => $"{ProductPrefix}{id}";
    public static string Category(int id) => $"{CategoryPrefix}{id}";
    public static string ProductsByCategory(int categoryId) => $"{ProductsByCategoryPrefix}{categoryId}";
}

// Invalidar todos los productos de una categoría
public sealed class DeleteProductCommandHandler : ICommandHandler<DeleteProductCommand, bool>
{
    private readonly IProductRepository _repository;
    private readonly IDistributedCache _cache;

    public async Task<bool> Handle(DeleteProductCommand command, CancellationToken ct)
    {
        var product = await _repository.GetByIdAsync(command.Id);
        if (product == null)
            throw new KeyNotFoundException();

        await _repository.DeleteAsync(command.Id);

        // Invalidar caches relacionados
        await _cache.RemoveAsync(CacheKeyPatterns.Product(command.Id), ct);
        await _cache.RemoveAsync(
            CacheKeyPatterns.ProductsByCategory(product.CategoryId), ct);
        await _cache.RemoveAsync(CacheKeyPatterns.AllProductsKey, ct);

        return true;
    }
}
```

---

## 8. Fallback a Memoria Local (Testing)

En desarrollo sin Redis:

```csharp
"RedisCache": {
  "Enabled": false  // Fallback a MemoryCache
}
```

`IDistributedCache` se comportará como `MemoryCache` (no distribuida entre instancias).

---

## 9. Testing

### Mock de IDistributedCache

```csharp
[TestClass]
public sealed class GetProductHandlerTests
{
    private Mock<IDistributedCache> _cacheMock;
    private Mock<IProductRepository> _repositoryMock;
    private GetProductHandler _handler;

    [TestInitialize]
    public void Setup()
    {
        _cacheMock = new Mock<IDistributedCache>();
        _repositoryMock = new Mock<IProductRepository>();
        var logger = new Mock<ILogger<GetProductHandler>>();

        _handler = new GetProductHandler(_repositoryMock.Object, _cacheMock.Object, logger.Object);
    }

    [TestMethod]
    public async Task Handle_CacheHit_ReturnsCachedData()
    {
        // Arrange
        var productDto = new ProductDto { Id = 1, Name = "Test" };
        var json = JsonSerializer.Serialize(productDto);
        
        _cacheMock
            .Setup(x => x.GetStringAsync("product:1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(json);

        // Act
        var result = await _handler.Handle(new GetProductQuery(1), CancellationToken.None);

        // Assert
        Assert.AreEqual("Test", result.Name);
        _cacheMock.Verify(x => x.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()));
        _repositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [TestMethod]
    public async Task Handle_CacheMiss_FetchesFromRepositoryAndCaches()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "Test", Price = 100 };
        
        _cacheMock
            .Setup(x => x.GetStringAsync("product:1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(product);

        // Act
        var result = await _handler.Handle(new GetProductQuery(1), CancellationToken.None);

        // Assert
        Assert.AreEqual("Test", result.Name);
        _repositoryMock.Verify(x => x.GetByIdAsync(1));
        _cacheMock.Verify(x => x.SetStringAsync(
            "product:1",
            It.IsAny<string>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()));
    }
}
```

---

## 10. Monitoreo y Métricas

```csharp
public sealed class CacheMetrics
{
    public long Hits { get; set; }
    public long Misses { get; set; }
    public double HitRate => Hits + Misses > 0 ? (double)Hits / (Hits + Misses) : 0;
}

public sealed class MonitoredDistributedCache : IDistributedCache
{
    private readonly IDistributedCache _inner;
    private readonly CacheMetrics _metrics = new();

    public async Task<byte[]?> GetAsync(string key, CancellationToken token = default)
    {
        var result = await _inner.GetAsync(key, token);
        if (result != null)
            Interlocked.Increment(ref _metrics.Hits);
        else
            Interlocked.Increment(ref _metrics.Misses);
        return result;
    }

    // Implementar otros métodos delegando a _inner
    // ...

    public CacheMetrics GetMetrics() => _metrics;
}
```

---

## Próximos Pasos

- **[DEPLOYMENT.md](DEPLOYMENT.md)** - Docker y Kubernetes con Redis
- **[CONFIGURATION.md](CONFIGURATION.md)** - Variables de entorno para Redis
