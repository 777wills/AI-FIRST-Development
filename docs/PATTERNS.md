# 🔄 Patrones Implementados - Olimpia API

Esta documento describe los patrones de diseño implementados en la solución Olimpia.

---

## 1. CQRS con Cortex.Mediator

**CQRS** (Command Query Responsibility Segregation) separa operaciones de lectura (Queries) de escritura (Commands).

### ¿Por qué CQRS?

- ✅ Claridad: es obvio si una operación modifica datos
- ✅ Escalabilidad: cada tipo puede optimizarse diferente
- ✅ Testing: handlers más fáciles de testear
- ✅ Futura CQRS asíncrona: solo cambiar dispatcher

### Cortex.Mediator vs MediatR

| Aspecto | Cortex.Mediator | MediatR |
|--------|-----------------|---------|
| Método | `SendAsync(request)` | `Send(request)` |
| Parámetro | `CancellationToken` automático | Opcional |
| Rendimiento | Más rápido (sin Reflection pesada) | Un poco más lento |
| Complejidad | Más simple, menos mágico | Más configuración |

---

## 1.1 Commands (Escritura de Datos)

Un **Command** es una instrucción que **modifica estado**. Siempre retorna un valor.

### Estructura

```csharp
// 1. Definir el Command como record
using Cortex.Mediator.Commands;

public record CreateProductCommand(string Name, string Description, decimal Price, int Stock)
    : ICommand<int>;  // <-- Retorna un int (Id del producto creado)

// 2. Definir el Handler
public sealed class CreateProductHandler : ICommandHandler<CreateProductCommand, int>
{
    private readonly IProductRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductHandler(IProductRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    // Método generado por GitHub Copilot
    public async Task<int> Handle(CreateProductCommand command, CancellationToken cancellationToken = default)
    {
        // 1. Validar (ya pasó FluentValidation)
        // 2. Construir entidad
        var product = new global::Olimpia.Domain.Entities.Product
        {
            Name = command.Name,
            Description = command.Description,
            Price = command.Price,
            Stock = command.Stock
        };

        // 3. Persistir
        var id = await _repository.AddAsync(product);
        await _unitOfWork.CommitAsync();

        return id;
    }
}

// 3. Registrar en DependencyInjection.cs
public static IServiceCollection AddApplication(this IServiceCollection services)
{
    // Cortex.Mediator auto-descubre todos los handlers
    services.AddCortexMediator(new[] { typeof(DependencyInjection) });
    return services;
}

// 4. Usar en Controller
[MapToApiVersion("1.0")]
[HttpPost]
public async Task<IActionResult> Create(CreateProductCommand command, CancellationToken ct)
{
    try
    {
        var id = await _mediator.SendAsync(command, ct);
        return Created($"api/v1/products/{id}", new { Id = id });
    }
    catch (InvalidOperationException ex)
    {
        return BadRequest(new { Error = ex.Message });
    }
}
```

### Validación en Commands

```csharp
// Definir validator (un validador por Command)
public sealed class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio")
            .MaximumLength(100).WithMessage("Máximo 100 caracteres");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("El precio debe ser mayor a 0");

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("El stock no puede ser negativo");
    }
}

// Registrar en DependencyInjection.cs
services.AddValidatorsFromAssembly(typeof(CreateProductValidator).Assembly);

// Ejecutar automáticamente antes del handler
// (Cortex.Mediator valida antes de invocar Handle)
```

---

## 1.2 Queries (Lectura de Datos)

Una **Query** es una solicitud que **solo lee datos** (sin efectos secundarios). Siempre retorna un valor.

### Estructura

