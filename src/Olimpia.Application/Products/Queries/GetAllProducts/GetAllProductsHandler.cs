// Inicio código generado por GitHub Copilot
using Cortex.Mediator.Queries;
using Olimpia.Application.Common.Pagination;
using Olimpia.Domain.Common;
using Olimpia.Domain.Repositories;

namespace Olimpia.Application.Products.Queries.GetAllProducts;

public sealed class GetAllProductsHandler : IQueryHandler<GetAllProductsQuery, PagedResult<ProductDto>>
{
    private static readonly IReadOnlyList<SortCriteria> DefaultSort = [new SortCriteria("CreatedAt", Descending: true)];

    private readonly IProductRepository _productRepository;

    // Método generado por GitHub Copilot
    public GetAllProductsHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    // Inicio refactorización/optimización por GitHub Copilot
    public async Task<PagedResult<ProductDto>> Handle(GetAllProductsQuery query, CancellationToken cancellationToken)
    {
        // Si no se especifica ordenamiento, aplicar sort por defecto: CreatedAt descendente (RF-04)
        var sortFields = query.SortFields is { Count: > 0 } ? query.SortFields : DefaultSort;

        var (data, totalCount) = await _productRepository.GetPagedAsync(
            query.PageNumber,
            query.PageSize,
            query.Filters,
            sortFields);

        var dtos = data.Select(p => new ProductDto(
            p.Id,
            p.Name,
            p.Description,
            p.Price,
            p.Stock,
            p.CreatedAt,
            p.UpdatedAt));

        return PagedResult<ProductDto>.Create(dtos, query.PageNumber, query.PageSize, totalCount);
    }
    // Fin refactorización/optimización por GitHub Copilot
}
// Fin código generado por GitHub Copilot
