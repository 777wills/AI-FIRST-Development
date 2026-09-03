// Inicio código generado por GitHub Copilot
using Cortex.Mediator.Queries;
using Olimpia.Domain.Repositories;

namespace Olimpia.Application.Products.Queries.GetProductById;

public sealed class GetProductByIdHandler : IQueryHandler<GetProductByIdQuery, ProductDto>
{
    private readonly IProductRepository _productRepository;

    // Método generado por GitHub Copilot
    public GetProductByIdHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    // Método generado por GitHub Copilot
    public async Task<ProductDto> Handle(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(query.Id);

        if (product is null)
            throw new KeyNotFoundException($"No se encontró el producto con identificador {query.Id}.");

        return new ProductDto(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.Stock,
            product.CreatedAt,
            product.UpdatedAt);
    }
}
// Fin código generado por GitHub Copilot
