# 🏗️ Arquitectura - Olimpia API .NET 10

## Visión General

Olimpia implementa **Clean Architecture** (también conocida como Hexagonal o Ports & Adapters) con 4 capas claramente delimitadas. Las dependencias siempre apuntan hacia el **Dominio**, manteniendo lógica de negocio aislada de detalles técnicos.

```
Olimpia.Api         ──> Olimpia.Application
Olimpia.Api         ──> Olimpia.Infrastructure
Olimpia.Api         ──> Olimpia.Infrastructure.Logging
Olimpia.Infrastructure ──> Olimpia.Application
Olimpia.Infrastructure ──> Olimpia.Domain
Olimpia.Infrastructure.Logging ──> (solo NuGet, sin project references)
Olimpia.Infrastructure.Logging.Entities ──> (solo NuGet — DTOs desacoplados para LogCentral)
Olimpia.Api.Gateway ──> (standalone — Ocelot + MMLib.SwaggerForOcelot)
```

> **Nota:** `Olimpia.Infrastructure.Logging.Entities` es un assembly separado que contiene únicamente los DTOs de request a LogCentral (`CreateAuditRequest`, `CreateErrorRequest`, etc.) para evitar dependencias circulares. `Olimpia.Api.Gateway` es un servicio independiente que actúa como proxy/enrutador y no referencia ningún otro proyecto de la solución.

---

## Capas de la Arquitectura

### 1. **Olimpia.Domain** - Núcleo del Negocio

**Responsabilidad:** Entidades, Value Objects, interfaces de contrato (sin implementación).

| Elemento | Propósito |
|----------|-----------|
| `Entities/` | Clases del dominio (e.g., `Product.cs`). Heredan de `BaseEntity` con `Id`, `CreatedAt`, `UpdatedAt`. |
| `Repositories/` | **Interfaces** solo: `IGenericRepository<T>`, `IProductRepository`, `IUnitOfWork`, `IStoredProcedureRepository`, `IViewRepository`. |
| `Common/` | Tipos de filtrado y ordenamiento reutilizables: `FilterOperator` (enum), `FilterCriteria` (record), `SortCriteria` (record). |
| `ValueObjects/` | Objetos de valor sin identidad (e.g., `Money.cs`, `Address.cs`). |

**Restricciones:**
- ❌ Sin dependencias externas (ni Dapper, ni SqlKata, ni LogCentral)
- ❌ Sin referencias a capa Application, Infrastructure o Api
- ✅ Pueden usar solo `System.*`, `Microsoft.Extensions.*` (solo interfaces)

**Ejemplo de entidad:**
```csharp
public sealed class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }

    public Product() { }

    public Product(string name, string description, decimal price, int stock)
    {
        Name        = name;
        Description = description;
        Price       = price;
        Stock       = stock;
    }
}
```

---

### 2. **Olimpia.Application** - Casos de Uso

**Responsabilidad:** Commands, Queries, Validators, DTOs, contratos de APIs externas.

| Elemento | Propósito |
|----------|-----------|
| `Products/Commands/` | Command handlers (e.g., `CreateProductCommand`, `UpdateProductHandler`). Usan `Cortex.Mediator.Commands`. |
| `Products/Queries/` | Query handlers (e.g., `GetAllProductsQuery`, `GetProductByIdQuery`). Usan `Cortex.Mediator.Queries`. |
| `Products/Validators/` | `FluentValidation` validators, uno por Command/Query. |
| `Products/DTOs/` | Data Transfer Objects para responses (e.g., `ProductDto`). |
| `Common/Pagination/` | Contratos reutilizables de paginación: `PagedQuery` (abstract record base), `PagedResult<T>` (resultado con propiedades calculadas). |
| `Common/Responses/` | Envelopes estándar HTTP: `PagedEnvelope<T>`, `PagedMeta`, `PaginationMeta` — formato `{ data, meta }`. |
| `Contracts/` | Interfaces de clientes HTTP externos (e.g., `IExternalApiClient`). |
| `DependencyInjection.cs` | Registro de handlers, validators, servicios. |

**Restricciones:**
- ❌ Solo depende de `Domain`
- ❌ Sin acceso directo a BD (usa `IProductRepository` inyectado)
- ❌ Sin `Dapper`, sin SQL crudo
- ✅ Usa `Cortex.Mediator`, `FluentValidation`

