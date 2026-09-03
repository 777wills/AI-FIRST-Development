// Inicio código generado por GitHub Copilot
using FluentValidation;

namespace Olimpia.Application.Products.Queries.GetProductById;

public sealed class GetProductByIdValidator : AbstractValidator<GetProductByIdQuery>
{
    // Método generado por GitHub Copilot
    public GetProductByIdValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("El identificador del producto debe ser un número positivo.");
    }
}
// Fin código generado por GitHub Copilot
