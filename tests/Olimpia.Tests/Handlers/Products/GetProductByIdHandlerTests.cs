// Inicio código generado por GitHub Copilot
using FluentAssertions;
using Moq;
using Olimpia.Application.Products;
using Olimpia.Application.Products.Queries.GetProductById;
using Olimpia.Domain.Repositories;

namespace Olimpia.Tests.Handlers.Products;

[TestClass]
public sealed class GetProductByIdHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly GetProductByIdHandler _handler;

    public GetProductByIdHandlerTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _handler = new GetProductByIdHandler(_productRepositoryMock.Object);
    }

    [TestMethod]
    public async Task Handle_Should_ReturnProductDto_When_ProductExists()
    {
        // Arrange
        var createdAt = new DateTime(2025, 6, 1, 8, 0, 0, DateTimeKind.Unspecified);
        var updatedAt = new DateTime(2025, 6, 10, 9, 0, 0, DateTimeKind.Unspecified);

        var product = new global::Olimpia.Domain.Entities.Product
        {
            Id = 1,
            Name = "Monitor 4K",
            Description = "UHD 27 pulgadas",
            Price = 799.99m,
            Stock = 8,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        var expected = new ProductDto(
            Id: 1,
            Name: "Monitor 4K",
            Description: "UHD 27 pulgadas",
            Price: 799.99m,
            Stock: 8,
            CreatedAt: createdAt,
            UpdatedAt: updatedAt);

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(product);

        var query = new GetProductByIdQuery(1);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert: un assert lógico — el DTO coincide íntegramente con lo esperado.
        result.Should().BeEquivalentTo(expected);
    }

    [TestMethod]
    public async Task Handle_Should_ThrowKeyNotFoundException_When_ProductNotFound()
    {
        // Arrange
        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(99))
            .ReturnsAsync((global::Olimpia.Domain.Entities.Product?)null);

        var query = new GetProductByIdQuery(99);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
// Fin código generado por GitHub Copilot
