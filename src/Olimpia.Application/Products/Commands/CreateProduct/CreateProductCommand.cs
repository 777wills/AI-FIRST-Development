// Inicio código generado por GitHub Copilot
using System.ComponentModel.DataAnnotations;
using Cortex.Mediator.Commands;

namespace Olimpia.Application.Products.Commands.CreateProduct;

/// <summary>
/// Comando para crear un producto en el catálogo.
/// </summary>
/// <param name="Name">Nombre del producto. Obligatorio, máximo 100 caracteres y único.</param>
/// <param name="Description">Descripción detallada del producto. Máximo 500 caracteres.</param>
/// <param name="Price">Precio en la moneda por defecto. Debe ser mayor a 0.</param>
/// <param name="Stock">Cantidad inicial en stock. Debe ser mayor o igual a 0.</param>
public sealed record CreateProductCommand(
    [Required]
    string Name,
    string Description,
    [Required]
    decimal Price,
    [Required]
    int Stock) : ICommand<int>;
// Fin código generado por GitHub Copilot