**Ejemplo de Command Handler:**
```csharp
using Cortex.Mediator.Commands;

public record CreateProductCommand(string Name, decimal Price) : ICommand<int>;

public sealed class CreateProductHandler : ICommandHandler<CreateProductCommand, int>
{
    private readonly IProductRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductHandler(IProductRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(CreateProductCommand command, CancellationToken ct)
    {
        var product = new Product { Name = command.Name, Price = command.Price };
        var id = await _repository.AddAsync(product);
        await _unitOfWork.CommitAsync();
        return id;
    }
}
```

---

### 3. **Olimpia.Infrastructure** - Implementaciones Técnicas

**Responsabilidad:** Repositorios, UnitOfWork, HttpClients, acceso a BD, orquestación de servicios.

| Elemento | Propósito |
|----------|-----------|
| `Persistence/Repositories/` | Implementación de `GenericRepository<T>` (Dapper + SqlKata) e interfaces específicas. |
| `Persistence/UnitOfWork.cs` | Gestiona transacciones y conexión compartida con `QueryFactory`. |
| `Http/` | `BearerTokenPropagationHandler`, `PollyRetryHandler`, `ExternalApiClient`. |
| `DependencyInjection.cs` | Registro de repositorios, UnitOfWork, HttpClientFactory, QueryFactory. |

**Restricciones:**
- ✅ Puede depender de `Domain` y `Application`
- ✅ Usa `Dapper`, `SqlKata`, `Microsoft.Data.SqlClient`, Polly
- ❌ Sin lógica de negocio
- ❌ Sin Controllers, sin Middleware

**Ejemplo de Repositorio:**
```csharp
public sealed class ProductRepository : GenericRepository<global::Olimpia.Domain.Entities.Product>, IProductRepository
{
    public ProductRepository(QueryFactory db, UnitOfWork unitOfWork) 
        : base(db, unitOfWork) { }

    public async Task<global::Olimpia.Domain.Entities.Product?> GetByNameAsync(string name) =>
        await Db.Query(TableName).Where("Name", name)
            .FirstOrDefaultAsync<global::Olimpia.Domain.Entities.Product>(transaction: UnitOfWork.DbTransaction);
}
```

---

### 4. **Olimpia.Api** - Presentación

**Responsabilidad:** Controllers, Middleware, configuración HTTP, inyección de dependencias (Program.cs), logger.

| Elemento | Propósito |
|----------|-----------|
| `Controllers/` | REST endpoints, decoradores `[Authorize]`, validación HTTP. |
| `Middleware/` | ExceptionMiddleware, AuditMiddleware, RequestLoggingMiddleware. |
| `Logging/` | CustomLogger, LogEntry, LogWriter, LogCentralClient. |
| `Program.cs` | Registro de servicios, pipeline de middleware, configuración Swagger/JWT. |

**Restricciones:**
- ✅ Referencia todas las capas
- ✅ Punto de entrada de la aplicación
- ❌ Sin lógica de negocio (solo orquestación)
- ❌ Sin SQL directo

**Ejemplo de Controller:**
```csharp
namespace Olimpia.Api.Controllers.V1;

[Authorize]
[ApiVersion("1.0")]
public sealed class ProductController : ApiController
{
    private readonly IMediator _mediator;

    public ProductController(IMediator mediator) => _mediator = mediator;

    [MapToApiVersion("1.0")]
    [HttpPost]
    [Authorize(Policy = "products.write")]
    public async Task<IActionResult> Create(CreateProductCommand command, CancellationToken ct)
    {
        try
        {
            var id = await _mediator.SendAsync(command, ct);
            return Ok(new { Id = id });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { Error = ex.Message });
        }
    }
}
```

---

### 5. **Olimpia.Infrastructure.Logging** - Servicio Centralizado de Logging

**Responsabilidad:** Cliente HTTP para LogCentral, encolado offline, tipos de log, extensiones de `ILogger`.

| Elemento | Propósito |
|----------|-----------|
| `LogType.cs` | Enum: `Auditoria`, `Error`, `Eventos`, `Request`. |
| `ILogCentralClient.cs` | Interfaz para envío de logs a servicio centralizado. |
| `LogCentralClient.cs` | Implementación con reintentos automáticos. |
| `LoggerExtensions.cs` | Extensiones `LogAudit()`, `LogError()`, `LogEvent()`, `LogRequest()`. |
| `DependencyInjection.cs` | Método `AddLoggingInfrastructure()`. |
| `Requests/` | DTOs de serialización para LogCentral API (`CreateAuditRequest`, etc.). |

---

