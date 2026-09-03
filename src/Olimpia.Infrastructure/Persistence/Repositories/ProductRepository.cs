// Inicio código generado por GitHub Copilot
using Olimpia.Domain.Repositories;
using SqlKata.Execution;

namespace Olimpia.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositorio de <see cref="global::Olimpia.Domain.Entities.Product"/>.
/// Las operaciones CRUD estándar (GetByIdAsync, GetAllAsync, AddAsync, UpdateAsync, DeleteAsync)
/// son provistas automáticamente por <see cref="GenericRepository{T}"/>.
/// Solo se implementan aquí los métodos de consulta específicos del dominio.
/// </summary>
public sealed class ProductRepository : GenericRepository<global::Olimpia.Domain.Entities.Product>, IProductRepository
{
    protected override string TableName => "Products";

    // Método generado por GitHub Copilot
    public ProductRepository(IUnitOfWork unitOfWork)
        : base(unitOfWork) { }

    // Método generado por GitHub Copilot
    public async Task<global::Olimpia.Domain.Entities.Product?> GetByNameAsync(string name) =>
        await Db.Query(TableName)
            .Where("Name", name)
            .FirstOrDefaultAsync<global::Olimpia.Domain.Entities.Product>(transaction: UnitOfWork.DbTransaction);
}
// Fin código generado por GitHub Copilot
