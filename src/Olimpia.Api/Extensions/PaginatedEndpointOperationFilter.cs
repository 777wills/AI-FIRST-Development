// Inicio código generado por GitHub Copilot
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using System.Reflection;

namespace Olimpia.Api.Extensions;

/// <summary>
/// Atributo marker para endpoints paginados. Define los filtros dinámicos permitidos.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class PaginatedEndpointAttribute : Attribute
{
    /// <summary>
    /// Filtros permitidos en formato "campo[operador]" separados por coma.
    /// Ejemplo: "name[contains],price[gte],price[lte],stock[gt]"
    /// </summary>
    public string AllowedFilters { get; set; } = string.Empty;

    /// <summary>
    /// Campos permitidos para ordenamiento, separados por coma.
    /// Ejemplo: "name,price,stock,createdAt"
    /// </summary>
    public string AllowedSortFields { get; set; } = string.Empty;

    /// <summary>
    /// Criterio de ordenamiento por defecto.
    /// Ejemplo: "-createdAt"
    /// </summary>
    public string DefaultSort { get; set; } = string.Empty;
}

/// <summary>
/// IOperationFilter que enriquece la documentación Swagger de endpoints paginados
/// con la sintaxis de filtros dinámicos, campos permitidos y ejemplos de uso.
/// </summary>
public sealed class PaginatedEndpointOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var attribute = context.MethodInfo.GetCustomAttribute<PaginatedEndpointAttribute>();
        if (attribute is null)
            return;

        // Construir descripción enriquecida con la sintaxis de filtros
        var filterSection = BuildFilterDocumentation(attribute);

        operation.Description = string.IsNullOrWhiteSpace(operation.Description)
            ? filterSection
            : $"{operation.Description}\n\n{filterSection}";
    }

    private static string BuildFilterDocumentation(PaginatedEndpointAttribute attribute)
    {
        var sections = new List<string>
        {
            "### Filtrado dinámico",
            "Los filtros usan la sintaxis `campo[operador]=valor` como query parameters adicionales.",
            "Múltiples filtros se combinan con **AND** lógico."
        };

        if (!string.IsNullOrWhiteSpace(attribute.AllowedFilters))
        {
            var filters = attribute.AllowedFilters.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var filterLines = string.Join("\n", filters.Select(f => $"{f}=valor"));
            sections.Add(string.Empty);
            sections.Add("**Filtros permitidos:**");
            sections.Add($"```\n{filterLines}\n```");
        }

        if (!string.IsNullOrWhiteSpace(attribute.AllowedSortFields))
        {
            sections.Add(string.Empty);
            sections.Add("**Campos de ordenamiento permitidos:**");
            sections.Add($"`{attribute.AllowedSortFields}`");
            sections.Add("Usar prefijo `-` para descendente (ej: `-price`).");
        }

        if (!string.IsNullOrWhiteSpace(attribute.DefaultSort))
        {
            sections.Add(string.Empty);
            sections.Add($"**Orden por defecto:** `{attribute.DefaultSort}`");
        }

        sections.Add(string.Empty);
        sections.Add("**Ejemplo:** `?name[contains]=Laptop&price[gte]=100&sort=name,-price&pageNumber=1&pageSize=10`");

        return string.Join("\n", sections);
    }
}
// Fin código generado por GitHub Copilot
