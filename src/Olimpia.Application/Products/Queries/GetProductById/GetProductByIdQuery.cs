using Cortex.Mediator.Queries;
using Olimpia.Application.Products;

namespace Olimpia.Application.Products.Queries.GetProductById;

// Inicio código generado por GitHub Copilot

/// <summary>
/// Consulta que devuelve un producto por su identificador.
/// </summary>
/// <param name="Id">Identificador único del producto.</param>
public sealed record GetProductByIdQuery(int Id) : IQuery<ProductDto>;
// Fin código generado por GitHub Copilot
