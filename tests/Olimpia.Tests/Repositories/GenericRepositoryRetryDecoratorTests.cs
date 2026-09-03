// Inicio código generado por GitHub Copilot
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Olimpia.Infrastructure.Configuration;
using Olimpia.Infrastructure.Persistence.Decorators;

namespace Olimpia.Tests.Repositories;

[TestClass]
public sealed class GenericRepositoryRetryDecoratorTests
{
    private readonly Mock<global::Olimpia.Domain.Repositories.IGenericRepository<global::Olimpia.Domain.Entities.Product>> _innerRepositoryMock;
    private readonly Mock<ILogger<GenericRepositoryRetryDecorator<global::Olimpia.Domain.Entities.Product>>> _loggerMock;
    private readonly IOptions<RepositoryRetryOptions> _options;
    private readonly GenericRepositoryRetryDecorator<global::Olimpia.Domain.Entities.Product> _decorator;

    public GenericRepositoryRetryDecoratorTests()
    {
        _innerRepositoryMock = new Mock<global::Olimpia.Domain.Repositories.IGenericRepository<global::Olimpia.Domain.Entities.Product>>();
        _loggerMock = new Mock<ILogger<GenericRepositoryRetryDecorator<global::Olimpia.Domain.Entities.Product>>>();
        _options = Options.Create(new RepositoryRetryOptions
        {
            RetryEnabled = true,
            MaxRetryAttempts = 2,
            InitialDelayMs = 0
        });
        _decorator = new GenericRepositoryRetryDecorator<global::Olimpia.Domain.Entities.Product>(
            _innerRepositoryMock.Object,
            _loggerMock.Object,
            _options);
    }

    // === Tests de ESCRITURAS: NO deben reintentar ante errores transitorios ===

