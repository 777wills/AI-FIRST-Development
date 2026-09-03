// Inicio código generado por GitHub Copilot
using Olimpia.Domain.Common;

namespace Olimpia.Domain.Entities;

public sealed class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }

    // Constructor vacío requerido por Dapper.
    public Product() { }

    // Constructor parametrizado para creación explícita.
    public Product(string name, string description, decimal price, int stock)
    {
        Name        = name;
        Description = description;
        Price       = price;
        Stock       = stock;
    }
}
// Fin código generado por GitHub Copilot
