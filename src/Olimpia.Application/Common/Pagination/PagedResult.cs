namespace Olimpia.Application.Common.Pagination;

// Inicio código generado por GitHub Copilot
public sealed record PagedResult<T>
{
    public IEnumerable<T> Data { get; init; } = [];
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => PageNumber < TotalPages;
    public bool HasPreviousPage => PageNumber > 1;

    public static PagedResult<T> Create(IEnumerable<T> data, int pageNumber, int pageSize, int totalCount)
        => new()
        {
            Data = data,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
}
// Fin código generado por GitHub Copilot
