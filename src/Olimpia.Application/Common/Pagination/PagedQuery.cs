using Olimpia.Domain.Common;

namespace Olimpia.Application.Common.Pagination;

// Inicio código generado por GitHub Copilot
public abstract record PagedQuery(
    int PageNumber = 1,
    int PageSize = 25,
    IReadOnlyList<FilterCriteria>? Filters = null,
    IReadOnlyList<SortCriteria>? SortFields = null);
// Fin código generado por GitHub Copilot