```csharp
// 1. Definir la Query
using Cortex.Mediator.Queries;

public record GetProductQuery(int Id) : IQuery<ProductDto>;

// 2. Definir el Handler
public sealed class GetProductHandler : IQueryHandler<GetProductQuery, ProductDto>
{
    private readonly IProductRepository _repository;
    private readonly IDistributedCache _cache;

    public GetProductHandler(IProductRepository repository, IDistributedCache cache)
    {
        _repository = repository;
        _cache = cache;
    }

    // Método generado por GitHub Copilot
    public async Task<ProductDto> Handle(GetProductQuery query, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"product:{query.Id}";

        // 1. Intentar obtener del caché
        var cached = await _cache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cached))
        {
            return JsonSerializer.Deserialize<ProductDto>(cached)!;
        }

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
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(dto), options, cancellationToken);

        return dto;
    }
}

// 3. Registrar validator (opcional para Queries)
public sealed class GetProductValidator : AbstractValidator<GetProductQuery>
{
    public GetProductValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Id inválido");
    }
}

// 4. Usar en Controller
[MapToApiVersion("1.0")]
[HttpGet("{id}")]
public async Task<IActionResult> GetById(int id, CancellationToken ct)
{
    var query = new GetProductQuery(id);
    var product = await _mediator.SendQueryAsync(query, ct);
    return Ok(product);
}
```

### Queries con Paginación

El proyecto implementa un **patrón de paginación reutilizable** basado en tres capas: tipos del dominio, contratos de Application y envelope de respuesta HTTP. Cualquier feature que necesite un listado paginado sigue este mismo patrón.

#### Paso 1 — Tipos base en Application/Common/

```csharp
// Application/Common/Pagination/PagedQuery.cs
// Record abstracto base para cualquier query paginada
public abstract record PagedQuery(
    int PageNumber = 1,
    int PageSize = 25,
    IReadOnlyList<FilterCriteria>? Filters = null,
    IReadOnlyList<SortCriteria>? SortFields = null);

// Application/Common/Pagination/PagedResult.cs
// Resultado interno del handler (antes de serializar)
public sealed record PagedResult<T>
{
    public IEnumerable<T> Data { get; init; } = [];
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => PageNumber < TotalPages;
    public bool HasPreviousPage => PageNumber > 1;

    public static PagedResult<T> Create(IEnumerable<T> data, int pageNumber, int pageSize, int totalCount) => ...
}

// Application/Common/Responses/PagedEnvelope.cs
// Envelope estándar para la respuesta HTTP: { data, meta }
public sealed record PagedEnvelope<T>(IEnumerable<T> Data, PagedMeta Meta)
{
    public static PagedEnvelope<T> FromPagedResult(PagedResult<T> result) => ...
}
```

**Formato JSON de respuesta:**
```json
{
  "data": [...],
  "meta": {
    "pagination": {
      "currentPage": 1,
      "pageSize": 25,
      "totalCount": 120,
      "totalPages": 5,
      "hasNextPage": true,
      "hasPreviousPage": false
    }
  }
}
```

#### Paso 2 — Definir la Query específica

```csharp
// Heredar de PagedQuery e implementar IQuery<PagedResult<TDto>>
public sealed record GetAllProductsQuery(
    int PageNumber = 1,
    int PageSize = 25,
    IReadOnlyList<FilterCriteria>? Filters = null,
    IReadOnlyList<SortCriteria>? SortFields = null)
    : PagedQuery(PageNumber, PageSize, Filters, SortFields), IQuery<PagedResult<ProductDto>>;
```

#### Paso 3 — Implementar el Handler

```csharp
public sealed class GetAllProductsHandler : IQueryHandler<GetAllProductsQuery, PagedResult<ProductDto>>
{
    private readonly IProductRepository _productRepository;

    public async Task<PagedResult<ProductDto>> Handle(GetAllProductsQuery query, CancellationToken ct)
    {
        var (data, totalCount) = await _productRepository.GetPagedAsync(
            query.PageNumber, query.PageSize, query.Filters, query.SortFields);

        var dtos = data.Select(p => new ProductDto(p.Id, p.Name, p.Description, p.Price, p.Stock, p.CreatedAt, p.UpdatedAt));

        return PagedResult<ProductDto>.Create(dtos, query.PageNumber, query.PageSize, totalCount);
    }
}
```

#### Paso 4 — Exponer en el Controller

