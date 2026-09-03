// Inicio código generado por GitHub Copilot
using FluentValidation;
using Olimpia.Domain.Common;

namespace Olimpia.Application.Products.Queries.GetAllProducts;

public sealed class GetAllProductsValidator : AbstractValidator<GetAllProductsQuery>
{
    // Inicio refactorización/optimización por GitHub Copilot
    // Campos permitidos como filtro y sus operadores válidos (case-insensitive).
    private static readonly Dictionary<string, IReadOnlyList<FilterOperator>> AllowedFilters = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Name"]      = [FilterOperator.Contains],
        ["Price"]     = [FilterOperator.Eq, FilterOperator.Gt, FilterOperator.Gte, FilterOperator.Lt, FilterOperator.Lte],
        ["Stock"]     = [FilterOperator.Eq, FilterOperator.Gt, FilterOperator.Gte, FilterOperator.Lt, FilterOperator.Lte],
        ["CreatedAt"] = [FilterOperator.Gt, FilterOperator.Lt]
    };

    // Campos permitidos como criterio de ordenamiento (case-insensitive).
    private static readonly HashSet<string> AllowedSortFields = new(["Name", "Price", "Stock", "CreatedAt"], StringComparer.OrdinalIgnoreCase);
    // Fin refactorización/optimización por GitHub Copilot

    // Método generado por GitHub Copilot
    public GetAllProductsValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage("El número de página debe ser mayor o igual a 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("El tamaño de página debe estar entre 1 y 100.");

        RuleFor(x => x.Filters)
            .Must(BeValidFilters)
            .When(x => x.Filters is not null && x.Filters.Count > 0)
            .WithMessage($"Los filtros contienen campos u operadores no permitidos. Campos válidos: {string.Join(", ", AllowedFilters.Keys)}.");

        // Inicio código generado por GitHub Copilot
        RuleFor(x => x.Filters)
            .Must(filters => filters!.All(IsValidValueForField))
            .When(x => x.Filters is not null && x.Filters.Count > 0)
            .WithMessage("Uno o más filtros contienen valores con formato inválido.");
        // Fin código generado por GitHub Copilot

        RuleFor(x => x.SortFields)
            .Must(BeValidSortFields)
            .When(x => x.SortFields is not null && x.SortFields.Count > 0)
            .WithMessage($"Los campos de ordenamiento no son válidos. Campos permitidos: {string.Join(", ", AllowedSortFields)}.");
    }

    private static bool BeValidFilters(IReadOnlyList<FilterCriteria>? filters)
    {
        if (filters is null)
            return true;

        foreach (var filter in filters)
        {
            if (!AllowedFilters.TryGetValue(filter.Field, out var allowedOperators))
                return false;

            if (!allowedOperators.Contains(filter.Operator))
                return false;
        }

        return true;
    }

    private static bool BeValidSortFields(IReadOnlyList<SortCriteria>? sortFields)
    {
        if (sortFields is null)
            return true;

        return sortFields.All(s => AllowedSortFields.Contains(s.Field));
    }

    // Inicio código generado por GitHub Copilot
    private static bool IsValidValueForField(FilterCriteria filter) =>
        filter.Field.ToUpperInvariant() switch
        {
            "PRICE" or "STOCK" => decimal.TryParse(filter.Value, out _),
            "CREATEDAT" => DateTime.TryParse(filter.Value, out _),
            _ => true
        };
    // Fin código generado por GitHub Copilot
}
// Fin código generado por GitHub Copilot