### 6. **Olimpia.Infrastructure.Logging.Entities** - DTOs Desacoplados de Logging

**Responsabilidad:** Assembly separado con únicamente los DTOs de request a LogCentral.
Elimina dependencias circulares en proyectos que necesitan los tipos sin importar todo el Logging.

| Elemento | Propósito |
|----------|-----------|
| `Requests/CreateAuditRequest.cs` | Payload de auditoría con `Action`, `Parameter`, `BeforeValue`, `AfterValue`. |
| `Requests/CreateErrorRequest.cs` | Payload de error con `Severity`, `Description`, `Code`. |
| `Requests/CreateEventRequest.cs` | Payload de evento con `Detail`, `Component`. |
| `Requests/CreateRequestRequest.cs` | Payload de request HTTP con `Method`, `Path`, `StatusCode`, `DurationMs`. |

---

### 7. **Olimpia.Api.Gateway** - API Gateway

**Responsabilidad:** Proxy de enrutamiento Ocelot + Swagger agregado via MMLib.SwaggerForOcelot.

| Elemento | Propósito |
|----------|-----------|
| `Program.cs` | Bootstrap con Ocelot + SwaggerForOcelot. |
| `ocelot.json` | Definición de rutas y downstream services. |
| `appsettings.json` | Configuración de logging del gateway. |

---

### 8. **Olimpia.Tests** - Pruebas Unitarias

**Responsabilidad:** Tests MSTest de Handlers, repositorios, y servicios.

| Elemento | Propósito |
|----------|-----------|
| `Handlers/` | Tests de Command/Query handlers. |
| `Repositories/` | Tests de repositorios con mocks de `QueryFactory`. |
| `Fixtures/` | Test data builders, mocks compartidos. |

**Stack:**
- `MSTest` (framework)
- `Moq` (mocking)
- `FluentAssertions` (assertions legibles)

---

## Estructura del Proyecto Completa