```csharp
[MapToApiVersion("1.0")]
[HttpGet]
[Authorize(Policy = "products.read")]
public async Task<IActionResult> GetAll()
{
    var (filters, sortFields, pageNumber, pageSize) = QueryStringFilterParser.Parse(HttpContext.Request.Query);
    var query = new GetAllProductsQuery(pageNumber, pageSize, filters, sortFields);
    var result = await _mediator.SendQueryAsync(query);
    return Ok(PagedEnvelope<ProductDto>.FromPagedResult(result));
}
```

Ver [**DATA_ACCESS.md — Paginación con GetPagedAsync**](DATA_ACCESS.md#paginación-con-getpagedasync) para detalles de la implementación en repositorio.

---

## 2. Repository Pattern con GenericRepository<T>

El **Repository Pattern** abstrae el acceso a datos detrás de una interfaz.

### ¿Por qué?

- ✅ Separación de responsabilidades
- ✅ Testeable (fácil mockear `IProductRepository`)
- ✅ Intercambiable (cambiar BD sin afectar handlers)
- ✅ Reutilizable (CRUD automático)

### Interfaz en Domain

```csharp
// Olimpia.Domain/Repositories/IGenericRepository.cs
namespace Olimpia.Domain.Repositories;

public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<int> AddAsync(T entity);
    Task<bool> UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id);
}

// Interfaz específica del dominio
public interface IProductRepository : IGenericRepository<Product>
{
    Task<Product?> GetByNameAsync(string name);
    Task<IEnumerable<Product>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice);
}

// UnitOfWork para transacciones
public interface IUnitOfWork
{
    SqlConnection DbConnection { get; }
    SqlTransaction? DbTransaction { get; }
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
}
```

### Implementación en Infrastructure

```csharp
// Olimpia.Infrastructure/Persistence/Repositories/GenericRepository.cs
public abstract class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    protected readonly QueryFactory Db;
    protected readonly UnitOfWork UnitOfWork;

    // Convención: nombre tabla = typeof(T).Name + "s"
    // Sobreescribir si es diferente
    protected virtual string TableName => typeof(T).Name + "s";

    protected GenericRepository(QueryFactory db, UnitOfWork unitOfWork)
    {
        Db = db;
        UnitOfWork = unitOfWork;
    }

    // Método generado por GitHub Copilot
    public virtual async Task<T?> GetByIdAsync(int id) =>
        await Db.Query(TableName).Where("Id", id)
            .FirstOrDefaultAsync<T>(transaction: UnitOfWork.DbTransaction);

    public virtual async Task<IEnumerable<T>> GetAllAsync() =>
        await Db.Query(TableName)
            .GetAsync<T>(transaction: UnitOfWork.DbTransaction);

    public virtual async Task<int> AddAsync(T entity)
    {
        var id = await Db.Query(TableName)
            .InsertGetIdAsync<int>(
                BuildInsertData(entity),
                transaction: UnitOfWork.DbTransaction);
        entity.Id = id;
        return id;
    }

    public virtual async Task<bool> UpdateAsync(T entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        var affected = await Db.Query(TableName)
            .Where("Id", entity.Id)
            .UpdateAsync(
                BuildUpdateData(entity),
                transaction: UnitOfWork.DbTransaction);
        return affected > 0;
    }

    public virtual async Task<bool> DeleteAsync(int id)
    {
        var affected = await Db.Query(TableName)
            .Where("Id", id)
            .DeleteAsync(transaction: UnitOfWork.DbTransaction);
        return affected > 0;
    }

    // Helpers privados construyen diccionarios via reflexión
    // Excluyen Id (identidad) y UpdatedAt en inserts
    // Excluyen Id (va en WHERE) y CreatedAt en updates
    protected virtual Dictionary<string, object?> BuildInsertData(T entity) { /* ... */ }
    protected virtual Dictionary<string, object?> BuildUpdateData(T entity) { /* ... */ }
    // Fin código generado por GitHub Copilot
}

// Repositorio concreto
public sealed class ProductRepository : GenericRepository<Product>, IProductRepository
{
    // TableName = "Products" por convención (no hace falta sobreescribir)
    public ProductRepository(QueryFactory db, UnitOfWork unitOfWork) : base(db, unitOfWork) { }

    // Métodos específicos del dominio
    public async Task<Product?> GetByNameAsync(string name) =>
        await Db.Query(TableName).Where("Name", name)
            .FirstOrDefaultAsync<Product>(transaction: UnitOfWork.DbTransaction);

    public async Task<IEnumerable<Product>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice) =>
        await Db.Query(TableName)
            .WhereBetween("Price", minPrice, maxPrice)
            .GetAsync<Product>(transaction: UnitOfWork.DbTransaction);
}
```

### Auto-Registro de Repositorios

**No hace falta registrar manualmente cada repositorio.**

```csharp
// Olimpia.Infrastructure/DependencyInjection.cs
public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
{
    // ... más registros ...
    
    // Auto-registro de repositorios
    services.RegisterRepositories();
    
    return services;
}

// Método interno que usa reflexión
private static void RegisterRepositories(this IServiceCollection services)
{
    // 1. Busca interfaces en Olimpia.Domain.Repositories
    var domainAssembly = typeof(IUnitOfWork).Assembly;
    var repositoryInterfaces = domainAssembly.GetTypes()
        .Where(t => t.IsInterface && typeof(IGenericRepository<>).IsAssignableFrom(t));

    // 2. Para cada interfaz, encuentra la implementación
    foreach (var @interface in repositoryInterfaces)
    {
        var implementation = FindImplementation(@interface);
        if (implementation != null)
        {
            services.AddScoped(@interface, implementation);
        }
    }
}
```

**Beneficio:** Agregar un nuevo repositorio `IOrderRepository` + `OrderRepository` automáticamente se registra.

---

## 3. Unit of Work Pattern

Gestiona una transacción compartida entre múltiples repositorios.

### Flujo

```csharp
// Handler necesita actualizar Producto y Historial en una transacción
public sealed class UpdateProductHandler : ICommandHandler<UpdateProductCommand, bool>
{
    private readonly IProductRepository _productRepo;
    private readonly IHistoryRepository _historyRepo;
    private readonly IUnitOfWork _unitOfWork;

    // Método generado por GitHub Copilot
    public async Task<bool> Handle(UpdateProductCommand command, CancellationToken ct)
    {
        // 1. Iniciar transacción
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // 2. Operar en múltiples repositorios (comparten DbTransaction)
            var product = await _productRepo.GetByIdAsync(command.Id);
            if (product == null)
                throw new KeyNotFoundException("Producto no encontrado");

            product.Name = command.Name;
            await _productRepo.UpdateAsync(product);

            var history = new History { EntityId = command.Id, Action = "UPDATE" };
            await _historyRepo.AddAsync(history);

            // 3. Commit
            await _unitOfWork.CommitAsync();
            return true;
        }
        catch
        {
            // Rollback automático
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }
    // Fin código generado por GitHub Copilot
}
```

---

## 4. Stored Procedure Repository

Para procedimientos almacenados sin SQL crudo.

```csharp
// Interfaz en Domain
public interface IStoredProcedureRepository
{
    Task<int> ExecuteAsync(string procedureName, object? parameters = null);
    Task<IEnumerable<T>> QueryAsync<T>(string procedureName, object? parameters = null);
    Task<T?> QuerySingleAsync<T>(string procedureName, object? parameters = null);
}

// Uso en Handler
public sealed class GetSalesReportHandler : IQueryHandler<GetSalesReportQuery, SalesReportDto>
{
    private readonly IStoredProcedureRepository _sp;

    public async Task<SalesReportDto> Handle(GetSalesReportQuery query, CancellationToken ct)
    {
        // Parámetros simples
        var items = await _sp.QueryAsync<SalesItemDto>("usp_GetSalesByMonth", 
            new { Month = query.Month, Year = query.Year });

        // Parámetros con salida
        var dp = new DynamicParameters();
        dp.Add("@Total", dbType: DbType.Decimal, direction: ParameterDirection.Output);
        await _sp.ExecuteAsync("usp_CalculateTotal", dp);
        var total = dp.Get<decimal>("@Total");

        return new SalesReportDto { Items = items, Total = total };
    }
}
```

---

## 5. View Repository

Para consultas a vistas sin SQL crudo.

```csharp
// Interfaz en Domain
public interface IViewRepository
{
    Task<IEnumerable<T>> QueryAsync<T>(string viewName, object? filters = null);
    Task<T?> QuerySingleAsync<T>(string viewName, object? filters = null);
    Task<IEnumerable<T>> QueryPagedAsync<T>(string viewName, int pageNumber, int pageSize, object? filters = null);
}

// Uso
public sealed class GetProductAnalyticsHandler : IQueryHandler<GetProductAnalyticsQuery, ProductAnalyticsDto>
{
    private readonly IViewRepository _view;

    public async Task<ProductAnalyticsDto> Handle(GetProductAnalyticsQuery query, CancellationToken ct)
    {
        // Consulta simple
        var allProducts = await _view.QueryAsync<ProductStatsDto>("vw_ProductStats");

        // Con filtros dinámicos
        var byCategory = await _view.QueryAsync<ProductStatsDto>("vw_ProductStats", 
            new { CategoryId = query.CategoryId });

        // Con paginación
        var paginated = await _view.QueryPagedAsync<ProductStatsDto>(
            "vw_ProductStats", 
            pageNumber: 1, 
            pageSize: 20,
            filters: new { Status = "Active" });

        return new ProductAnalyticsDto { /* ... */ };
    }
}
```

---

## 6. Decorator Pattern

Ortogonal a CQRS y Repositories. Útil para cross-cutting concerns.

### BearerTokenPropagationHandler

```csharp
// Decora HttpClient para propagar token JWT automáticamente
public sealed class BearerTokenPropagationHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = _httpContextAccessor.HttpContext?.GetTokenAsync("access_token");
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}

// Registración en DependencyInjection.cs
services.AddScoped<BearerTokenPropagationHandler>();
services.AddHttpClient<IExternalApiClient, ExternalApiClient>()
    .AddHttpMessageHandler<BearerTokenPropagationHandler>();
```

### PollyRetryHandler

```csharp
// Decora HttpClient con reintentos automáticos (Polly v8)
public sealed class PollyRetryHandler : DelegatingHandler
{
    private readonly ResiliencePipeline<HttpResponseMessage> _retryPipeline;

    public PollyRetryHandler()
    {
        _retryPipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromMilliseconds(200),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = args => 
                    args is { Outcome.Result.StatusCode: 408 or 429 or 500 or 502 or 503 or 504 } ||
                    args.Outcome.Exception is HttpRequestException or TaskCanceledException
            })
            .Build();
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        return await _retryPipeline.ExecuteAsync(
            async ct => await base.SendAsync(request, ct),
            cancellationToken);
    }
}
```

---

## 7. Convenciones de Código C# (Code Style)

Las convenciones A1–A18 son **obligatorias** en todo el código. La versión normativa para agentes IA vive en [`.github/instructions/csharp-conventions.instructions.md`](../.github/instructions/csharp-conventions.instructions.md). Los analyzers en `.editorconfig` bloquean automáticamente las violaciones mecanizables.

### 7.1 Formato y espaciado (A1)

- Línea en blanco entre definiciones de métodos.
- Línea en blanco entre propiedades con docs XML, inicializadores o lógica.
- `catch` siempre abre llaves en **línea nueva** (nunca en una sola línea).

### 7.2 Idioma (A2)

| Superficie | Idioma |
|------------|--------|
| Identificadores, nombres de tipos, variables | **Inglés** |
| Mensajes de excepción de negocio (`throw new ... ("...")`) | **Español** (usuarios finales) |
| Comentarios de código | **Español** |
| Emojis en comentarios / logs de infraestructura | **Prohibidos** |

### 7.3 Formato de comentarios (A3)

- Comentarios en **línea separada** (nunca al final de una línea de código).
- Iniciar con **mayúscula** y terminar con **punto**.
- Explican el *porqué*, no el *qué* (el identificador ya describe el qué).

### 7.4 Interfaces (A4)

Prefijo **`I`** obligatorio: `IProductRepository`, `IExternalApiClient`.

### 7.5 Abreviaturas (A5)

**1↔1** (una abreviatura = un significado). Minimizar. Tabla aprobada:

| Término | Forma correcta | Incorrecta |
|---------|----------------|------------|
| Identifier | `Id` | `ID`, `Identif` |
| Uniform Resource Locator | `Url` | `URL` |
| HTTP | `Http` | `HTTP` |
| SQL | `Sql` | `SQL` |
| Application Programming Interface | `Api` | `API` |
| Globally Unique Identifier | `Guid` | `GUID` |
| Input/Output | `Io` | `IO` |

Siglas al inicio de identificador en camelCase van **todas en minúscula**: `htmlBody`, `urlBuilder`, `apiClient`.

### 7.6 Clase = archivo (A6)

**Una clase pública por archivo**. El nombre del archivo coincide con el nombre de la clase: `Product.cs` contiene `class Product`.

### 7.7 Nombres de propiedades (A7)

No repetir el nombre de la clase en la propiedad: `Rectangle.Area` (no `Rectangle.RectangleArea`).

| Elemento | Convención | Ejemplo |
|----------|-----------|---------|
| Propiedades públicas | PascalCase | `ProductId` |
| Campos privados | `_camelCase` | `_productRepository` |
| Parámetros / locales | camelCase | `productId` |

### 7.8 Nombres de booleanos (A8)

Prefijos semánticos obligatorios:

- **`Is`** para estado: `IsEnabled`, `IsValid`.
- **`Has`** para posesión: `HasPermission`, `HasItems`.
- **`Can`** para capacidad: `CanEdit`, `CanRetry`.
- **`Should`** para recomendación: `ShouldRetry`.

### 7.9 Nombres de métodos (A9)

Acción + entidad: `ReadFile`, `GetBalance`, `AddAsync`, `UpdateAsync`, `DeleteAsync`. Si el método retorna valor, el nombre debe indicar **qué devuelve** (`GetProductById`, `GetUserSummary`). Sufijo `Async` obligatorio en métodos asíncronos.

### 7.10 Sellado de clases (A10)

Clases concretas deben ser **`sealed`** salvo que estén diseñadas para herencia:

- **`sealed`:** handlers, validators, repositorios concretos, controllers, middleware, tests, DTOs, clases de configuración.
- **No `sealed`:** `abstract class BaseEntity`, `abstract class GenericRepository<T>`, clases base explícitas.
- Static classes: idealmente `static sealed`.

### 7.11 Comparaciones booleanas (A11)

**NUNCA** comparar contra `true`/`false`:

- ❌ `if (isValid == true)` → ✅ `if (isValid)`
- ❌ `if (isValid == false)` → ✅ `if (!isValid)`
- ❌ `if (x?.Y == true)` → ✅ `if (x is { Y: true })`

### 7.12 Liberación de referencias (A12)

- Toda instancia `IDisposable` debe liberarse con `using` o `using var`.
- Tipos que poseen recursos desechables como campos deben implementar `IDisposable` (analyzer **CA2213**).
- **Excepción documentada**: el patrón fire-and-forget de `CustomLogger.SendLogAsync` (ver comentario XML in-code). Cualquier otra excepción debe justificarse con `<remarks>` localmente.

### 7.13 Scope de variables (A13)

- Declarar variables **lo más cerca posible** de su primer uso.
- Preferir `readonly` / `const` donde el valor no cambie.
- No "levantar" variables al inicio del método si solo se usan dentro de un `if`/bloque.

### 7.14 Prohibición de `null` returns (A14)

**Los métodos no deben retornar `null` de forma "sorpresa".** El consumidor no debería necesitar null-check en cada llamado. Patrones sustitutos según semántica:

| Semántica | Patrón |
|-----------|--------|
| "No encontrado" = error | Lanzar `KeyNotFoundException`. Firma `Task<T>` (sin `?`). |
| "No encontrado" = caso válido | `bool TryGetX(..., out T value)` o `Result<T>` / `Option<T>`. |
| Recurso opcional (ej. `BeginScope`) | Devolver null-object (`NullScope.Instance`) en vez de `null`. |

Si la firma usa `T?`, el consumidor **debe** tratar el `null` explícitamente (nunca propagarlo sin pensar).

### 7.15 Especificidad de tipos ante colisiones (A15)

En repositorios, handlers y tests calificar completamente tipos del Domain con `global::` para evitar colisiones:

```csharp
global::Olimpia.Domain.Entities.Product
```

Aplica cuando un namespace local (ej. `Olimpia.Application.Products`) contiene tipos con el mismo nombre que la entidad del Domain.

### 7.16 Casting (A16)

Preferir `is` + pattern matching o `as` + null-check:

- ✅ `if (x is Foo f) { ... }`
- ✅ `var f = x as Foo; if (f is null) { ... }`

Cast explícito `(Foo)x` solo si (1) el contrato estático garantiza la conversión (genéricos con `where T : Foo`) o (2) se documenta con un comentario inmediato anterior justificando el cast y por qué es seguro. Evitar casts que pierdan precisión (`decimal` → `int`, `long` → `int`).

### 7.17 Uso de `var` (A17)

`var` **solo** cuando el tipo es evidente del lado derecho:

- ✅ `var count = 0;`
- ✅ `var list = new List<Product>();`
- ❌ `var result = repo.DoSomething();` → mejor tipo explícito para legibilidad.

### 7.18 Código generado por IA (A18)

- Todo método generado por GitHub Copilot incluye `// Método generado por GitHub Copilot` al inicio.
- Bloques grandes se delimitan con `// Inicio código generado por GitHub Copilot` … `// Fin código generado por GitHub Copilot`.
- Refactorizaciones con `// Inicio refactorización/optimización por GitHub Copilot` … `// Fin …`.
- Regla mantenida por decisión del equipo (ver [`.github/copilot-instructions.md`](../.github/copilot-instructions.md)).

### 7.19 Constructor Dual (Entidades)

Las entidades requieren dos constructores para convivir con Dapper:

```csharp
public sealed class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }

    // Constructor vacío requerido por Dapper.
    public Product() { }

    // Constructor parametrizado para creación explícita.
    public Product(string name, decimal price)
    {
        Name  = name;
        Price = price;
    }
}
```

### 7.20 Formato de archivo

- Indentación: **4 espacios**, CRLF, UTF-8 con BOM.
- Inicializar strings con `= string.Empty` para evitar nulls.

### Referencias cruzadas

- [`docs/API_DOCUMENTATION.md`](API_DOCUMENTATION.md) — Documentación XML obligatoria en Controllers y DTOs expuestos, `ProblemDetails` centralizado.
- [`docs/TESTING.md`](TESTING.md) — Convenciones de tests (MSTest, AAA, `BeEquivalentTo`, `[DataRow]`).
- [`.github/instructions/csharp-conventions.instructions.md`](../.github/instructions/csharp-conventions.instructions.md) — Versión normativa para IAs.
- [`.github/instructions/api-xmldocs.instructions.md`](../.github/instructions/api-xmldocs.instructions.md) — Plantillas XML para contratos API.

---

## Próximos Pasos

- **[API_DOCUMENTATION.md](API_DOCUMENTATION.md)** - Documentación XML obligatoria en Swagger
- **[TESTING.md](TESTING.md)** - MSTest + Moq + FluentAssertions
- **[DATA_ACCESS.md](DATA_ACCESS.md)** - Detalles de Dapper + SqlKata
- **[RESILIENCE.md](RESILIENCE.md)** - Reintentos y Polly
- **[HTTP_CLIENTS.md](HTTP_CLIENTS.md)** - IHttpClientFactory y handlers
