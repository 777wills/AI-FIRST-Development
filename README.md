# Olimpia — Clean Architecture Web API (.NET 10)

> API REST con **Clean Architecture**, **CQRS (Cortex.Mediator)**, **Dapper + SqlKata**, **JWT Bearer (OpenIddict)**, **Redis Cache**, **Logger centralizado (LogCentral)** y **Spec-Driven Development with AI**.

| | |
|---|---|
| **Arquitectura** | Clean Architecture — 4 capas |
| **Framework** | .NET 10 |
| **Datos** | Dapper + SqlKata (sin Entity Framework) |
| **Auth** | OpenIddict (Resource Server) |
| **Logging** | LogCentral (servicio centralizado) con fallback local |
| **Cache** | Redis via IDistributedCache |
| **Testing** | MSTest + Moq + FluentAssertions |
| **Metodología** | Spec-Driven Development with AI |

---

## Inicio Rápido

### Prerrequisitos
- .NET SDK 10.0
- SQL Server (local o Docker)

### Post-generación

1. **Actualizar el proyecto de Azure DevOps** en `.github/agents/spec-builder.agent.md`:
   - Cambiar `CAMBIAR_NOMBRE_PROYECTO_ADO` por el nombre real del proyecto en Azure DevOps.

2. **Configurar conexión a BD**:
   ```bash
   # Editar src/Olimpia.Api/appsettings.Development.json
   # Actualizar ConnectionStrings:DefaultConnection con tu servidor SQL
   ```

3. **Ejecutar API**:
   ```bash
   dotnet run --project src/Olimpia.Api
   ```

4. **Swagger UI**: `https://localhost:7153/swagger`

### Docker

```bash
docker build -t olimpia-template .

docker run -p 8080:8080 \
  -e "Jwt__Authority=http://localhost:5001" \
  -e "ConnectionStrings__DefaultConnection=Server=db;..." \
  olimpia-template
```

Ver [**DEPLOYMENT.md**](docs/DEPLOYMENT.md) para detalles completos.

---

## Estructura del Proyecto

```
{NombreProyecto}/
├── src/
│   ├── Olimpia.Domain/              # Entidades, interfaces
│   ├── Olimpia.Application/         # Commands, Queries, Validators
│   ├── Olimpia.Infrastructure/      # Repositories, HTTP Clients, UnitOfWork
│   ├── Olimpia.Infrastructure.Logging/  # LogCentral, OfflineQueue
│   └── Olimpia.Api/                 # Controllers, Middleware, Program.cs
├── tests/
│   └── Olimpia.Tests/               # MSTest, fixtures
├── .github/
│   ├── copilot-instructions.md      # Reglas de codificación
│   ├── agents/                      # 16 agentes Copilot (4 principales + 12 sub-agentes)
│   ├── prompts/                     # /spec-from-story, /plan-from-spec, /tasks-from-plan, /implement-tasks
│   ├── skills/                      # caching, clean-arch-validation, external-api, new-feature, stored-procedures-views, tdd-workflow
│   └── instructions/                # Reglas por capa (testing, clean-arch, csharp, database)
├── specs/
│   ├── active/                      # Features en desarrollo
│   └── templates/                   # Plantillas de specification, plan, tasks
├── docs/                            # Documentación completa por tema
├── Dockerfile
└── README.md (este archivo)
```

Ver [**ARCHITECTURE.md**](docs/ARCHITECTURE.md) para estructura completa.

---

## Documentación