```
Olimpia/
├── src/
│   ├── Olimpia.Domain/
│   │   ├── Common/
│   │   │   ├── BaseEntity.cs
│   │   │   ├── FilterCriteria.cs
│   │   │   ├── FilterOperator.cs
│   │   │   └── SortCriteria.cs
│   │   ├── Entities/
│   │   │   └── Product.cs
│   │   └── Repositories/
│   │       ├── IGenericRepository.cs
│   │       ├── IProductRepository.cs
│   │       ├── IStoredProcedureRepository.cs
│   │       ├── IUnitOfWork.cs
│   │       └── IViewRepository.cs
│   │
│   ├── Olimpia.Application/
│   │   ├── Common/
│   │   │   ├── Configuration/
│   │   │   │   ├── JwtOptions.cs          (multi-proveedor JWT)
│   │   │   │   ├── JwtProviderOptions.cs
│   │   │   │   └── JwtProviderType.cs     (enum: Oidc | Symmetric)
│   │   │   ├── Pagination/
│   │   │   │   ├── PagedQuery.cs
│   │   │   │   └── PagedResult.cs
│   │   │   └── Responses/
│   │   │       └── PagedEnvelope.cs
│   │   ├── Contracts/
│   │   │   └── IExternalApiClient.cs
│   │   ├── Products/
│   │   │   ├── Commands/
│   │   │   │   └── CreateProduct/
│   │   │   │       ├── CreateProductCommand.cs
│   │   │   │       ├── CreateProductHandler.cs
│   │   │   │       └── CreateProductValidator.cs
│   │   │   ├── Mappings/
│   │   │   │   └── ProductMappingConfig.cs  (Mapster IRegister)
│   │   │   ├── Queries/
│   │   │   │   ├── GetAllProducts/
│   │   │   │   │   ├── GetAllProductsHandler.cs
│   │   │   │   │   ├── GetAllProductsQuery.cs
│   │   │   │   │   └── GetAllProductsValidator.cs
│   │   │   │   └── GetProductById/
│   │   │   │       ├── GetProductByIdHandler.cs
│   │   │   │       ├── GetProductByIdQuery.cs
│   │   │   │       └── GetProductByIdValidator.cs
│   │   │   └── ProductDto.cs
│   │   └── DependencyInjection.cs
│   │
│   ├── Olimpia.Infrastructure/
│   │   ├── Configuration/
│   │   │   ├── HttpClientRetryOptions.cs
│   │   │   └── RepositoryRetryOptions.cs
│   │   ├── Http/
│   │   │   ├── BearerTokenPropagationHandler.cs
│   │   │   ├── ExternalApiClient.cs
│   │   │   └── PollyRetryHandler.cs
│   │   ├── Persistence/
│   │   │   ├── Decorators/
│   │   │   │   ├── GenericRepositoryRetryDecorator.cs
│   │   │   │   ├── StoredProcedureRetryDecorator.cs
│   │   │   │   └── ViewRepositoryRetryDecorator.cs
│   │   │   ├── Repositories/
│   │   │   │   ├── GenericRepository.cs
│   │   │   │   ├── ProductRepository.cs
│   │   │   │   ├── StoredProcedureRepository.cs
│   │   │   │   └── ViewRepository.cs
│   │   │   └── UnitOfWork.cs
│   │   ├── IRequestLoggingService.cs
│   │   ├── RequestLoggingService.cs
│   │   └── DependencyInjection.cs
│   │
│   ├── Olimpia.Infrastructure.Logging/
│   │   ├── Requests/
│   │   │   ├── CreateAuditRequest.cs
│   │   │   ├── CreateErrorRequest.cs
│   │   │   ├── CreateEventRequest.cs
│   │   │   └── CreateRequestRequest.cs
│   │   ├── CustomLogger.cs
│   │   ├── CustomLoggerProvider.cs
│   │   ├── DependencyInjection.cs
│   │   ├── ILogCentralClient.cs
│   │   ├── LogCentralClient.cs
│   │   ├── LogContext.cs
│   │   ├── LoggerExtensions.cs
│   │   ├── LogType.cs
│   │   └── NullLogger.cs
│   │
│   ├── Olimpia.Infrastructure.Logging.Entities/
│   │   └── Requests/
│   │       ├── CreateAuditRequest.cs
│   │       ├── CreateErrorRequest.cs
│   │       ├── CreateEventRequest.cs
│   │       └── CreateRequestRequest.cs
│   │
│   ├── Olimpia.Api.Gateway/
│   │   ├── Program.cs
│   │   ├── ocelot.json
│   │   └── appsettings.json
│   │
│   └── Olimpia.Api/
│       ├── Controllers/
│       │   ├── ApiController.cs (base — ruta: api/v{version:apiVersion}/[controller])
│       │   └── V1/
│       │       └── ProductController.cs
│       ├── Extensions/
│       │   ├── ApiVersioningExtensions.cs
│       │   ├── ConfigureSwaggerOptions.cs
│       │   ├── PaginatedEndpointOperationFilter.cs
│       │   ├── QueryStringFilterParser.cs
│       │   ├── SecretsConfigurationExtensions.cs
│       │   └── SwaggerExtensions.cs
│       ├── Middleware/
│       │   ├── ExceptionMiddleware.cs
│       │   ├── RequestLoggingMiddleware.cs
│       │   └── AuditMiddleware.cs
│       ├── Program.cs
│       └── appsettings.json
│
├── tests/
│   └── Olimpia.Tests/
│       ├── Handlers/
│       │   └── Products/
│       │       ├── CreateProductHandlerTests.cs
│       │       ├── GetAllProductsHandlerTests.cs
│       │       ├── GetProductByIdHandlerTests.cs
│       │       ├── PagedEnvelopeTests.cs
│       │       ├── PagedResultTests.cs
│       │       └── ProductControllerTests.cs
│       ├── Infrastructure/
│       │   ├── Configuration/
│       │   │   └── RetryOptionsConfigurationTests.cs
│       │   ├── Http/
│       │   │   └── PollyRetryHandlerTests.cs
│       │   └── QueryStringFilterParserTests.cs
│       ├── Repositories/
│       │   └── GenericRepositoryRetryDecoratorTests.cs
│       ├── Validators/
│       │   ├── GetAllProductsValidatorTests.cs
│       │   ├── GetProductByIdValidatorTests.cs
│       │   └── JwtOptionsTests.cs
│       └── MSTestSettings.cs
│
├── .github/
│   ├── copilot-instructions.md
│   ├── agents/ (16 archivos *.agent.md)
│   ├── instructions/ (23 archivos *.instructions.md)
│   ├── prompts/ (5 archivos *.prompt.md)
│   ├── skills/ (7 carpetas con SKILL.md)
│   └── hooks/
│       └── quality-gates.json
│
├── docs/ (14 archivos .md de documentación)
├── specs/ (plantillas y specs activas)
├── Dockerfile
├── .env.example
├── Olimpia.slnx (archivo solución)
└── README.md
```

---

## Flujo de una Solicitud HTTP

