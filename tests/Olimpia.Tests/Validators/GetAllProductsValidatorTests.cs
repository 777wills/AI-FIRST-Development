// Inicio código generado por GitHub Copilot
using FluentValidation.TestHelper;
using Olimpia.Application.Products.Queries.GetAllProducts;
using global::Olimpia.Domain.Common;

namespace Olimpia.Tests.Validators;

[TestClass]
public sealed class GetAllProductsValidatorTests
{
    private readonly GetAllProductsValidator _validator = new();

    // Inicio refactorización/optimización por GitHub Copilot
    [TestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    [DataRow(-100)]
    public void Validate_Should_Fail_When_PageNumberIsInvalid(int pageNumber)
    {
        var query = new GetAllProductsQuery(pageNumber, 25);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.PageNumber);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(101)]
    public void Validate_Should_Fail_When_PageSizeIsInvalid(int pageSize)
    {
        var query = new GetAllProductsQuery(1, pageSize);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }
    // Fin refactorización/optimización por GitHub Copilot

    [TestMethod]
    public void Validate_Should_Fail_When_FilterFieldNotInWhitelist()
    {
        var filters = new List<FilterCriteria> { new("InvalidField", FilterOperator.Eq, "test") };
        var query = new GetAllProductsQuery(1, 25, filters);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Filters);
    }

    [TestMethod]
    public void Validate_Should_Fail_When_FilterOperatorNotAllowedForField()
    {
        // Name solo permite Contains; Eq no es válido para Name
        var filters = new List<FilterCriteria> { new("Name", FilterOperator.Eq, "Laptop") };
        var query = new GetAllProductsQuery(1, 25, filters);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Filters);
    }

    [TestMethod]
    public void Validate_Should_Fail_When_SortFieldNotInWhitelist()
    {
        var sort = new List<SortCriteria> { new("InvalidField", false) };
        var query = new GetAllProductsQuery(1, 25, null, sort);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.SortFields);
    }

    [TestMethod]
    public void Validate_Should_Pass_When_RequestIsValid()
    {
        var query = new GetAllProductsQuery(1, 25);
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [TestMethod]
    public void Validate_Should_Pass_When_ValidFilterNameContainsIsProvided()
    {
        var filters = new List<FilterCriteria> { new("Name", FilterOperator.Contains, "Laptop") };
        var query = new GetAllProductsQuery(1, 25, filters);
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [TestMethod]
    public void Validate_Should_Pass_When_ValidFilterPriceGtIsProvided()
    {
        var filters = new List<FilterCriteria> { new("Price", FilterOperator.Gt, "100") };
        var query = new GetAllProductsQuery(1, 25, filters);
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [TestMethod]
    public void Validate_Should_Pass_When_ValidSortByNameIsProvided()
    {
        var sort = new List<SortCriteria> { new("Name", false) };
        var query = new GetAllProductsQuery(1, 25, null, sort);
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    // Inicio código generado por GitHub Copilot
    [TestMethod]
    public void Validate_Should_Fail_When_PriceFilterHasNonNumericValue()
    {
        var filters = new List<FilterCriteria> { new("Price", FilterOperator.Gt, "notanumber") };
        var query = new GetAllProductsQuery(1, 25, filters);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Filters);
    }

    [TestMethod]
    public void Validate_Should_Fail_When_StockFilterHasNonNumericValue()
    {
        var filters = new List<FilterCriteria> { new("Stock", FilterOperator.Eq, "abc") };
        var query = new GetAllProductsQuery(1, 25, filters);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Filters);
    }

    [TestMethod]
    public void Validate_Should_Fail_When_CreatedAtFilterHasNonDateValue()
    {
        var filters = new List<FilterCriteria> { new("CreatedAt", FilterOperator.Gt, "notadate") };
        var query = new GetAllProductsQuery(1, 25, filters);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Filters);
    }

    [TestMethod]
    public void Validate_Should_Pass_When_PriceFilterHasNumericValue()
    {
        var filters = new List<FilterCriteria> { new("Price", FilterOperator.Lte, "999.99") };
        var query = new GetAllProductsQuery(1, 25, filters);
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveValidationErrorFor(x => x.Filters);
    }

    [TestMethod]
    public void Validate_Should_Pass_When_CreatedAtFilterHasDateValue()
    {
        var filters = new List<FilterCriteria> { new("CreatedAt", FilterOperator.Lt, "2025-01-01") };
        var query = new GetAllProductsQuery(1, 25, filters);
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveValidationErrorFor(x => x.Filters);
    }

    // Inicio código generado por GitHub Copilot
    [TestMethod]
    public void Validate_Should_Pass_When_SortFieldsIsNull()
    {
        // La regla SortFields tiene cláusula When que la omite si es null
        var query = new GetAllProductsQuery(1, 25, null, null);
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveValidationErrorFor(x => x.SortFields);
    }

    [TestMethod]
    public void Validate_Should_Fail_When_SortFieldHasInvalidField()
    {
        // "Id" no está en la whitelist de campos de ordenamiento
        var sort = new List<SortCriteria> { new("Id", true) };
        var query = new GetAllProductsQuery(1, 25, null, sort);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.SortFields);
    }
    // Fin código generado por GitHub Copilot
}
// Fin código generado por GitHub Copilot
