// Inicio código generado por GitHub Copilot
using FluentAssertions;
using Olimpia.Application.Common.Pagination;
using Olimpia.Application.Common.Responses;
using Olimpia.Application.Products;

namespace Olimpia.Tests.Handlers.Products;

[TestClass]
public sealed class PagedEnvelopeTests
{
    private static PagedResult<ProductDto> BuildPagedResult(int pageNumber, int pageSize, int totalCount)
    {
        var data = new List<ProductDto>
        {
            new(1, "Laptop", "Gaming laptop", 1500m, 10, DateTime.Now, null),
            new(2, "Mouse", "Wireless mouse", 50m, 20, DateTime.Now, null)
        };
        return PagedResult<ProductDto>.Create(data, pageNumber, pageSize, totalCount);
    }

    [TestMethod]
    public void PagedEnvelope_FromPagedResult_Should_MapDataCorrectly()
    {
        // Arrange
        var pagedResult = BuildPagedResult(pageNumber: 2, pageSize: 10, totalCount: 30);

        // Act
        var envelope = PagedEnvelope<ProductDto>.FromPagedResult(pagedResult);

        // Assert
        envelope.Data.Should().HaveCount(2);
        envelope.Data.Should().ContainSingle(x => x.Name == "Laptop");
        envelope.Data.Should().ContainSingle(x => x.Name == "Mouse");
    }

    [TestMethod]
    public void PagedEnvelope_FromPagedResult_Should_MapPaginationMetaCorrectly()
    {
        // Arrange
        var pagedResult = BuildPagedResult(pageNumber: 2, pageSize: 10, totalCount: 30);

        // Act
        var envelope = PagedEnvelope<ProductDto>.FromPagedResult(pagedResult);

        // Assert
        envelope.Meta.Pagination.CurrentPage.Should().Be(2);
        envelope.Meta.Pagination.PageSize.Should().Be(10);
        envelope.Meta.Pagination.TotalCount.Should().Be(30);
        envelope.Meta.Pagination.TotalPages.Should().Be(3);
    }

    [TestMethod]
    public void PagedEnvelope_FromPagedResult_Should_SetHasNextPageAndHasPreviousPage()
    {
        // Arrange — página 2 de 3: HasNextPage=true, HasPreviousPage=true
        var pagedResult = BuildPagedResult(pageNumber: 2, pageSize: 10, totalCount: 30);

        // Act
        var envelope = PagedEnvelope<ProductDto>.FromPagedResult(pagedResult);

        // Assert
        envelope.Meta.Pagination.HasNextPage.Should().BeTrue();
        envelope.Meta.Pagination.HasPreviousPage.Should().BeTrue();
    }
}
// Fin código generado por GitHub Copilot