```
1. Request HTTP
   │
   ├─> ExceptionMiddleware (envuelve todo, captura excepciones)
   │
   ├─> RequestLoggingMiddleware (inicia Stopwatch)
   │
   ├─> AuditMiddleware (identifica usuario)
   │
   ├─> Authentication (valida JWT)
   │
   ├─> Authorization (verifica scopes/políticas)
   │
   ├─> ProductController.Create(CreateProductCommand)
   │   │
   │   └─> _mediator.SendAsync(command)
   │       │
   │       ├─> CreateProductValidator.Validate(command) ✓ o ✗
   │       │
   │       └─> CreateProductHandler.Handle(command)
   │           │
   │           ├─> _repository.AddAsync(product)
   │           │   │
   │           │   └─> QueryFactory (SqlKata + Dapper)
   │           │       │
   │           │       └─> SQL Server (INSERT)
   │           │
   │           ├─> _unitOfWork.CommitAsync() (COMMIT transacción)
   │           │
   │           └─> return id ✓
   │
   ├─> Response 200 OK
   │
   ├─> AuditMiddleware (finally: LogAudit con usuario, status, duración)
   │
   ├─> RequestLoggingMiddleware (finally: LogRequest con duración Stopwatch)
   │
   └─> CustomLogger.Log(...)
       │
       ├─> LogCentral (si enabled)
       │   └─> HTTP POST a servicio centralizado
       │
       └─> FileLogWriter (fallback o si disabled)
           └─> Archivo logs/yyyy-MM-dd.jsonl
```

---

## Principios de Diseño

### 1. **Dependency Inversion**
- Las capas superiores dependen de abstracciones (interfaces) de capas inferiores
- `IProductRepository` se define en `Domain`, se implementa en `Infrastructure`

### 2. **Single Responsibility**
- Cada clase tiene una razón para cambiar
- `CreateProductHandler` solo orquesta la creación
- `ProductRepository` solo accede a datos
- `CustomLogger` solo registra

### 3. **Open/Closed**
- Abierto para extensión (añadir nuevos repositories, handlers, middlewares)
- Cerrado para modificación (cambios mínimos en código existente)
- Ejemplo: `GenericRepository<T>` base, `ProductRepository` específico

### 4. **Testability**
- Todas las dependencias inyectadas
- Fácil crear mocks (Moq)
- Handlers no tienen efectos secundarios no esperados

---

## Decisiones Arquitectónicas

| Decisión | Razón |
|----------|-------|
| **Clean Architecture** | Separación clara, portabilidad, testability |
| **CQRS** | Commands (escritura) y Queries (lectura) separadas, escalabilidad futura |
| **GenericRepository<T>** | CRUD automático, menos código repetido, cohesión |
| **Auto-registro de Repos** | Escalabilidad: agregar repo sin tocar DI |
| **Cortex.Mediator** | CQRS ligero, alternativa a MediatR, mejor rendimiento |
| **Dapper + SqlKata** | Control fino de SQL, sin ORM pesado, API fluida, rendimiento |
| **IDistributedCache** | Redis opcional, abstraído, fácil cambiar a otro provider |
| **Logger personalizado** | Sin dependencias externas, integracion con LogCentral |
| **OpenIddict Resource Server** | OIDC estándar, solo valida tokens JWT, no los emite |

---

## Regla `global::` en la Arquitectura

Toda referencia a tipos de entidad en archivos de **Repositorio**, **Handler** o **Test** debe calificarse con `global::`:

```csharp
// ✅ CORRECTO
public sealed class ProductRepository : GenericRepository<global::Olimpia.Domain.Entities.Product>, IProductRepository

// ❌ EVITAR (error CS0118 en algunos contextos)
public sealed class ProductRepository : GenericRepository<Product>, IProductRepository
```

Esto evita ambigüedad cuando el nombre de la clase coincide con un segmento del namespace (e.g., clase `Factura` en namespace `Olimpia.Facturacion.Factura.*`).

---

## Próximos Pasos

- **[PATTERNS.md](PATTERNS.md)** - Patrones implementados (CQRS, Repositories, Decorators)
- **[DATA_ACCESS.md](DATA_ACCESS.md)** - Acceso a datos (Dapper, SqlKata, Repositories)
- **[RESILIENCE.md](RESILIENCE.md)** - Reintentos y decoradores (Polly)
- **[AUTHENTICATION.md](AUTHENTICATION.md)** - JWT y OpenIddict Resource Server
