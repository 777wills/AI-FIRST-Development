namespace Olimpia.Application.Products;

// Inicio código generado por GitHub Copilot

/// <summary>
/// Representación pública de un producto del catálogo.
/// </summary>
/// <param name="Id">Identificador único del producto.</param>
/// <param name="Name">Nombre del producto.</param>
/// <param name="Description">Descripción detallada del producto.</param>
/// <param name="Price">Precio en la moneda por defecto.</param>
/// <param name="Stock">Cantidad disponible en stock.</param>
/// <param name="CreatedAt">Fecha de creación (UTC).</param>
/// <param name="UpdatedAt">Fecha de última actualización (UTC). <c>null</c> si nunca se actualizó.</param>
public sealed record ProductDto(
    int Id,
    string Name,
    string Description,
    decimal Price,
    int Stock,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
// Fin código generado por GitHub Copilot
