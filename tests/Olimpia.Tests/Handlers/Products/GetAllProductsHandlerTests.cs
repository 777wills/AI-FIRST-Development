// Inicio código generado por GitHub Copilot
using FluentAssertions;
using Moq;
using Olimpia.Application.Products;
using Olimpia.Application.Products.Queries.GetAllProducts;
using Olimpia.Domain.Repositories;
using global::Olimpia.Domain.Common;

namespace Olimpia.Tests.Handlers.Products;

[TestClass]
public sealed class GetAllProductsHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly GetAllProductsHandler _handler;

    public GetAllProductsHandlerTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _handler = new GetAllProductsHandler(_productRepositoryMock.Object);
    }

    [TestMethod]
    public async Task Handle_Should_ReturnPagedResult_When_ProductsExist()
    {
        // Arrange
        var products = new List<global::Olimpia.Domain.Entities.Product>
        {
            new() { Id = 1, Name = "Laptop", Description = "Gaming", Price = 1500m, Stock = 5, CreatedAt = DateTime.Now },
            new() { Id = 2, Name = "Mouse", Description = "Wireless", Price = 50m, Stock = 20, CreatedAt = DateTime.Now }
        };

        _productRepositoryMock
            .Setup(x => x.GetPagedAsync(1, 25, null, It.IsAny<IReadOnlyList<SortCriteria>?>()))
            .ReturnsAsync((products, 10));

        var query = new GetAllProductsQuery(1, 25);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Data.Count().Should().Be(2);
        result.TotalCount.Should().Be(10);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(25);
    }

    [TestMethod]
    public async Task Handle_Should_ReturnEmptyPagedResult_When_NoProductsFound()
    {
        // Arrange
        _productRepositoryMock
            .Setup(x => x.GetPagedAsync(1, 25, null, It.IsAny<IReadOnlyList<SortCriteria>?>()))
            .ReturnsAsync((new List<global::Olimpia.Domain.Entities.Product>(), 0));

        var query = new GetAllProductsQuery(1, 25);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Data.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    [TestMethod]
    public async Task Handle_Should_PassFiltersAndSortToRepository()
    {
        // Arrange
        var filters = new List<FilterCriteria>
        {
            new("Name", FilterOperator.Contains, "Laptop")
        };
        var sort = new List<SortCriteria>
        {
            new("Price", false)
        };

        IReadOnlyList<FilterCriteria>? capturedFilters = null;
        IReadOnlyList<SortCriteria>? capturedSort = null;

        _productRepositoryMock
            .Setup(x => x.GetPagedAsync(
                1, 25,
                It.IsAny<IReadOnlyList<FilterCriteria>?>(),
                It.IsAny<IReadOnlyList<SortCriteria>?>()))
            .Callback<int, int, IReadOnlyList<FilterCriteria>?, IReadOnlyList<SortCriteria>?>(
                (_, _, f, s) => { capturedFilters = f; capturedSort = s; })
            .ReturnsAsync((new List<global::Olimpia.Domain.Entities.Product>(), 0));

        var query = new GetAllProductsQuery(1, 25, filters, sort);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        capturedFilters.Should().BeEquivalentTo(filters);
        capturedSort.Should().BeEquivalentTo(sort);
    }

    [TestMethod]
    public async Task Handle_Should_MapProductToProductDtoCorrectly()
    {
        // Arrange
        var createdAt = new DateTime(2025, 1, 15, 10, 0, 0, DateTimeKind.Unspecified);
        var updatedAt = new DateTime(2025, 3, 20, 12, 0, 0, DateTimeKind.Unspecified);

        var product = new global::Olimpia.Domain.Entities.Product
        {
            Id = 42,
            Name = "Teclado Mecánico",
            Description = "RGB retroiluminado",
            Price = 299.99m,
            Stock = 15,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        _productRepositoryMock
            .Setup(x => x.GetPagedAsync(1, 25, null, It.IsAny<IReadOnlyList<SortCriteria>?>()))
            .ReturnsAsync((new List<global::Olimpia.Domain.Entities.Product> { product }, 1));

        var query = new GetAllProductsQuery(1, 25);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        var dto = result.Data.Single();

        // Assert
        dto.Id.Should().Be(42);
        dto.Name.Should().Be("Teclado Mecánico");
        dto.Description.Should().Be("RGB retroiluminado");
        dto.Price.Should().Be(299.99m);
        dto.Stock.Should().Be(15);
        dto.CreatedAt.Should().Be(createdAt);
        dto.UpdatedAt.Should().Be(updatedAt);
    }

    // Inicio código generado por GitHub Copilot
    [TestMethod]
    public async Task Handle_Should_UseDefaultSortByCreatedAtDescending_When_SortFieldsIsNull()
    {
        // Arrange
        IReadOnlyList<SortCriteria>? capturedSort = null;

        _productRepositoryMock
            .Setup(x => x.GetPagedAsync(
                1, 25, null,
                It.IsAny<IReadOnlyList<SortCriteria>?>()))
            .Callback<int, int, IReadOnlyList<FilterCriteria>?, IReadOnlyList<SortCriteria>?>(
                (_, _, _, s) => capturedSort = s)
            .ReturnsAsync((new List<global::Olimpia.Domain.Entities.Product>(), 0));

        var query = new GetAllProductsQuery(1, 25, null, null);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        capturedSort.Should().NotBeNull();
        capturedSort.Should().ContainSingle();
        capturedSort![0].Field.Should().Be("CreatedAt");
        capturedSort[0].Descending.Should().BeTrue();
    }

    [TestMethod]
    public async Task Handle_Should_UseDefaultSortByCreatedAtDescending_When_SortFieldsIsEmpty()
    {
        // Arrange
        IReadOnlyList<SortCriteria>? capturedSort = null;

        _productRepositoryMock
            .Setup(x => x.GetPagedAsync(
                1, 25, null,
                It.IsAny<IReadOnlyList<SortCriteria>?>()))
            .Callback<int, int, IReadOnlyList<FilterCriteria>?, IReadOnlyList<SortCriteria>?>(
                (_, _, _, s) => capturedSort = s)
            .ReturnsAsync((new List<global::Olimpia.Domain.Entities.Product>(), 0));

        var query = new GetAllProductsQuery(1, 25, null, new List<SortCriteria>());

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        capturedSort.Should().NotBeNull();
        capturedSort.Should().ContainSingle();
        capturedSort![0].Field.Should().Be("CreatedAt");
        capturedSort[0].Descending.Should().BeTrue();
    }

    [TestMethod]
    public async Task Handle_Should_UseProvidedSort_When_SortFieldsHasValues()
    {
        // Arrange
        var sort = new List<SortCriteria> { new("Price", false) };
        IReadOnlyList<SortCriteria>? capturedSort = null;

        _productRepositoryMock
            .Setup(x => x.GetPagedAsync(
                1, 25, null,
                It.IsAny<IReadOnlyList<SortCriteria>?>()))
            .Callback<int, int, IReadOnlyList<FilterCriteria>?, IReadOnlyList<SortCriteria>?>(
                (_, _, _, s) => capturedSort = s)
            .ReturnsAsync((new List<global::Olimpia.Domain.Entities.Product>(), 0));

        var query = new GetAllProductsQuery(1, 25, null, sort);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        capturedSort.Should().BeEquivalentTo(sort);
    }
    // Fin código generado por GitHub Copilot
}
// Fin código generado por GitHub Copilot