| Tema | Documento |
|------|-----------|
| **Arquitectura** | [ARCHITECTURE.md](docs/ARCHITECTURE.md) |
| **Patrones CQRS** | [PATTERNS.md](docs/PATTERNS.md) |
| **Data Access** | [DATA_ACCESS.md](docs/DATA_ACCESS.md) |
| **Autenticación** | [AUTHENTICATION.md](docs/AUTHENTICATION.md) |
| **Security Standards** | [Constitution Principle X](.specify/memory/constitution.md#x-security-standards--application-security-governance-non-negotiable) + [Security Package](.specify/presets/secure-engineering-kit/memory/security/) |
| **HTTP Clients** | [HTTP_CLIENTS.md](docs/HTTP_CLIENTS.md) |
| **Caché Redis** | [CACHING.md](docs/CACHING.md) |
| **Logging** | [LOGGING_CENTRAL.md](docs/LOGGING_CENTRAL.md) |
| **Resiliencia** | [RESILIENCE.md](docs/RESILIENCE.md) |
| **Configuración** | [CONFIGURATION.md](docs/CONFIGURATION.md) |
| **Deployment** | [DEPLOYMENT.md](docs/DEPLOYMENT.md) |
| **Testing** | [TESTING.md](docs/TESTING.md) |
| **Paginación** | [PAGINATION.md](docs/PAGINATION.md) |

---

## Stack Tecnológico

| Componente | Librería | Versión |
|---|---|---|
| **Framework** | `.NET` | `10` |
| **CQRS / Mediator** | `Cortex.Mediator` | `3.1.2` |
| **Validación** | `FluentValidation` | `12.1.1` |
| **Data Access** | `Dapper` + `SqlKata` + `SqlKata.Execution` | latest + `4.0.1` |
| **SQL Server** | `Microsoft.Data.SqlClient` | latest |
| **Cache** | `Microsoft.Extensions.Caching.StackExchangeRedis` | `9.0.0` |
| **API Docs** | `Swashbuckle.AspNetCore` | `6.9.0` |
| **JWT** | `Microsoft.AspNetCore.Authentication.JwtBearer` | `10.x` |
| **HTTP Clients** | `IHttpClientFactory` + `Polly` | built-in + `8.x` |
| **Logging** | Implementación custom + LogCentral | custom |
| **Testing** | `MSTest` + `Moq` + `FluentAssertions` | latest |

---

## Ejecución Rápida

### Prerrequisitos
- .NET SDK 10.0
- SQL Server (local o Docker)

### Pasos
1. **Configurar conexión**
   ```bash
   # Editar src/Olimpia.Api/appsettings.Development.json
   # Actualizar ConnectionStrings:DefaultConnection
   ```

2. **Ejecutar API**
   ```bash
   dotnet run --project src/Olimpia.Api
   ```

3. **Swagger UI**
   ```
   https://localhost:7153/swagger
   ```

### Docker
```bash
docker build -t olimpia-template .

docker run -p 8080:8080 \
  -e "Jwt__Authority=http://localhost:5001" \
  -e "ConnectionStrings__DefaultConnection=Server=db;..." \
  olimpia-template
```

Ver [**DEPLOYMENT.md**](docs/DEPLOYMENT.md) para detalles completos.

---

## Estructura del Proyecto

```
Olimpia/
├── src/
│   ├── Olimpia.Domain/              # Entidades, interfaces
│   ├── Olimpia.Application/         # Commands, Queries, Validators
│   ├── Olimpia.Infrastructure/      # Repositories, HTTP Clients, UnitOfWork
│   ├── Olimpia.Infrastructure.Logging/  # LogCentral, OfflineQueue
│   └── Olimpia.Api/                 # Controllers, Middleware, Program.cs
├── tests/
│   └── Olimpia.Tests/               # MSTest, fixtures
├── .github/
│   ├── copilot-instructions.md      # Reglas de codificación
│   ├── agents/                      # 16 agentes Copilot (4 principales + 12 sub-agentes)
│   ├── prompts/                     # /spec-from-story, /plan-from-spec, /tasks-from-plan, /implement-tasks
│   ├── skills/                      # caching, clean-arch-validation, external-api, new-feature, stored-procedures-views, tdd-workflow
│   ├── hooks/                       # Quality gates (SubagentStart, PreToolUse)
│   └── instructions/                # Reglas por capa (testing, clean-arch, csharp, database)
├── specs/
│   ├── active/                      # Features en desarrollo ({ID}-{feature}/)
│   ├── completed/                   # Features terminadas
│   └── templates/                   # Plantillas de specification, plan, tasks
├── Dockerfile
├── docker-compose.yml
└── README.md (este archivo)
```

Ver [**ARCHITECTURE.md**](docs/ARCHITECTURE.md) para estructura completa.

---

## Flujo AI-First (Spec-Driven Development)

El proyecto implementa un flujo de desarrollo **Spec-Driven** con **GitHub Copilot Agents** que transforma historias de usuario en código productivo mediante 4 fases secuenciales:

```
/spec-from-story → /plan-from-spec → /tasks-from-plan → /implement-tasks
```

| Fase | Prompt | Agente | Produce |
|------|--------|--------|---------|
| 1. Especificación | `/spec-from-story` | Spec Builder | `specs/active/{ID}-{feature}/specification.md` |
| 2. Plan | `/plan-from-spec {ID}` | Plan Builder | `specs/active/{ID}-{feature}/plan.md` |
| 3. Tareas | `/tasks-from-plan {ID}` | Task Definer | `specs/active/{ID}-{feature}/tasks.md` |
| 4. Implementación | `/implement-tasks {ID}` | Orchestrator | Código, tests, SQL, docs |

El Orchestrator delega a **12 sub-agentes** especializados (Domain, Application, Infrastructure, API, SQL Server, TDD Red/Green/Refactor, Code Reviewer, Coverage Analyzer, Doc Updater, Explorer) — cada uno con contexto aislado y TDD estricto (≥95% cobertura).

Ver [**AI-FIRST-WORKFLOW.md**](AI-FIRST-WORKFLOW.md) para documentación completa del flujo.

---

## Patrón: CQRS con Cortex.Mediator

### Command (Escritura)
```csharp
public record CreateProductCommand(string Name, decimal Price) : ICommand<int>;

public sealed class CreateProductHandler : ICommandHandler<CreateProductCommand, int>
{
    public async Task<int> Handle(CreateProductCommand cmd, CancellationToken ct)
    {
        var product = new Product { Name = cmd.Name, Price = cmd.Price };
        var id = await _repository.AddAsync(product);
        await _unitOfWork.CommitAsync();
        return id;
    }
}
```

### Query (Lectura)
```csharp
public record GetProductQuery(int Id) : IQuery<ProductDto>;

public sealed class GetProductHandler : IQueryHandler<GetProductQuery, ProductDto>
{
    public async Task<ProductDto> Handle(GetProductQuery query, CancellationToken ct)
    {
        var product = await _repository.GetByIdAsync(query.Id);
        return new ProductDto { Id = product.Id, Name = product.Name };
    }
}
```

### Query Paginada (Listado)
```csharp
// Heredar PagedQuery y declarar el tipo de retorno
public sealed record GetAllProductsQuery(
    int PageNumber = 1,
    int PageSize = 25,
    IReadOnlyList<FilterCriteria>? Filters = null,
    IReadOnlyList<SortCriteria>? SortFields = null)
    : PagedQuery(PageNumber, PageSize, Filters, SortFields), IQuery<PagedResult<ProductDto>>;

// El Controller parsea la query string y envuelve con PagedEnvelope<T>
// GET /api/v1/products?name[contains]=Laptop&price[gte]=100&sort=name,-price
// → { "data": [...], "meta": { "pagination": { "currentPage": 1, "totalPages": 5, ... } } }
```

**Endpoints disponibles:**

| Verbo | Ruta | Scope requerido | Descripción |
|-------|------|-----------------|-------------|
| `POST` | `/api/v1/products` | `products.write` | Crear producto |
| `GET` | `/api/v1/products` | `products.read` | Listar paginado (filtros + orden) |
| `GET` | `/api/v1/products/{id}` | `products.read` | Obtener por Id |

Ver [**PATTERNS.md**](docs/PATTERNS.md) para detalles completos.

---

## Repository Pattern

**GenericRepository<T>** proporciona CRUD automático. Solo implementa métodos específicos del dominio.

```csharp
// Domain: interfaz
public interface IProductRepository : IGenericRepository<Product>
{
    Task<Product?> GetByNameAsync(string name);
}

// Infrastructure: implementación
public sealed class ProductRepository : GenericRepository<Product>, IProductRepository
{
    public ProductRepository(QueryFactory db, UnitOfWork unitOfWork) : base(db, unitOfWork) { }

    public async Task<Product?> GetByNameAsync(string name) =>
        await Db.Query(TableName)
            .Where("Name", name)
            .FirstOrDefaultAsync<Product>(transaction: UnitOfWork.DbTransaction);
}
```

Auto-registro: agregar interfaz + clase, ¡sin tocar DependencyInjection.cs!

Ver [**PATTERNS.md#2-repository-pattern**](docs/PATTERNS.md#2-repository-pattern) y [**DATA_ACCESS.md**](docs/DATA_ACCESS.md).

---

## Autenticación & Autorización

**Resource Server** con OpenIddict. Token se valida automáticamente.

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "products.write")]  // Requiere scope
public sealed class ProductController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateProductCommand command)
    {
        var id = await _mediator.SendAsync(command);
        return Created($"api/products/{id}", new { Id = id });
    }
}
```

Token del request entrante se propaga automáticamente a APIs externas.

Ver [**AUTHENTICATION.md**](docs/AUTHENTICATION.md) y [**HTTP_CLIENTS.md**](docs/HTTP_CLIENTS.md).

---

## HTTP Clients con Token Relay

APIs externas reciben el token del request entrante automáticamente.

```csharp
public sealed class GetCatalogHandler : IQueryHandler<GetCatalogQuery, CatalogDto>
{
    private readonly IExternalApiClient _client;

