---
name: tdd-workflow
description: "Flujo TDD Red→Green→Refactor, patrones de testing unitario con MSTest + Moq + FluentAssertions y cobertura ≥95% para Olimpia."
---

# Skill: TDD y Testing — Olimpia

**Meta de cobertura:** ≥95% line coverage en archivos nuevos.
**Herramientas:** MSTest + Moq + FluentAssertions + coverlet.collector.

## Ciclo TDD

**Fase RED:** Escribe tests que definan el comportamiento ANTES de implementar. Domain/interfaces deben existir y compilar. Tests deben compilar y FALLAR al ejecutar (no por error de compilación). NO crear stubs vacíos ni código de producción.

**Fase GREEN:** Implementación mínima para que todos los tests pasen. NO agregar funcionalidad extra ni refactorizar. Verifica: `dotnet test` — todos PASAN.

**Fase REFACTOR:** Con tests en verde, extrae métodos, simplifica condicionales, mejora nombres, elimina código muerto. Ejecuta `dotnet test` tras CADA cambio; si falla, revierte inmediatamente.

## Estructura del Proyecto de Tests

> **ADVERTENCIA CRÍTICA:** La estructura de tests es **por capa** (`Handlers/`, `Validators/`, `Repositories/`, `Fixtures/`)

Estructura **por capa** (NO por feature):

```
tests/Olimpia.Tests/
├── MSTestSettings.cs
├── Handlers/                    ← Tests de command y query handlers
│   ├── Products/
│   │   ├── CreateProductHandlerTests.cs
│   │   └── GetProductHandlerTests.cs
│   └── {Feature}/
│       └── Create{Feature}HandlerTests.cs
├── Validators/                  ← Tests de validators
│   └── Create{Feature}ValidatorTests.cs
├── Repositories/                ← Tests de repositorios
│   └── {Feature}RepositoryTests.cs
└── Fixtures/
    ├── ProductFixture.cs
    └── MockFactory.cs
```

**NUNCA** uses estructura por feature (`{Feature}/Commands/` o `{Feature}/Queries/`). Usa siempre `Handlers/{Feature}/`, `Validators/`, `Repositories/`.

## Test de Command Handler (con Rollback)

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

    [TestMethod]
    public async Task Handle_Should_ReturnProductId_When_ValidCommand()
    {
        // Arrange
        var command = new CreateProductCommand("Laptop", "Gaming laptop", 1500m, 10);
        _repositoryMock
            .Setup(x => x.AddAsync(It.IsAny<global::Olimpia.Domain.Entities.Product>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeGreaterThan(0);
        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<global::Olimpia.Domain.Entities.Product>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.BeginTransactionAsync(), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);
        _unitOfWorkMock.Verify(x => x.RollbackAsync(), Times.Never);
    }

    [TestMethod]
    public async Task Handle_Should_RollbackAndRethrow_When_RepositoryThrows()
    {
        // Arrange
        var command = new CreateProductCommand("Laptop", "Desc", 1500m, 10);
        _repositoryMock
            .Setup(x => x.AddAsync(It.IsAny<global::Olimpia.Domain.Entities.Product>()))
            .ThrowsAsync(new Exception("DB error"));

        // Act & Assert
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>().WithMessage("DB error");
        _unitOfWorkMock.Verify(x => x.RollbackAsync(), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Never);
    }
}
```

## Test de Query Handler

```csharp
[TestClass]
public sealed class GetProductHandlerTests
{
    [TestMethod]
    public async Task Handle_Should_ReturnDto_When_ProductExists()
    {
        // Arrange
        var product = ProductFixture.CreateValid(id: 1, name: "Laptop", price: 1500m);
        var repoMock = new Mock<IProductRepository>();
        repoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(product);
        var handler = new GetProductHandler(repoMock.Object);

        // Act
        var result = await handler.Handle(new GetProductQuery(1), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be("Laptop");
    }

    [TestMethod]
    public async Task Handle_Should_ThrowKeyNotFoundException_When_ProductNotFound()
    {
        // Arrange
        var repoMock = new Mock<IProductRepository>();
        repoMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((global::Olimpia.Domain.Entities.Product?)null);
        var handler = new GetProductHandler(repoMock.Object);

        // Act
        var act = () => handler.Handle(new GetProductQuery(999), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
```

## Test de Validator (con DataRow)

```csharp
[TestClass]
public sealed class CreateProductCommandValidatorTests
{
    private readonly CreateProductCommandValidator _validator = new();

    [TestMethod]
    public void Validate_Should_BeValid_When_AllFieldsCorrect()
    {
        var result = _validator.Validate(new CreateProductCommand("Laptop", "Desc", 1500m, 10));
        result.IsValid.Should().BeTrue();
    }

    [TestMethod]
    [DataRow("", DisplayName = "Empty name")]
    [DataRow(null, DisplayName = "Null name")]
    public void Validate_Should_HaveError_When_NameIsInvalid(string? name)
    {
        var result = _validator.Validate(new CreateProductCommand(name!, "Desc", 1500m, 10));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    public void Validate_Should_HaveError_When_PriceIsInvalid(double price)
    {
        var result = _validator.Validate(new CreateProductCommand("Laptop", "Desc", (decimal)price, 10));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Price");
    }
}
```

## Fixtures y MockFactory

```csharp
// Fixtures/ProductFixture.cs
public static class ProductFixture
{
    public static global::Olimpia.Domain.Entities.Product CreateValid(
        int id = 1, string name = "Test Product", decimal price = 99.99m)
    {
        return new global::Olimpia.Domain.Entities.Product
        {
            Id = id, Name = name, Price = price, Stock = 10, Description = "Test"
        };
    }
}

// Fixtures/MockFactory.cs
public static class MockFactory
{
    public static Mock<IUnitOfWork> CreateUnitOfWork()
    {
        var mock = new Mock<IUnitOfWork>();
        mock.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
        mock.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);
        mock.Setup(x => x.RollbackAsync()).Returns(Task.CompletedTask);
        return mock;
    }
}
```

## FluentAssertions — Referencia

```csharp
result.Should().Be(42);
result.Should().NotBeNull();
list.Should().HaveCount(3);
list.Should().Contain(x => x.Name == "Laptop");
await act.Should().ThrowAsync<KeyNotFoundException>();
result.Should().BeOfType<ProductDto>();
result.Should().BeEquivalentTo(expected);
```

## Convenciones en Tests

- Framework: MSTest (`[TestClass]`, `[TestMethod]`)
- Mocking: Moq (`Mock<T>`, `Setup`, `Verify`)
- Assertions: FluentAssertions (`.Should()`)
- Entidades: `global::Olimpia.Domain.Entities.{Nombre}`
- Clases de test: `sealed`, inicialización en constructor
- Naming: `Handle_Should_Result_When_Scenario` (el método real se llama `Handle`)
- Organización: Por capa — `Handlers/{Feature}/`, `Validators/`, `Repositories/`, `Fixtures/`

## Coverage Analyzer

Al finalizar Red → Green → Refactor, el Orchestrator invoca al Coverage Analyzer (`dotnet test --collect:"XPlat Code Coverage"`). Si cobertura < 95%, vuelve a Fase RED con tests adicionales. Repite hasta ≥95% en archivos nuevos.
