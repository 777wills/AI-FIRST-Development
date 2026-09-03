// Inicio código generado por GitHub Copilot
using FluentAssertions;
using Olimpia.Application.Common.Pagination;
using Olimpia.Application.Products;

namespace Olimpia.Tests.Handlers.Products;

[TestClass]
public sealed class PagedResultTests
{
    [TestMethod]
    public void PagedResult_HasNextPage_Should_BeTrue_When_NotOnLastPage()
    {
        // Arrange
        var data = Enumerable.Repeat(new ProductDto(1, "A", "D", 1m, 1, DateTime.Now, null), 5);
        var result = PagedResult<ProductDto>.Create(data, pageNumber: 1, pageSize: 5, totalCount: 10);

        // Act & Assert
        result.HasNextPage.Should().BeTrue();
    }

    [TestMethod]
    public void PagedResult_HasNextPage_Should_BeFalse_When_OnLastPage()
    {
        // Arrange
        var data = Enumerable.Repeat(new ProductDto(1, "A", "D", 1m, 1, DateTime.Now, null), 5);
        var result = PagedResult<ProductDto>.Create(data, pageNumber: 2, pageSize: 5, totalCount: 10);

        // Act & Assert
        result.HasNextPage.Should().BeFalse();
    }

    [TestMethod]
    public void PagedResult_HasPreviousPage_Should_BeFalse_When_OnFirstPage()
    {
        // Arrange
        var data = Enumerable.Repeat(new ProductDto(1, "A", "D", 1m, 1, DateTime.Now, null), 5);
        var result = PagedResult<ProductDto>.Create(data, pageNumber: 1, pageSize: 5, totalCount: 10);

        // Act & Assert
        result.HasPreviousPage.Should().BeFalse();
    }

    [TestMethod]
    public void PagedResult_HasPreviousPage_Should_BeTrue_When_NotOnFirstPage()
    {
        // Arrange
        var data = Enumerable.Repeat(new ProductDto(1, "A", "D", 1m, 1, DateTime.Now, null), 5);
        var result = PagedResult<ProductDto>.Create(data, pageNumber: 2, pageSize: 5, totalCount: 10);

        // Act & Assert
        result.HasPreviousPage.Should().BeTrue();
    }

    [TestMethod]
    public void PagedResult_TotalPages_Should_RoundUp_When_DataDoesNotFillLastPage()
    {
        // Arrange — 11 items con pageSize=5 → TotalPages=3
        var data = Enumerable.Repeat(new ProductDto(1, "A", "D", 1m, 1, DateTime.Now, null), 5);
        var result = PagedResult<ProductDto>.Create(data, pageNumber: 1, pageSize: 5, totalCount: 11);

        // Act & Assert
        result.TotalPages.Should().Be(3);
    }

    [TestMethod]
    public void PagedResult_Create_Should_SetAllProperties_Correctly()
    {
        // Arrange
        var data = new List<ProductDto>
        {
            new(1, "Laptop", "Gaming laptop", 1500m, 10, DateTime.Now, null),
            new(2, "Mouse", "Wireless mouse", 50m, 20, DateTime.Now, null)
        };

        // Act
        var result = PagedResult<ProductDto>.Create(data, pageNumber: 1, pageSize: 25, totalCount: 0);

        // Assert — 0 items → TotalPages=0, HasNextPage=false
        result.TotalPages.Should().Be(0);
        result.HasNextPage.Should().BeFalse();
        result.HasPreviousPage.Should().BeFalse();
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(25);
        result.TotalCount.Should().Be(0);
    }
}
// Fin código generado por GitHub Copilot
