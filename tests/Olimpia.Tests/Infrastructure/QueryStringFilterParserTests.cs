// Inicio código generado por GitHub Copilot
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Olimpia.Api.Extensions;
using Olimpia.Domain.Common;

namespace Olimpia.Tests.Infrastructure;

[TestClass]
public sealed class QueryStringFilterParserTests
{
    [TestMethod]
    public void Parse_Should_ReturnDefaultPagination_When_NoQueryParams()
    {
        // Arrange
        var queryCollection = new QueryCollection(new Dictionary<string, StringValues>());

        // Act
        var (filters, sortFields, pageNumber, pageSize) = QueryStringFilterParser.Parse(queryCollection);

        // Assert
        pageNumber.Should().Be(1);
        pageSize.Should().Be(25);
        filters.Should().BeEmpty();
        sortFields.Should().BeEmpty();
    }

    [TestMethod]
    public void Parse_Should_ParsePageNumberAndPageSize()
    {
        // Arrange
        var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
        {
            { "pageNumber", "2" },
            { "pageSize", "10" }
        });

        // Act
        var (_, _, pageNumber, pageSize) = QueryStringFilterParser.Parse(queryCollection);

        // Assert
        pageNumber.Should().Be(2);
        pageSize.Should().Be(10);
    }

    [TestMethod]
    public void Parse_Should_ParseFilterNameContains()
    {
        // Arrange
        var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
        {
            { "name[contains]", "Laptop" }
        });

        // Act
        var (filters, _, _, _) = QueryStringFilterParser.Parse(queryCollection);

        // Assert
        filters.Should().HaveCount(1);
        filters[0].Field.Should().Be("name");
        filters[0].Operator.Should().Be(FilterOperator.Contains);
        filters[0].Value.Should().Be("Laptop");
    }

    [TestMethod]
    public void Parse_Should_ParseFilterPriceGte()
    {
        // Arrange
        var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
        {
            { "price[gte]", "100" }
        });

        // Act
        var (filters, _, _, _) = QueryStringFilterParser.Parse(queryCollection);

        // Assert
        filters.Should().HaveCount(1);
        filters[0].Field.Should().Be("price");
        filters[0].Operator.Should().Be(FilterOperator.Gte);
        filters[0].Value.Should().Be("100");
    }

    [TestMethod]
    public void Parse_Should_ParseMultipleFilters()
    {
        // Arrange
        var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
        {
            { "price[gte]", "100" },
            { "stock[gt]", "0" }
        });

        // Act
        var (filters, _, _, _) = QueryStringFilterParser.Parse(queryCollection);

        // Assert
        filters.Should().HaveCount(2);
        filters.Should().ContainSingle(f => f.Field == "price" && f.Operator == FilterOperator.Gte);
        filters.Should().ContainSingle(f => f.Field == "stock" && f.Operator == FilterOperator.Gt);
    }

    [TestMethod]
    public void Parse_Should_ParseSortAscending()
    {
        // Arrange
        var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
        {
            { "sort", "name" }
        });

        // Act
        var (_, sortFields, _, _) = QueryStringFilterParser.Parse(queryCollection);

        // Assert
        sortFields.Should().HaveCount(1);
        sortFields[0].Field.Should().Be("name");
        sortFields[0].Descending.Should().BeFalse();
    }

    [TestMethod]
    public void Parse_Should_ParseSortDescending()
    {
        // Arrange
        var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
        {
            { "sort", "-price" }
        });

        // Act
        var (_, sortFields, _, _) = QueryStringFilterParser.Parse(queryCollection);

        // Assert
        sortFields.Should().HaveCount(1);
        sortFields[0].Field.Should().Be("price");
        sortFields[0].Descending.Should().BeTrue();
    }

    [TestMethod]
    public void Parse_Should_ParseMultipleSort()
    {
        // Arrange
        var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
        {
            { "sort", "name,-price" }
        });

        // Act
        var (_, sortFields, _, _) = QueryStringFilterParser.Parse(queryCollection);

        // Assert
        sortFields.Should().HaveCount(2);
        sortFields.Should().ContainSingle(s => s.Field == "name" && !s.Descending);
        sortFields.Should().ContainSingle(s => s.Field == "price" && s.Descending);
    }

    [TestMethod]
    public void Parse_Should_IgnoreUnknownOperator()
    {
        // Arrange — operador inválido es ignorado silenciosamente
        var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
        {
            { "name[invalidop]", "test" }
        });

        // Act
        var (filters, _, _, _) = QueryStringFilterParser.Parse(queryCollection);

        // Assert
        filters.Should().BeEmpty();
    }

    [TestMethod]
    public void Parse_Should_UseDefaultPageNumber_When_PageNumberIsInvalid()
    {
        // Arrange
        var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
        {
            { "pageNumber", "abc" }
        });

        // Act
        var (_, _, pageNumber, _) = QueryStringFilterParser.Parse(queryCollection);

        // Assert
        pageNumber.Should().Be(1);
    }

    // Inicio código generado por GitHub Copilot
    [TestMethod]
    public void ParseFilters_Should_ExtractFilters_When_ValidBracketSyntax()
    {
        // Arrange
        var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
        {
            { "name[contains]", "Laptop" },
            { "price[gte]", "100" }
        });

        // Act
        var filters = QueryStringFilterParser.ParseFilters(queryCollection);

        // Assert
        filters.Should().HaveCount(2);
        filters.Should().ContainSingle(f => f.Field == "name" && f.Operator == FilterOperator.Contains && f.Value == "Laptop");
        filters.Should().ContainSingle(f => f.Field == "price" && f.Operator == FilterOperator.Gte && f.Value == "100");
    }

    [TestMethod]
    public void ParseFilters_Should_ReturnEmpty_When_NoBracketParams()
    {
        // Arrange
        var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
        {
            { "pageNumber", "1" },
            { "pageSize", "25" },
            { "sort", "name" }
        });

        // Act
        var filters = QueryStringFilterParser.ParseFilters(queryCollection);

        // Assert
        filters.Should().BeEmpty();
    }

    [TestMethod]
    public void ParseFilters_Should_IgnoreInvalidOperator()
    {
        // Arrange
        var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
        {
            { "name[invalidop]", "test" }
        });

        // Act
        var filters = QueryStringFilterParser.ParseFilters(queryCollection);

        // Assert
        filters.Should().BeEmpty();
    }

    [TestMethod]
    public void ParseSortFields_Should_ParseDescending_When_PrefixDash()
    {
        // Act
        var sortFields = QueryStringFilterParser.ParseSortFields("-price");

        // Assert
        sortFields.Should().HaveCount(1);
        sortFields[0].Field.Should().Be("price");
        sortFields[0].Descending.Should().BeTrue();
    }

    [TestMethod]
    public void ParseSortFields_Should_ParseAscending_When_NoPrefixDash()
    {
        // Act
        var sortFields = QueryStringFilterParser.ParseSortFields("name");

        // Assert
        sortFields.Should().HaveCount(1);
        sortFields[0].Field.Should().Be("name");
        sortFields[0].Descending.Should().BeFalse();
    }

    [TestMethod]
    public void ParseSortFields_Should_ReturnEmpty_When_NullOrEmpty()
    {
        // Act & Assert
        QueryStringFilterParser.ParseSortFields(null).Should().BeEmpty();
        QueryStringFilterParser.ParseSortFields("").Should().BeEmpty();
        QueryStringFilterParser.ParseSortFields("  ").Should().BeEmpty();
    }

    [TestMethod]
    public void ParseSortFields_Should_ParseMultipleFields()
    {
        // Act
        var sortFields = QueryStringFilterParser.ParseSortFields("name,-price,createdAt");

        // Assert
        sortFields.Should().HaveCount(3);
        sortFields[0].Should().BeEquivalentTo(new SortCriteria("name", false));
        sortFields[1].Should().BeEquivalentTo(new SortCriteria("price", true));
        sortFields[2].Should().BeEquivalentTo(new SortCriteria("createdAt", false));
    }
    // Fin código generado por GitHub Copilot
}
// Fin código generado por GitHub Copilot
