# Testing - MSTest, Moq, FluentAssertions

Estrategia de testing completa para Olimpia API con enfoque en handlers, repositorios y servicios.

**Meta de cobertura:** ≥95% line coverage en archivos nuevos.

---

## Stack de Testing

| Herramienta | Propósito |
|-------------|----------|
| **MSTest** | Framework de testing (atributos `[TestClass]`, `[TestMethod]`) |
| **Moq** | Mocking (crear mocks de dependencias) |
| **FluentAssertions** | Assertions legibles (`Should().Be()`) |
| **coverlet.collector** | Recopilación de cobertura de código |

---

## Reglas Estrictas

| Regla | Descripción |
|-------|-------------|
| **Un assert lógico** por test | Validar **un solo concepto**. Para verificar un DTO completo, usar `result.Should().BeEquivalentTo(expected)` (cuenta como un assert lógico porque valida la forma completa del objeto). |
| Sin lógica condicional | No usar `if`, `switch`, bucles o ternarios dentro de tests. |
| Escenarios múltiples con `[DataRow]` | En vez de ramas condicionales, separar escenarios vía `[TestMethod]` + `[DataRow(...)]`. |
| Sin magic strings | Preferir constantes o variables con nombre descriptivo. |
| `sealed` en clases de test | Todas las clases `[TestClass]` son `sealed`. |
| Preferir constructor | Inicialización vía constructor sobre `[TestInitialize]`. |
| Framework oficial | **MSTest** (no xUnit ni NUnit). Ver `tests/Olimpia.Tests/Olimpia.Tests.csproj`. |

### Características de Toda Prueba (FIRST)

| Característica | Descripción |
|----------------|-------------|
| **Fast** | Menos de 200ms por test |
| **Isolated** | Sin dependencias entre tests |
| **Repeatable** | Mismo resultado siempre |
| **Self-validating** | Pasa o falla sin inspección manual |
| **Timely** | Se escribe junto al código de producción |

---

## 1. Estructura de Carpetas

```
tests/
└── Olimpia.Tests/
    ├── Handlers/
    │   └── Products/
    │       ├── CreateProductHandlerTests.cs
    │       ├── GetAllProductsHandlerTests.cs
    │       ├── GetProductByIdHandlerTests.cs
    │       ├── PagedResultTests.cs
    │       ├── PagedEnvelopeTests.cs
    │       └── ProductControllerTests.cs
    ├── Repositories/
    │   └── GenericRepositoryRetryDecoratorTests.cs
    ├── Validators/
    │   ├── GetAllProductsValidatorTests.cs
    │   └── GetProductByIdValidatorTests.cs
    ├── Infrastructure/
    │   └── QueryStringFilterParserTests.cs
    ├── Fixtures/
    ├── Integration/
    └── TestResults/
```

---

## 2. Convenciones

### Nombres de Tests

```csharp
[TestClass]
public sealed class CreateProductHandlerTests
{
    // [Handle_Should_Result_When_Scenario]
    [TestMethod]
    public async Task Handle_Should_ReturnProductId_When_ValidCommand() { }

    [TestMethod]
    public async Task Handle_Should_ThrowInvalidOperationException_When_DuplicateName() { }

    [TestMethod]
    public async Task Handle_Should_ThrowArgumentException_When_InvalidPrice() { }
}
```

### Estructura AAA (Arrange-Act-Assert)

```csharp
[TestMethod]
public async Task Handle_Should_ReturnProductId_When_ValidCommand()
{
    // Arrange: preparar datos y mocks
    var command = new CreateProductCommand("Laptop", "Good laptop", 1500m, 5);
    var mockRepository = new Mock<IProductRepository>();
    var mockUnitOfWork = new Mock<IUnitOfWork>();
    var handler = new CreateProductHandler(mockRepository.Object, mockUnitOfWork.Object);

    // Act: ejecutar lo que se está probando
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert: verificar el resultado
    result.Should().BeGreaterThan(0);
    mockRepository.Verify(x => x.AddAsync(It.IsAny<global::Olimpia.Domain.Entities.Product>()));
    mockUnitOfWork.Verify(x => x.CommitAsync());
}
```

