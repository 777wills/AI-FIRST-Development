// Inicio código generado por GitHub Copilot
namespace Olimpia.Domain.Repositories;

public interface IProductRepository : IGenericRepository<global::Olimpia.Domain.Entities.Product>
{
    // Método generado por GitHub Copilot
    Task<global::Olimpia.Domain.Entities.Product?> GetByNameAsync(string name);
}
// Fin código generado por GitHub Copilot