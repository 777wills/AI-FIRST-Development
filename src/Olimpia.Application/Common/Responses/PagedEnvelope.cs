using Olimpia.Application.Common.Pagination;

namespace Olimpia.Application.Common.Responses;

// Inicio código generado por GitHub Copilot
public sealed record PaginationMeta(
    int CurrentPage,
    int PageSize,
    int TotalCount,
    int TotalPages,
    bool HasNextPage,
    bool HasPreviousPage);

public sealed record PagedMeta(PaginationMeta Pagination);

public sealed record PagedEnvelope<T>(IEnumerable<T> Data, PagedMeta Meta)
{
    public static PagedEnvelope<T> FromPagedResult(PagedResult<T> result)
        => new(
            result.Data,
            new PagedMeta(new PaginationMeta(
                result.PageNumber,
                result.PageSize,
                result.TotalCount,
                result.TotalPages,
                result.HasNextPage,
                result.HasPreviousPage)));
}
// Fin código generado por GitHub Copilot