    public async Task<CatalogDto> Handle(GetCatalogQuery query, CancellationToken ct)
    {
        // Token se propaga automáticamente
        var catalog = await _client.GetAsync<CatalogDto>(
            "CatalogoService",
            $"api/catalog/{query.Id}",
            ct);
        return catalog;
    }
}
```

Con **reintentos automáticos** (Polly): 3 intentos con backoff exponencial.

Ver [**HTTP_CLIENTS.md**](docs/HTTP_CLIENTS.md) y [**RESILIENCE.md**](docs/RESILIENCE.md).

---

## Redis Cache

Caché distribuida con expiración configurable.

```csharp
public sealed class GetProductHandler : IQueryHandler<GetProductQuery, ProductDto>
{
    public async Task<ProductDto> Handle(GetProductQuery query, CancellationToken ct)
    {
        var cacheKey = $"product:{query.Id}";
        
        // 1. Intentar caché
        var cached = await _cache.GetStringAsync(cacheKey, ct);
        if (!string.IsNullOrEmpty(cached))
            return JsonSerializer.Deserialize<ProductDto>(cached);

        // 2. Base de datos
        var product = await _repository.GetByIdAsync(query.Id);

        // 3. Guardar en caché
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(product),
            new DistributedCacheEntryOptions 
            { 
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30) 
            }, ct);

        return product;
    }
}
```

Ver [**CACHING.md**](docs/CACHING.md).

---

## Testing

**MSTest + Moq + FluentAssertions**

```csharp
[TestClass]
public sealed class CreateProductHandlerTests
{
    [TestMethod]
    public async Task Handle_Should_ReturnProductId_When_ValidCommand()
    {
        // Arrange
        var command = new CreateProductCommand("Laptop", 1500m);
        var repositoryMock = new Mock<IProductRepository>();
        repositoryMock.Setup(x => x.AddAsync(It.IsAny<global::Olimpia.Domain.Entities.Product>())).ReturnsAsync(1);

        var handler = new CreateProductHandler(repositoryMock.Object, _unitOfWorkMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(1);
        repositoryMock.Verify(x => x.AddAsync(It.IsAny<global::Olimpia.Domain.Entities.Product>()), Times.Once);
    }
}
```

Ver [**TESTING.md**](docs/TESTING.md).

---

## Configuración por Entorno

Variables de entorno con separador `__` (doble guion bajo):

```bash
# Desarrollo
Jwt__Authority=http://localhost:5001
ConnectionStrings__DefaultConnection=Server=(localdb);

# Producción
Jwt__Authority=https://identity.production.com
Jwt__RequireHttpsMetadata=true
RedisCache__Enabled=true
RedisCache__ConnectionString=redis-cluster:6379
```

Ver [**CONFIGURATION.md**](docs/CONFIGURATION.md).

---

## Deployment

### Docker
```bash
docker build -t olimpia-template .
docker run -p 8080:8080 olimpia-template
```

### Kubernetes
```bash
kubectl apply -f k8s/
kubectl logs -f deployment/olimpia-template
```

### Docker Compose
```bash
docker-compose up -d
```

Ver [**DEPLOYMENT.md**](docs/DEPLOYMENT.md).

---

## Regla global::

Toda referencia a tipos de entidad en Repositorios/Handlers/Tests usa `global::`:

```csharp
// ✅ Correcto
public sealed class ProductRepository : GenericRepository<global::Olimpia.Domain.Entities.Product>

// ❌ Evitar
public sealed class ProductRepository : GenericRepository<Product>
```

Esto evita ambigüedad cuando el nombre de la clase coincide con un segmento del namespace.

---

## Convenciones de Código

| Elemento | Convención | Ejemplo |
|----------|-----------|---------|
| Métodos | PascalCase + `Async` | `GetProductAsync` |
| Variables locales | camelCase | `productId` |
| Propiedades públicas | PascalCase | `ProductId` |
| Campos privados | `_camelCase` | `_productRepository` |
| Interfaces | I + PascalCase | `IProductRepository` |
| Clases | PascalCase + `sealed` | `sealed class ProductHandler` |

**Reglas clave:**
- Todas las clases concretas son `sealed` (handlers, validators, repos, controllers, test classes).
- Constructor dual en entidades: vacío (Dapper) + parametrizado (creación).
- Código en inglés, comentarios en español.
- Comentarios Copilot obligatorios: `// Método generado por GitHub Copilot`.

Ver [**PATTERNS.md#7-convenciones-de-código-c**](docs/PATTERNS.md#7-convenciones-de-código-c) y [**.github/copilot-instructions.md**](.github/copilot-instructions.md).

---

## Template

Este proyecto fue generado desde el template organizacional de clean architecture.

Para generar nuevos proyectos, consulta la documentación del template en el repositorio base.

---

## Próximas Lecturas

1. **[DOCUMENTATION.md](docs/DOCUMENTATION.md)** — Índice y guía de lectura
2. **[ARCHITECTURE.md](docs/ARCHITECTURE.md)** — Capas y estructura
3. **[PATTERNS.md](docs/PATTERNS.md)** — CQRS, Repository, Decorators
4. **[.github/copilot-instructions.md](.github/copilot-instructions.md)** — Estándares

---

**¿Dudas? Consulta [DOCUMENTATION.md](docs/DOCUMENTATION.md).**
