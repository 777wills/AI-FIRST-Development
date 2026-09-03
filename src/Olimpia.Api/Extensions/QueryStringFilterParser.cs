using Microsoft.AspNetCore.Http;
using Olimpia.Domain.Common;
using System.Text.RegularExpressions;

namespace Olimpia.Api.Extensions;

// Inicio refactorización/optimización por GitHub Copilot
public static class QueryStringFilterParser
{
    private static readonly Regex FilterPattern = new(@"^(\w+)\[(\w+)\]$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Extrae filtros dinámicos con sintaxis campo[operador]=valor del query string.
    /// </summary>
    public static List<FilterCriteria> ParseFilters(IQueryCollection query)
    {
        var filters = new List<FilterCriteria>();

        foreach (var key in query.Keys)
        {
            var match = FilterPattern.Match(key);
            if (!match.Success) continue;

            var field = match.Groups[1].Value;
            var operatorStr = match.Groups[2].Value;

            if (Enum.TryParse<FilterOperator>(operatorStr, ignoreCase: true, out var filterOperator))
            {
                var value = query[key].ToString();
                filters.Add(new FilterCriteria(field, filterOperator, value));
            }
            // Si el operador no es válido, se ignora (el Validator lo rechazará con 400)
        }

        return filters;
    }

    /// <summary>
    /// Parsea el parámetro sort (formato: campo1,-campo2) a lista de criterios de ordenamiento.
    /// </summary>
    public static List<SortCriteria> ParseSortFields(string? sort)
    {
        var sortFields = new List<SortCriteria>();

        if (string.IsNullOrWhiteSpace(sort))
            return sortFields;

        foreach (var field in sort.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            var trimmed = field.Trim();
            if (trimmed.StartsWith('-'))
                sortFields.Add(new SortCriteria(trimmed[1..], Descending: true));
            else
                sortFields.Add(new SortCriteria(trimmed, Descending: false));
        }

        return sortFields;
    }

    /// <summary>
    /// Método legacy que combina ParseFilters + ParseSortFields + paginación.
    /// Preferir los métodos individuales con [FromQuery] en el controller.
    /// </summary>
    public static (List<FilterCriteria> Filters, List<SortCriteria> SortFields, int PageNumber, int PageSize) Parse(IQueryCollection query)
    {
        var filters = ParseFilters(query);

        query.TryGetValue("sort", out var sortValue);
        var sortFields = ParseSortFields(sortValue);

        var pageNumber = query.TryGetValue("pageNumber", out var pnValue) && int.TryParse(pnValue, out var pn) && pn > 0 ? pn : 1;
        var pageSize = query.TryGetValue("pageSize", out var psValue) && int.TryParse(psValue, out var ps) && ps > 0 ? ps : 25;

        return (filters, sortFields, pageNumber, pageSize);
    }
}
// Fin refactorización/optimización por GitHub Copilot
