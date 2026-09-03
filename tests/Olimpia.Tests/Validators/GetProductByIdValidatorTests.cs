// Inicio código generado por GitHub Copilot
using FluentValidation.TestHelper;
using Olimpia.Application.Products.Queries.GetProductById;

namespace Olimpia.Tests.Validators;

[TestClass]
public sealed class GetProductByIdValidatorTests
{
    private readonly GetProductByIdValidator _validator = new();

    // Inicio refactorización/optimización por GitHub Copilot
    [TestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    [DataRow(-99)]
    public void Validate_Should_Fail_When_IdIsInvalid(int id)
    {
        var result = _validator.TestValidate(new GetProductByIdQuery(id));
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }
    // Fin refactorización/optimización por GitHub Copilot

    [TestMethod]
    public void Validate_Should_Pass_When_IdIsPositive()
    {
        var query = new GetProductByIdQuery(1);
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
// Fin código generado por GitHub Copilot
