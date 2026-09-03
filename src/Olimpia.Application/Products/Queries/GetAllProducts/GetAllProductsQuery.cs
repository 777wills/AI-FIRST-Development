using Cortex.Mediator.Queries;
using Olimpia.Application.Common.Pagination;
using Olimpia.Application.Products;
using Olimpia.Domain.Common;

namespace Olimpia.Application.Products.Queries.GetAllProducts;

// Inicio código generado por GitHub Copilot

/// <summary>
/// Consulta que devuelve una página de productos aplicando filtros dinámicos y ordenamiento.
/// </summary>
/// <param name="PageNumber">Número de página (1-based). Por defecto 1.</param>
/// <param name="PageSize">Cantidad de elementos por página. Por defecto 25, máximo 100.</param>
/// <param name="Filters">Lista de filtros dinámicos parseada desde el query string (<c>campo[operador]=valor</c>).</param>
/// <param name="SortFields">Lista de campos de ordenamiento con dirección. Si es <c>null</c>, aplica el orden por defecto.</param>
public sealed record GetAllProductsQuery(
    int PageNumber = 1,
    int PageSize = 25,
    IReadOnlyList<FilterCriteria>? Filters = null,
    IReadOnlyList<SortCriteria>? SortFields = null)
    : PagedQuery(PageNumber, PageSize, Filters, SortFields), IQuery<PagedResult<ProductDto>>;
// Fin código generado por GitHub Copilot