---

## 3. Testing de Handlers

### Command Handler - Happy Path

```csharp
[TestClass]
public sealed class CreateProductHandlerTests
{
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateProductHandler _handler;

    public CreateProductHandlerTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new CreateProductHandler(_repositoryMock.Object, _unitOfWorkMock.Object);
    }

    // Método generado por GitHub Copilot
    [TestMethod]
    public async Task Handle_Should_ReturnProductId_When_ValidCommand()
    {
        // Arrange
        var command = new CreateProductCommand(
            "Laptop Dell",
            "High-performance laptop",
            1500m,
            10);

        _repositoryMock
            .Setup(x => x.AddAsync(It.IsAny<global::Olimpia.Domain.Entities.Product>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(1);
        
        _repositoryMock.Verify(
            x => x.AddAsync(It.Is<global::Olimpia.Domain.Entities.Product>(p => p.Name == "Laptop Dell")),
            Times.Once);
        
        _unitOfWorkMock.Verify(x => x.BeginTransactionAsync(), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);
    }

    [TestMethod]
    public async Task Handle_Should_ThrowArgumentException_When_NegativePrice()
    {
        // Arrange
        var command = new CreateProductCommand("Product", "Desc", -100m, 5);

        // Act & Assert
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [TestMethod]
    public async Task Handle_Should_CallRollback_When_RepositoryFails()
    {
        // Arrange
        var command = new CreateProductCommand("Laptop", "Desc", 1500m, 10);

        _repositoryMock
            .Setup(x => x.AddAsync(It.IsAny<global::Olimpia.Domain.Entities.Product>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));

        // Act & Assert
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        _unitOfWorkMock.Verify(x => x.RollbackAsync(), Times.Once);
    }
    // Fin código generado por GitHub Copilot
}
```

### Query Handler con Caché

```csharp
[TestClass]
public sealed class GetProductHandlerTests
{
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly Mock<ILogger<GetProductHandler>> _loggerMock;
    private readonly GetProductHandler _handler;

    public GetProductHandlerTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _cacheMock = new Mock<IDistributedCache>();
        _loggerMock = new Mock<ILogger<GetProductHandler>>();
        _handler = new GetProductHandler(
            _repositoryMock.Object,
            _cacheMock.Object,
            _loggerMock.Object);
    }

    [TestMethod]
    public async Task Handle_Should_ReturnFromCache_When_CacheHit()
    {
        // Arrange
        var query = new GetProductQuery(1);
        var cached = JsonSerializer.Serialize(
            new ProductDto { Id = 1, Name = "Cached Product" });

        _cacheMock
            .Setup(x => x.GetStringAsync("product:1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cached);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Name.Should().Be("Cached Product");
        
        // Verificar que NO llamó a repositorio
        _repositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<int>()),
            Times.Never);
    }

    [TestMethod]
    public async Task Handle_Should_FetchFromRepository_When_CacheMiss()
    {
        // Arrange
        var query = new GetProductQuery(1);
        var product = new global::Olimpia.Domain.Entities.Product { Id = 1, Name = "DB Product", Price = 100m };

        _cacheMock
            .Setup(x => x.GetStringAsync("product:1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(product);

        _cacheMock
            .Setup(x => x.SetStringAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Name.Should().Be("DB Product");
        
        _repositoryMock.Verify(x => x.GetByIdAsync(1), Times.Once);
        _cacheMock.Verify(
            x => x.SetStringAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [TestMethod]
    public async Task Handle_Should_ThrowKeyNotFoundException_When_NotFound()
    {
        // Arrange
        var query = new GetProductQuery(999);

        _cacheMock
            .Setup(x => x.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(999))
            .ReturnsAsync((global::Olimpia.Domain.Entities.Product?)null);

        // Act & Assert
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
```

---

## 4. Testing de Repositorios

