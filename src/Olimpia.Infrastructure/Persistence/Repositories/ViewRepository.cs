// Inicio código generado por GitHub Copilot
using Olimpia.Domain.Repositories;
using SqlKata;
using SqlKata.Compilers;
using SqlKata.Execution;

namespace Olimpia.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementación de <see cref="IViewRepository"/> usando SqlKata + Dapper
/// sobre la conexión y transacción activa del <see cref="UnitOfWork"/>.
///
/// Todas las consultas propagan automáticamente la transacción en curso, por lo que las
/// llamadas a vistas participan en el mismo <c>UnitOfWork</c> que el resto de operaciones
/// del caso de uso.
///
/// Ejemplo de uso en un Handler:
/// <code>
/// // Consulta simple
/// var activos = await _view.QueryAsync&lt;ProductoDto&gt;("vw_ProductosActivos");
///
/// // Con filtros dinámicos (se convierten en WHERE automáticamente)
/// var productos = await _view.QueryAsync&lt;ProductoDto&gt;("vw_Productos", new { CategoriaId = 5, Stock = &gt; 0 });
///
/// // Consulta paginada
/// var pagina = await _view.QueryPagedAsync&lt;ProductoDto&gt;("vw_Productos", pageNumber: 2, pageSize: 20);
/// </code>
/// </summary>
public class ViewRepository(IUnitOfWork unitOfWork) : IViewRepository
{
    /// <summary>QueryFactory configurada para SQL Server, compartiendo conexión con UnitOfWork.</summary>
    private readonly QueryFactory _db = new(unitOfWork.DbConnection, new SqlServerCompiler());

    // Método generado por GitHub Copilot
    /// <inheritdoc/>
    public async Task<IEnumerable<T>> QueryAsync<T>(string viewName, object? filters = null)
    {
        // Inicio refactorización/optimización por GitHub Copilot
        await unitOfWork.EnsureOpenAsync().ConfigureAwait(false);
        // Fin refactorización/optimización por GitHub Copilot
        var query = _db.Query(viewName);

        if (filters is not null)
        {
            ApplyFilters(query, filters);
        }

        return await query.GetAsync<T>(transaction: unitOfWork.DbTransaction);
    }

    // Método generado por GitHub Copilot
    /// <inheritdoc/>
    public async Task<T?> QuerySingleAsync<T>(string viewName, object? filters = null)
    {
        // Inicio refactorización/optimización por GitHub Copilot
        await unitOfWork.EnsureOpenAsync().ConfigureAwait(false);
        // Fin refactorización/optimización por GitHub Copilot
        var query = _db.Query(viewName);

        if (filters is not null)
        {
            ApplyFilters(query, filters);
        }

        return await query.FirstOrDefaultAsync<T>(transaction: unitOfWork.DbTransaction);
    }

    // Método generado por GitHub Copilot
    /// <inheritdoc/>
    public async Task<IEnumerable<T>> QueryPagedAsync<T>(string viewName, int pageNumber, int pageSize, object? filters = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageNumber, nameof(pageNumber));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize, nameof(pageSize));

        // Inicio refactorización/optimización por GitHub Copilot
        await unitOfWork.EnsureOpenAsync().ConfigureAwait(false);
        // Fin refactorización/optimización por GitHub Copilot
        var query = _db.Query(viewName);

        if (filters is not null)
        {
            ApplyFilters(query, filters);
        }

        var offset = (pageNumber - 1) * pageSize;
        return await query
            .Offset(offset)
            .Limit(pageSize)
            .GetAsync<T>(transaction: unitOfWork.DbTransaction);
    }

    // Método generado por GitHub Copilot
    /// <summary>
    /// Aplica filtros dinámicos como cláusulas WHERE en la consulta.
    /// Convierte las propiedades del objeto en condiciones de igualdad.
    /// </summary>
    private static void ApplyFilters(Query query, object filters)
    {
        var properties = filters.GetType().GetProperties();
        foreach (var prop in properties)
        {
            var value = prop.GetValue(filters);
            if (value is not null)
            {
                query.Where(prop.Name, value);
            }
        }
    }
}
// Fin código generado por GitHub Copilot
