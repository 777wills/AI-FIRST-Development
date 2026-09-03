# OlimpiaIT.CleanApi.Template

Template organizacional de **API REST** con **Clean Architecture** para equipos de Olimpia IT.

> .NET 10 · CQRS (Cortex.Mediator) · Dapper + SqlKata · JWT Bearer (OpenIddict) · Redis Cache · LogCentral · Spec-Driven Development with AI

---

## Tabla de Contenido

- [Requisitos Previos](#requisitos-previos)
- [Instalación del Template](#instalación-del-template)
- [Crear un Nuevo Proyecto](#crear-un-nuevo-proyecto)
- [Parámetros](#parámetros)
- [Resultado Generado](#resultado-generado)
- [Post-Generación — Primeros Pasos](#post-generación--primeros-pasos)
- [Estructura del Proyecto](#estructura-del-proyecto)
- [Arquitectura](#arquitectura)
- [Stack Tecnológico](#stack-tecnológico)
- [Patrones Incluidos](#patrones-incluidos)
- [Autenticación y Autorización](#autenticación-y-autorización)
- [Data Access — Dapper + SqlKata](#data-access--dapper--sqlkata)
- [Redis Cache](#redis-cache)
- [HTTP Clients con Token Relay](#http-clients-con-token-relay)
- [Logging Centralizado](#logging-centralizado)
- [Paginación, Filtrado y Ordenamiento](#paginación-filtrado-y-ordenamiento)
- [Testing](#testing)
- [Configuración por Entorno](#configuración-por-entorno)
- [Docker y Deployment](#docker-y-deployment)
- [Flujo AI-First (Spec-Driven Development)](#flujo-ai-first-spec-driven-development)
- [Convenciones de Código](#convenciones-de-código)
- [Documentación Incluida](#documentación-incluida)
- [Empaquetar y Publicar (Mantenedores)](#empaquetar-y-publicar-mantenedores)
- [Desinstalar](#desinstalar)

---

## Requisitos Previos

- **.NET SDK 10.0** o superior
- **SQL Server** (local, Docker o remoto)
- **Redis** (opcional, para caché distribuida)
- Acceso al feed de Azure Artifacts de Olimpia IT

---

## Instalación del Template

```bash
dotnet new install OlimpiaIT.CleanApi.Template \
  --nuget-source "https://pkgs.dev.azure.com/olimpiait/9fc7cbdd-2938-4c6e-be8b-989e9e378132/_packaging/sicovii/nuget/v3/index.json" \
  --interactive
```

> `--interactive` permite autenticarse con Azure DevOps si es necesario.

Verificar instalación:
```bash
dotnet new list olimpia
```

---

## Crear un Nuevo Proyecto

### Con ejemplos incluidos (CRUD de Products)
```bash
dotnet new olimpia-cleanapi --OrgName MiEmpresa --ProjectName Inventario
```

### Sin ejemplos (proyecto vacío)
```bash
dotnet new olimpia-cleanapi --OrgName MiEmpresa --ProjectName Inventario --IncludeExamples false
```

---

## Parámetros

| Parámetro | Obligatorio | Default | Descripción |
|-----------|:-----------:|:-------:|-------------|
| `--OrgName` | ✅ | — | Nombre de la organización en PascalCase (ej: `Olimpia`, `MiEmpresa`). Primer segmento del namespace. |
| `--ProjectName` | ✅ | — | Nombre del proyecto en PascalCase (ej: `Inventario`, `Facturacion`). Segundo segmento del namespace. |
| `--IncludeExamples` | ❌ | `true` | Incluir CRUD de Products como ejemplo funcional. |

---

## Resultado Generado

Con `--OrgName Acme --ProjectName Billing` se genera:

```
Acme.Billing/
├── src/
│   ├── Acme.Billing.Domain/
│   ├── Acme.Billing.Application/
│   ├── Acme.Billing.Infrastructure/
│   ├── Acme.Billing.Infrastructure.Logging/
│   └── Acme.Billing.Api/
├── tests/
│   └── Acme.Billing.Tests/
├── docs/
├── .github/
├── specs/
├── Dockerfile
├── Acme.Billing.slnx
└── README.md
```

**Todo se renombra automáticamente**: namespaces, carpetas, `.csproj`, Dockerfile, configuraciones de Redis, JWT y logs.

---

## Post-Generación — Primeros Pasos

### 1. Configurar Azure DevOps
Editar `.github/agents/spec-builder.agent.md` y reemplazar `CAMBIAR_NOMBRE_PROYECTO_ADO` por el nombre real del proyecto en Azure DevOps.

### 2. Configurar conexión a Base de Datos
```bash
# Editar src/{OrgName}.{ProjectName}.Api/appsettings.Development.json
```
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MiDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### 3. Compilar y verificar
```bash
dotnet build
```

### 4. Ejecutar la API
```bash
dotnet run --project src/{OrgName}.{ProjectName}.Api
```

### 5. Abrir Swagger UI
```
https://localhost:7153/swagger
```

### 6. Ejecutar tests (si incluyó ejemplos)
```bash
dotnet test
```

---

## Estructura del Proyecto

```
{Proyecto}/
├── src/
│   ├── {Org}.{Proj}.Domain/              # Entidades, interfaces de repositorio
│   ├── {Org}.{Proj}.Application/         # Commands, Queries, Validators, DTOs
│   ├── {Org}.{Proj}.Infrastructure/      # Repositories (Dapper+SqlKata), HTTP Clients, UoW
│   ├── {Org}.{Proj}.Infrastructure.Logging/  # LogCentral, OfflineQueue
│   └── {Org}.{Proj}.Api/                 # Controllers, Middleware, Program.cs
├── tests/
│   └── {Org}.{Proj}.Tests/               # MSTest + Moq + FluentAssertions
├── .github/
│   ├── copilot-instructions.md           # Reglas globales de codificación
│   ├── agents/                           # 16 agentes Copilot especializados
│   ├── prompts/                          # Slash commands para AI workflow
│   ├── skills/                           # Skills: caching, external-api, tdd, etc.
│   └── instructions/                     # Reglas por capa y componente
├── specs/
│   ├── active/                           # Features en desarrollo
│   ├── completed/                        # Features terminadas
│   └── templates/                        # Plantillas de spec, plan, tasks
├── docs/                                 # Documentación técnica completa
├── scripts/sql/                          # Scripts SQL iniciales
├── Dockerfile
└── README.md
```

---

## Arquitectura

**Clean Architecture** estricta de 4 capas con dependencias de afuera hacia adentro:

```
┌─────────────────────────────────────────────────────────────┐
│  Api (Controllers, Middleware, Program.cs)                   │
├─────────────────────────────────────────────────────────────┤
│  Infrastructure (Repos, HTTP Clients, UoW, DI)              │
│  Infrastructure.Logging (LogCentral)                        │
├─────────────────────────────────────────────────────────────┤
│  Application (Commands, Queries, Handlers, Validators)      │
├─────────────────────────────────────────────────────────────┤
│  Domain (Entities, Repository Interfaces)                   │
└─────────────────────────────────────────────────────────────┘
```

**Reglas de dependencia:**
- `Domain` → Sin dependencias externas
- `Application` → Solo depende de `Domain`
- `Infrastructure` → Depende de `Application`
- `Api` → Depende de `Application`, `Infrastructure`

---

## Stack Tecnológico

| Componente | Librería | Versión |
|:---|:---|:---|
| **Framework** | .NET | 10 |
| **CQRS / Mediator** | Cortex.Mediator | 3.1.2 |
| **Validación** | FluentValidation | 12.1.1 |
| **Data Access** | Dapper + SqlKata + SqlKata.Execution | latest + 4.0.1 |
| **SQL Server** | Microsoft.Data.SqlClient | latest |
| **Cache** | Microsoft.Extensions.Caching.StackExchangeRedis | 9.0.0 |
| **API Docs** | Swashbuckle.AspNetCore | 6.9.0 |
| **JWT** | Microsoft.AspNetCore.Authentication.JwtBearer | 10.x |
| **HTTP Clients** | IHttpClientFactory + Polly | built-in + 8.x |
| **Logging** | LogCentral (custom) | custom |
| **Testing** | MSTest + Moq + FluentAssertions | latest |

---

## Patrones Incluidos

### CQRS con Cortex.Mediator

**NO usa MediatR**. Usa exclusivamente `Cortex.Mediator`:
- `SendAsync` para **Commands** (escritura)
- `SendQueryAsync` para **Queries** (lectura)

#### Command (Escritura)
```csharp
// Record inmutable que implementa ICommand<TResult>
public record CreateProductCommand(string Name, decimal Price) : ICommand<int>;

// Handler sealed con un solo método Handle
public sealed class CreateProductHandler : ICommandHandler<CreateProductCommand, int>
{
    private readonly IProductRepository _repository;
    private readonly UnitOfWork _unitOfWork;

    public async Task<int> Handle(CreateProductCommand cmd, CancellationToken ct)
    {
        var product = new Product { Name = cmd.Name, Price = cmd.Price };
        var id = await _repository.AddAsync(product);
        await _unitOfWork.CommitAsync();
        return id;
    }
}
```

#### Query (Lectura)
```csharp
public record GetProductQuery(int Id) : IQuery<ProductDto>;

public sealed class GetProductHandler : IQueryHandler<GetProductQuery, ProductDto>
{
    public async Task<ProductDto> Handle(GetProductQuery query, CancellationToken ct)
    {
        var product = await _repository.GetByIdAsync(query.Id)
            ?? throw new KeyNotFoundException("Producto no encontrado.");
        return new ProductDto { Id = product.Id, Name = product.Name };
    }
}
```

#### Query Paginada
```csharp
public sealed record GetAllProductsQuery(
    int PageNumber = 1,
    int PageSize = 25,
    IReadOnlyList<FilterCriteria>? Filters = null,
    IReadOnlyList<SortCriteria>? SortFields = null)
    : PagedQuery(PageNumber, PageSize, Filters, SortFields), IQuery<PagedResult<ProductDto>>;
```

### Repository Pattern

**GenericRepository\<T\>** con CRUD automático. Solo se implementan métodos específicos:

```csharp
// Domain: interfaz
public interface IProductRepository : IGenericRepository<Product>
{
    Task<Product?> GetByNameAsync(string name);
}

// Infrastructure: implementación (auto-registrada con Scrutor)
public sealed class ProductRepository : GenericRepository<Product>, IProductRepository
{
    public async Task<Product?> GetByNameAsync(string name) =>
        await Db.Query(TableName)
            .Where("Name", name)
            .FirstOrDefaultAsync<Product>(transaction: UnitOfWork.DbTransaction);
}
```

> **Auto-registro**: solo agregar interfaz + clase. **No se toca** `DependencyInjection.cs` (Scrutor lo registra automáticamente).

### Validación con FluentValidation

Cada Command/Query puede tener un Validator:
```csharp
public sealed class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThan(0);
    }
}
```

---

## Autenticación y Autorización

**Resource Server** con **OpenIddict**. El token JWT se valida automáticamente.

```csharp
// Proteger por scope
[Authorize(Policy = "products.write")]
[HttpPost]
public async Task<IActionResult> Create(CreateProductCommand command)
{
    var id = await _mediator.SendAsync(command);
    return Created($"api/products/{id}", new { Id = id });
}
```

**Scopes como Policies**: Cada scope del token se mapea a una policy de autorización.

El token del request entrante se propaga automáticamente a APIs externas via `BearerTokenPropagationHandler`.

---

## Data Access — Dapper + SqlKata

**NO usa Entity Framework**. El acceso a datos es exclusivamente con `Dapper` + `SqlKata`:

```csharp
// Queries con SqlKata fluent API
var products = await Db.Query("Products")
    .Where("IsActive", true)
    .OrderBy("Name")
    .GetAsync<Product>(transaction: UnitOfWork.DbTransaction);

// Soporte para Stored Procedures y Views
var result = await _spRepo.ExecuteAsync<ResultDto>(
    "sp_GetProductReport",
    new { StartDate = startDate, EndDate = endDate });
```

**Transacciones** se manejan con `UnitOfWork`:
```csharp
await _unitOfWork.BeginAsync();
// ... operaciones ...
await _unitOfWork.CommitAsync();
```

---

## Redis Cache

Caché distribuida con **Redis** usando el patrón Cache-Aside:

```csharp
// En Query Handlers — verificar caché antes de ir a BD
var cacheKey = $"product:{query.Id}";
var cached = await _cache.GetStringAsync(cacheKey, ct);
if (!string.IsNullOrEmpty(cached))
    return JsonSerializer.Deserialize<ProductDto>(cached);

// Si no está en caché, ir a BD y guardar
var product = await _repository.GetByIdAsync(query.Id);
await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(product),
    new DistributedCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
    }, ct);
```

Configuración en `appsettings.json`:
```json
{
  "RedisCache": {
    "Enabled": true,
    "ConnectionString": "localhost:6379",
    "InstanceName": "MiProyecto:"
  }
}
```

---

## HTTP Clients con Token Relay

APIs externas reciben automáticamente el token del request entrante:

```csharp
public sealed class GetCatalogHandler : IQueryHandler<GetCatalogQuery, CatalogDto>
{
    private readonly IExternalApiClient _client;

    public async Task<CatalogDto> Handle(GetCatalogQuery query, CancellationToken ct)
    {
        // Token se propaga automáticamente via BearerTokenPropagationHandler
        return await _client.GetAsync<CatalogDto>(
            "CatalogoService",
            $"api/catalog/{query.Id}",
            ct);
    }
}
```

**Resiliencia**: reintentos automáticos con **Polly** (3 intentos, backoff exponencial).

---

## Logging Centralizado

Sistema de logging con **LogCentral** (servicio centralizado) y fallback local:

- Logs se envían a LogCentral via HTTP
- Si LogCentral no está disponible, se encolan localmente (OfflineQueue)
- Middleware automático para logging de requests/responses
- Tipos de log: `Information`, `Warning`, `Error`, `Critical`

---

## Paginación, Filtrado y Ordenamiento

Todos los endpoints de listado soportan paginación, filtrado avanzado y ordenamiento:

```
GET /api/v1/products?pageNumber=1&pageSize=25&name[contains]=Laptop&price[gte]=100&sort=name,-price
```

**Respuesta con envelope**:
```json
{
  "data": [ ... ],
  "meta": {
    "pagination": {
      "currentPage": 1,
      "pageSize": 25,
      "totalCount": 150,
      "totalPages": 6,
      "hasPreviousPage": false,
      "hasNextPage": true
    }
  }
}
```

**Operadores de filtrado**: `eq`, `neq`, `gt`, `gte`, `lt`, `lte`, `contains`, `startswith`, `endswith`.

---

## Testing

**MSTest + Moq + FluentAssertions** con cobertura objetivo ≥ 95%.

```csharp
[TestClass]
public sealed class CreateProductHandlerTests
{
    [TestMethod]
    public async Task Handle_Should_ReturnProductId_When_ValidCommand()
    {
        // Arrange
        var command = new CreateProductCommand("Laptop", 1500m);
        var repoMock = new Mock<IProductRepository>();
        repoMock.Setup(x => x.AddAsync(It.IsAny<Product>())).ReturnsAsync(42);

        var handler = new CreateProductHandler(repoMock.Object, _unitOfWorkMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(42);
    }
}
```

**Convenciones**: un assert lógico por test, `[DataRow]` para parametrización, Fixtures para datos reutilizables.

---

## Configuración por Entorno

Variables de entorno con separador `__` (doble guion bajo):

```bash
# Desarrollo
Jwt__Authority=http://localhost:5001
ConnectionStrings__DefaultConnection=Server=(localdb);Database=MiDb;...

# Producción
Jwt__Authority=https://identity.production.com
Jwt__RequireHttpsMetadata=true
RedisCache__Enabled=true
RedisCache__ConnectionString=redis-cluster:6379
LogCentral__BaseUrl=https://logcentral.production.com
```

Archivos de configuración: `appsettings.json`, `appsettings.Development.json`, `appsettings.Production.json`.

---

## Docker y Deployment

### Docker
```bash
docker build -t mi-api .
docker run -p 8080:8080 \
  -e "Jwt__Authority=http://localhost:5001" \
  -e "ConnectionStrings__DefaultConnection=Server=db;..." \
  mi-api
```

### Docker Compose
```bash
docker-compose up -d
```

### Kubernetes
```bash
kubectl apply -f k8s/
```

---

## Flujo AI-First (Spec-Driven Development)

El proyecto incluye un flujo de desarrollo completo con **GitHub Copilot Agents** que transforma historias de usuario en código productivo:

```
/spec-from-story → /plan-from-spec → /tasks-from-plan → /implement-tasks
```

| Fase | Prompt | Produce |
|:-----|:-------|:--------|
| 1. Especificación | `/spec-from-story` | `specs/active/{ID}/specification.md` |
| 2. Plan | `/plan-from-spec {ID}` | `specs/active/{ID}/plan.md` |
| 3. Tareas | `/tasks-from-plan {ID}` | `specs/active/{ID}/tasks.md` |
| 4. Implementación | `/implement-tasks {ID}` | Código, tests, SQL, docs |

**16 agentes Copilot** incluidos: Spec Builder, Plan Builder, Task Definer, Orchestrator, Domain/Application/Infrastructure/API Implementers, TDD Implementer, SQL Server Implementer, Code Reviewer, Coverage Analyzer, Doc Updater, Explorer.

---

## Convenciones de Código

| Elemento | Convención | Ejemplo |
|:---------|:-----------|:--------|
| Métodos | PascalCase + `Async` | `GetProductAsync` |
| Variables locales | camelCase | `productId` |
| Propiedades | PascalCase | `ProductId` |
| Campos privados | `_camelCase` | `_productRepository` |
| Interfaces | I + PascalCase | `IProductRepository` |
| Clases concretas | `sealed` siempre | `sealed class ProductHandler` |
| Abreviaturas | PascalCase .NET | `Id`, `Url`, `Http`, `Api` (nunca `ID`, `URL`) |
| Código | Inglés | `GetProductAsync` |
| Mensajes throw | Español | `"Producto no encontrado."` |
| Comentarios | Español | `// Obtiene el producto por Id` |

**Prohibiciones**:
- NO Entity Framework (solo Dapper + SqlKata)
- NO MediatR (solo Cortex.Mediator)
- NO SQL crudo (solo SqlKata fluent API)
- NO clases concretas en inyección (solo interfaces)
- NO `null` returns sorpresa
- NO `== true` / `== false`
- NO try/catch en Controllers (middleware maneja excepciones)

---

## Documentación Incluida

El proyecto generado incluye documentación técnica completa en la carpeta `docs/`:

| Documento | Tema |
|:----------|:-----|
| `ARCHITECTURE.md` | Arquitectura Clean Architecture detallada |
| `PATTERNS.md` | Patrones CQRS, Repository, UoW, validación |
| `DATA_ACCESS.md` | Dapper, SqlKata, GenericRepository |
| `AUTHENTICATION.md` | JWT Bearer, OpenIddict, scopes, policies |
| `HTTP_CLIENTS.md` | IExternalApiClient, Token Relay, Polly |
| `CACHING.md` | Redis, Cache-Aside pattern, TTL |
| `LOGGING_CENTRAL.md` | LogCentral, OfflineQueue, middlewares |
| `RESILIENCE.md` | Polly, retry policies, circuit breaker |
| `PAGINATION.md` | Paginación, filtros, ordenamiento, envelope |
| `CONFIGURATION.md` | Variables de entorno, appsettings |
| `DEPLOYMENT.md` | Docker, Kubernetes, docker-compose |
| `API_DOCUMENTATION.md` | ProblemDetails, error handling |
| `TESTING.md` | MSTest, Moq, FluentAssertions, TDD |

---

## Empaquetar y Publicar (Mantenedores)

Cada vez que se modifique el template (código, documentación, instrucciones, etc.) se debe publicar una nueva versión al feed de Azure Artifacts.

### 1. Hacer los cambios

Editar los archivos necesarios en `src/`, `docs/`, `TEMPLATE_README.md`, `.github/`, `tests/`, etc.

> **Nota**: `TEMPLATE_README.md` es el archivo que se muestra en la pestaña **Overview** del paquete en Azure Artifacts. Si cambias documentación relevante para consumidores del template, actualiza este archivo también.

### 2. Incrementar la versión del paquete

Editar `.template.config/OlimpiaIT.CleanApi.Template.csproj` y subir el número de versión:

```xml
<PackageVersion>1.0.2</PackageVersion>  <!-- Incrementar en cada publicación -->
```

> **Importante**: Azure Artifacts **no permite re-publicar la misma versión**. Siempre se debe incrementar el número. Usar [SemVer](https://semver.org/): patch (`1.0.X`) para fixes, minor (`1.X.0`) para features nuevas, major (`X.0.0`) para breaking changes.

### 3. Generar el paquete `.nupkg`

Desde la raíz del repositorio:

```bash
dotnet pack .template.config/OlimpiaIT.CleanApi.Template.csproj -o ./nupkg
```

Verificar que se generó el archivo con la versión correcta:

```bash
ls ./nupkg/
# OlimpiaIT.CleanApi.Template.1.0.2.nupkg
```

### 4. Publicar a Azure Artifacts

```bash
dotnet nuget push ./nupkg/OlimpiaIT.CleanApi.Template.1.0.2.nupkg --source "sicovii" --api-key AzureDevOps
```

> Reemplazar `1.0.2` por la versión que se configuró en el paso 2.

### 5. Verificar en Azure Artifacts

Ir a **Azure DevOps → Artifacts → Feed sicovii → OlimpiaIT.CleanApi.Template** y confirmar que:
- La nueva versión aparece en la pestaña **Versions**
- La documentación se renderiza correctamente en **Overview**

### 6. (Opcional) Actualizar el template en máquinas locales

Los consumidores del template deben actualizar su instalación local:

```bash
dotnet new install OlimpiaIT.CleanApi.Template --nuget-source "https://pkgs.dev.azure.com/olimpiait/9fc7cbdd-2938-4c6e-be8b-989e9e378132/_packaging/sicovii/nuget/v3/index.json" --interactive
```

> `dotnet new install` con el mismo `PackageId` actualiza automáticamente a la última versión.

---

## Desinstalar

```bash
dotnet new uninstall OlimpiaIT.CleanApi.Template
```