```csharp
[TestClass]
public sealed class ProductRepositoryTests
{
    private readonly Mock<QueryFactory> _dbMock;
    private readonly Mock<UnitOfWork> _unitOfWorkMock;
    private readonly ProductRepository _repository;

    public ProductRepositoryTests()
    {
        _dbMock = new Mock<QueryFactory>();
        _unitOfWorkMock = new Mock<UnitOfWork>();
        _repository = new ProductRepository(_dbMock.Object, _unitOfWorkMock.Object);
    }

    [TestMethod]
    public async Task GetByIdAsync_WithValidId_ReturnsProduct()
    {
        // Arrange
        var product = new global::Olimpia.Domain.Entities.Product { Id = 1, Name = "Test Product", Price = 100m };

        var queryBuilderMock = new Mock<IQueryBuilder>();
        queryBuilderMock
            .Setup(x => x.FirstOrDefaultAsync<global::Olimpia.Domain.Entities.Product>(It.IsAny<SqlTransaction>()))
            .ReturnsAsync(product);

        _dbMock
            .Setup(x => x.Query("Products"))
            .Returns(queryBuilderMock.Object);

        // Act
        var result = await _repository.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be("Test Product");
    }

    [TestMethod]
    public async Task AddAsync_WithValidProduct_ReturnsId()
    {
        // Arrange
        var product = new global::Olimpia.Domain.Entities.Product { Name = "New Product", Price = 200m };

        var queryBuilderMock = new Mock<IQueryBuilder>();
        queryBuilderMock
            .Setup(x => x.InsertGetIdAsync<int>(
                It.IsAny<Dictionary<string, object?>>(),
                It.IsAny<SqlTransaction>()))
            .ReturnsAsync(5);

        _dbMock
            .Setup(x => x.Query("Products"))
            .Returns(queryBuilderMock.Object);

        // Act
        var id = await _repository.AddAsync(product);

        // Assert
        id.Should().Be(5);
        product.Id.Should().Be(5);
    }
}
```

---

## 5. Testing de Validators

```csharp
[TestClass]
public sealed class CreateProductValidatorTests
{
    private readonly CreateProductValidator _validator = new();

    [TestMethod]
    public void Validate_ValidCommand_HasNoErrors()
    {
        // Arrange
        var command = new CreateProductCommand(
            "Laptop",
            "Good laptop",
            1500m,
            10);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [TestMethod]
    public void Validate_EmptyName_HasErrors()
    {
        // Arrange
        var command = new CreateProductCommand(
            "",  // Nombre vacío
            "Desc",
            1500m,
            10);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Name");
    }

    // Nota: [DataTestMethod] está deprecado (MSTEST0044). Usar [TestMethod] + [DataRow].
    [TestMethod]
    [DataRow(-100)]
    [DataRow(0)]
    public void Validate_InvalidPrice_HasErrors(decimal price)
    {
        // Arrange
        var command = new CreateProductCommand("Laptop", "Desc", price, 10);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Price");
    }
}
```

---

## 6. Fixtures (Datos de Prueba)

```csharp
// tests/Olimpia.Tests/Fixtures/ProductFixture.cs
public static class ProductFixture
{
    public static global::Olimpia.Domain.Entities.Product CreateValid(
        int id = 1,
        string name = "Default Product",
        decimal price = 100m,
        int stock = 10)
    {
        return new global::Olimpia.Domain.Entities.Product
        {
            Id = id,
            Name = name,
            Description = "Test product",
            Price = price,
            Stock = stock,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static CreateProductCommand CreateValidCommand(
        string name = "Test Product",
        decimal price = 100m,
        int stock = 10)
    {
        return new CreateProductCommand(
            name,
            "Test description",
            price,
            stock);
    }

    public static IEnumerable<global::Olimpia.Domain.Entities.Product> CreateProductList(int count = 5)
    {
        return Enumerable.Range(1, count)
            .Select(i => CreateValid(
                id: i,
                name: $"Product {i}",
                price: 100m * i))
            .ToList();
    }
}

// Uso
[TestMethod]
public async Task Handle_Should_ReturnProductId_When_ValidCommand()
{
    // Arrange
    var command = ProductFixture.CreateValidCommand();
    // ...
}
```