    [TestMethod]
    public async Task RetryDecorator_AddAsync_Should_NOT_RetryOnTransientError()
    {
        // Arrange
        var entity = new global::Olimpia.Domain.Entities.Product();
        _innerRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<global::Olimpia.Domain.Entities.Product>()))
            .ThrowsAsync(new TimeoutException());

        // Act
        Func<Task> act = async () => await _decorator.AddAsync(entity);

        // Assert: lanzó la excepción Y se llamó exactamente 1 vez (sin retry)
        await act.Should().ThrowAsync<TimeoutException>();
        _innerRepositoryMock.Verify(r => r.AddAsync(entity), Times.Once);
    }

    [TestMethod]
    public async Task RetryDecorator_UpdateAsync_Should_NOT_RetryOnTransientError()
    {
        // Arrange
        var entity = new global::Olimpia.Domain.Entities.Product();
        _innerRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<global::Olimpia.Domain.Entities.Product>()))
            .ThrowsAsync(new TimeoutException());

        // Act
        Func<Task> act = async () => await _decorator.UpdateAsync(entity);

        // Assert: lanzó la excepción Y se llamó exactamente 1 vez (sin retry)
        await act.Should().ThrowAsync<TimeoutException>();
        _innerRepositoryMock.Verify(r => r.UpdateAsync(entity), Times.Once);
    }

    [TestMethod]
    public async Task RetryDecorator_DeleteAsync_Should_NOT_RetryOnTransientError()
    {
        // Arrange
        _innerRepositoryMock
            .Setup(r => r.DeleteAsync(It.IsAny<int>()))
            .ThrowsAsync(new TimeoutException());

        // Act
        Func<Task> act = async () => await _decorator.DeleteAsync(1);

        // Assert: lanzó la excepción Y se llamó exactamente 1 vez (sin retry)
        await act.Should().ThrowAsync<TimeoutException>();
        _innerRepositoryMock.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    // === Tests de LECTURAS: deben reintentar ante errores transitorios ===

    [TestMethod]
    public async Task RetryDecorator_GetByIdAsync_Should_RetryOnTransientError()
    {
        // Arrange — falla 2 veces, luego tiene éxito
        var entity = new global::Olimpia.Domain.Entities.Product();
        _innerRepositoryMock
            .SetupSequence(r => r.GetByIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new TimeoutException())
            .ThrowsAsync(new TimeoutException())
            .ReturnsAsync(entity);

        // Act
        var result = await _decorator.GetByIdAsync(1);

        // Assert: se llamó 3 veces (2 reintentos + 1 éxito)
        result.Should().Be(entity);
        _innerRepositoryMock.Verify(r => r.GetByIdAsync(1), Times.Exactly(3));
    }

    [TestMethod]
    public async Task RetryDecorator_GetAllAsync_Should_RetryOnTransientError()
    {
        // Arrange — falla 1 vez, luego tiene éxito
        var products = new List<global::Olimpia.Domain.Entities.Product>();
        _innerRepositoryMock
            .SetupSequence(r => r.GetAllAsync())
            .ThrowsAsync(new TimeoutException())
            .ReturnsAsync(products);

        // Act
        var result = await _decorator.GetAllAsync();

        // Assert: se llamó 2 veces (1 reintento + 1 éxito)
        result.Should().BeSameAs(products);
        _innerRepositoryMock.Verify(r => r.GetAllAsync(), Times.Exactly(2));
    }

    [TestMethod]
    public async Task RetryDecorator_GetPagedAsync_Should_RetryOnTransientError()
    {
        // Arrange — falla 1 vez, luego tiene éxito
        var pagedResult = (Data: Enumerable.Empty<global::Olimpia.Domain.Entities.Product>(), TotalCount: 0);
        _innerRepositoryMock
            .SetupSequence(r => r.GetPagedAsync(
                It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<IReadOnlyList<global::Olimpia.Domain.Common.FilterCriteria>?>(),
                It.IsAny<IReadOnlyList<global::Olimpia.Domain.Common.SortCriteria>?>()))
            .ThrowsAsync(new TimeoutException())
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _decorator.GetPagedAsync(1, 10, null, null);

        // Assert: se llamó 2 veces (1 reintento + 1 éxito)
        result.TotalCount.Should().Be(0);
        _innerRepositoryMock.Verify(r => r.GetPagedAsync(1, 10, null, null), Times.Exactly(2));
    }

    // === Smoke tests: delegación correcta ===

    [TestMethod]
    public async Task RetryDecorator_GetByIdAsync_Should_DelegateToInnerRepository()
    {
        // Arrange
        var entity = new global::Olimpia.Domain.Entities.Product();
        _innerRepositoryMock
            .Setup(r => r.GetByIdAsync(5))
            .ReturnsAsync(entity);

        // Act
        var result = await _decorator.GetByIdAsync(5);

        // Assert
        result.Should().Be(entity);
        _innerRepositoryMock.Verify(r => r.GetByIdAsync(5), Times.Once);
    }

    [TestMethod]
    public async Task RetryDecorator_AddAsync_Should_DelegateToInnerRepository()
    {
        // Arrange
        var entity = new global::Olimpia.Domain.Entities.Product();
        _innerRepositoryMock
            .Setup(r => r.AddAsync(entity))
            .ReturnsAsync(42);

        // Act
        var result = await _decorator.AddAsync(entity);

        // Assert
        result.Should().Be(42);
        _innerRepositoryMock.Verify(r => r.AddAsync(entity), Times.Once);
    }

    [TestMethod]
    public async Task RetryDecorator_UpdateAsync_Should_DelegateToInnerRepository()
    {
        // Arrange
        var entity = new global::Olimpia.Domain.Entities.Product();
        _innerRepositoryMock
            .Setup(r => r.UpdateAsync(entity))
            .ReturnsAsync(true);

        // Act
        var result = await _decorator.UpdateAsync(entity);

        // Assert
        result.Should().BeTrue();
        _innerRepositoryMock.Verify(r => r.UpdateAsync(entity), Times.Once);
    }

    [TestMethod]
    public async Task RetryDecorator_DeleteAsync_Should_DelegateToInnerRepository()
    {
        // Arrange
        _innerRepositoryMock
            .Setup(r => r.DeleteAsync(7))
            .ReturnsAsync(true);

        // Act
        var result = await _decorator.DeleteAsync(7);

        // Assert
        result.Should().BeTrue();
        _innerRepositoryMock.Verify(r => r.DeleteAsync(7), Times.Once);
    }
}
// Fin código generado por GitHub Copilot