---

## 7. MockFactory

```csharp
// tests/Olimpia.Tests/Fixtures/MockFactory.cs
public static class MockFactory
{
    public static Mock<IProductRepository> CreateProductRepositoryMock()
    {
        var mock = new Mock<IProductRepository>();
        
        // Configurar comportamiento por defecto
        mock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => ProductFixture.CreateValidProduct(id: id));

        return mock;
    }

    public static Mock<IUnitOfWork> CreateUnitOfWorkMock()
    {
        var mock = new Mock<IUnitOfWork>();
        
        mock
            .Setup(x => x.BeginTransactionAsync())
            .Returns(Task.CompletedTask);

        mock
            .Setup(x => x.CommitAsync())
            .Returns(Task.CompletedTask);

        mock
            .Setup(x => x.RollbackAsync())
            .Returns(Task.CompletedTask);

        return mock;
    }

    public static Mock<IDistributedCache> CreateCacheMock()
    {
        var mock = new Mock<IDistributedCache>();
        
        // Por defecto: no hay nada en caché
        mock
            .Setup(x => x.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        mock
            .Setup(x => x.SetStringAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return mock;
    }
}

// Uso
[TestMethod]
public async Task Handle_Should_ReturnProductId_When_ValidCommand()
{
    // Arrange
    var repositoryMock = MockFactory.CreateProductRepositoryMock();
    var unitOfWorkMock = MockFactory.CreateUnitOfWorkMock();
    // ...
}
```

---

## 8. Integration Tests

```csharp
[TestClass]
public sealed class ProductControllerIntegrationTests
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;

    [TestInitialize]
    public void Setup()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Reemplazar servicios reales por mocks para testing
                    var descriptor = services
                        .SingleOrDefault(d => d.ServiceType == typeof(IProductRepository));
                    
                    if (descriptor != null)
                        services.Remove(descriptor);

                    services.AddScoped(_ =>
                        MockFactory.CreateProductRepositoryMock().Object);
                });
            });

        _client = _factory.CreateClient();
    }

    [TestMethod]
    public async Task GetProductById_WithValidId_Returns200()
    {
        // Arrange
        var token = JwtTokenGenerator.GenerateTestToken(scopes: "products.read");
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("api/products/1");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsAsync<ProductDto>();
        content.Id.Should().Be(1);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }
}
```

---

## 8.1 Un assert lógico: patrón `BeEquivalentTo` para DTOs

La regla "un assert lógico por test" no significa "un solo `.Should()`". Significa **validar un solo concepto**. Cuando el concepto es "el handler devuelve el DTO esperado", el test debe afirmar la forma completa del DTO en **una sola llamada** con `BeEquivalentTo`.

### ❌ Antes — N asserts, ruido al fallar

```csharp
[TestMethod]
public async Task Handle_Should_ReturnProductDto_When_ProductExists()
{
    // Arrange...
    var result = await _handler.Handle(query, CancellationToken.None);

    // Assert — 7 asserciones, si falla la primera no sabes si las demás también fallan
    result.Id.Should().Be(1);
    result.Name.Should().Be("Laptop");
    result.Description.Should().Be("High-end");
    result.Price.Should().Be(1499.99m);
    result.Stock.Should().Be(5);
    result.CreatedAt.Should().Be(createdAt);
    result.UpdatedAt.Should().Be(updatedAt);
}
```

### ✅ Después — un assert lógico con reporte completo al fallar

```csharp
[TestMethod]
public async Task Handle_Should_ReturnProductDto_When_ProductExists()
{
    // Arrange
    var createdAt = new DateTime(2026, 1, 1);
    var updatedAt = new DateTime(2026, 2, 1);
    var expected = new ProductDto(1, "Laptop", "High-end", 1499.99m, 5, createdAt, updatedAt);

    _repositoryMock.Setup(x => x.GetByIdAsync(1))
        .ReturnsAsync(new global::Olimpia.Domain.Entities.Product { /* ... */ });

    // Act
    var result = await _handler.Handle(new GetProductByIdQuery(1), CancellationToken.None);

    // Assert — un assert lógico: "el DTO coincide íntegramente con lo esperado".
    result.Should().BeEquivalentTo(expected);
}
```

**Cuándo usar cada forma:**

| Forma | Úsala cuando… |
|-------|---------------|
| `BeEquivalentTo(expected)` | Verificas el **estado completo** de un DTO/entidad retornada. |
| `Be(value)` único | Verificas **un valor escalar** (int, bool, enum, id). |
| `Verify(..., Times.Once)` | Es **parte del mismo concepto** que el assert del resultado (fan-out de mocks se permite, pero no mezclar verificaciones de dos conceptos distintos en el mismo test). |
| Dos tests separados | Estás verificando **dos conceptos diferentes** (ej. "retorna DTO" y "persiste en DB" son dos tests, no uno). |

### Opciones útiles de `BeEquivalentTo`

```csharp
// Ignorar timestamps generados dinámicamente.
result.Should().BeEquivalentTo(expected, opts => opts.Excluding(x => x.UpdatedAt));

// Comparar colecciones ignorando orden.
result.Items.Should().BeEquivalentTo(expected.Items, opts => opts.WithoutStrictOrdering());

// Tolerancia para decimales / fechas.
result.Should().BeEquivalentTo(expected, opts => opts
    .Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromSeconds(1)))
    .WhenTypeIs<DateTime>());
```

---

## 9. Datos con [TestMethod] + [DataRow]

> **Nota:** `[DataTestMethod]` está deprecado desde MSTest 3.10 (MSTEST0044). Usar `[TestMethod]` + `[DataRow]`.

```csharp
[TestClass]
public sealed class PriceValidationTests
{
    private readonly CreateProductValidator _validator = new();

    [TestMethod]
    [DataRow("Product1", 100, 5, true)]
    [DataRow("Product2", 0, 10, false)]      // Precio 0 inválido
    [DataRow("", 100, 10, false)]             // Nombre vacío
    [DataRow("Product3", -50, 5, false)]      // Precio negativo
    public void Validate_VariousInputs_ResultsAsExpected(
        string name, decimal price, int stock, bool expectedValid)
    {
        // Arrange
        var command = new CreateProductCommand(name, "Desc", price, stock);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().Be(expectedValid);
    }
}
```

---

## 10. Ejecución y Coverage

### Ejecutar Tests

```bash
# Todos los tests
dotnet test

# Solo una clase
dotnet test --filter "ClassName=CreateProductHandlerTests"

# Con logging
dotnet test --logger "console;verbosity=detailed"

# Con cobertura (coverlet)
dotnet test --collect:"XPlat Code Coverage" --settings tests/Olimpia.Tests/coverage.runsettings
```

### Exclusiones de Cobertura

Los siguientes archivos se excluyen del cálculo:
- `Program.cs` — punto de entrada, no lógica testeable.
- `DependencyInjection.cs` — registro de servicios.
- Assemblies de test y logging.

---

## Buenas Prácticas

| Recomendación | Razón |
|---------------|-------|
| ✅ Una asserción por test | Claridad en qué falló |
| ✅ Mocks en dependencies | Aislar unit under test |
| ✅ Usar Fixtures | DRY, datos reutilizables |
| ✅ Nombre descriptivo | Claridad sin leer el código |
| ❌ Múltiples asserciones | Difícil debuggear |
| ❌ Datos hardcoded | Difícil mantener |
| ✅ Verificar mocks | Asegurar llamadas correctas |
| ✅ Test casos edge | Boundary conditions |

---

## Próximos Pasos

- **[PATTERNS.md §7](PATTERNS.md#7-convenciones-de-código-c-code-style)** - Convenciones C# (A1–A18) que también aplican a tests.
- **[API_DOCUMENTATION.md](API_DOCUMENTATION.md)** - XML docs obligatorias en contratos API.
- **[CONFIGURATION.md](CONFIGURATION.md)** - Variables de entorno para testing.
