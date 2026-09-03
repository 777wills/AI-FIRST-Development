// Inicio código generado por GitHub Copilot
using System.Data;
using Dapper;
using Olimpia.Domain.Repositories;

namespace Olimpia.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementación de <see cref="IStoredProcedureRepository"/> usando Dapper
/// sobre la conexión y transacción activa del <see cref="UnitOfWork"/>.
///
/// Todos los métodos propagan automáticamente la transacción en curso, por lo que las
/// llamadas a SPs participan en el mismo <c>UnitOfWork</c> que el resto de operaciones
/// del caso de uso.
///
/// Ejemplo de uso en un Handler:
/// <code>
/// // Parámetros simples (objeto anónimo)
/// var items = await _sp.QueryAsync&lt;ItemDto&gt;("usp_GetItemsByOrden", new { OrdenId = command.OrdenId });
///
/// // Parámetro de salida con DynamicParameters
/// var dp = new DynamicParameters();
/// dp.Add("@Total", dbType: DbType.Decimal, direction: ParameterDirection.Output);
/// await _sp.ExecuteAsync("usp_CalcularTotal", dp);
/// decimal total = dp.Get&lt;decimal&gt;("@Total");
/// </code>
/// </summary>
public class StoredProcedureRepository(IUnitOfWork unitOfWork) : IStoredProcedureRepository
{
    /// <summary>UnitOfWork que provee la conexión y la transacción activa.</summary>
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    // Método generado por GitHub Copilot
    /// <inheritdoc/>
    public async Task<int> ExecuteAsync(string procedureName, object? parameters = null)
    {
        // Inicio refactorización/optimización por GitHub Copilot
        await _unitOfWork.EnsureOpenAsync().ConfigureAwait(false);
        // Fin refactorización/optimización por GitHub Copilot
        return await _unitOfWork.DbConnection.ExecuteAsync(
            procedureName,
            parameters,
            transaction:  _unitOfWork.DbTransaction,
            commandType:  CommandType.StoredProcedure);
    }

    // Método generado por GitHub Copilot
    /// <inheritdoc/>
    public async Task<IEnumerable<T>> QueryAsync<T>(string procedureName, object? parameters = null)
    {
        // Inicio refactorización/optimización por GitHub Copilot
        await _unitOfWork.EnsureOpenAsync().ConfigureAwait(false);
        // Fin refactorización/optimización por GitHub Copilot
        return await _unitOfWork.DbConnection.QueryAsync<T>(
            procedureName,
            parameters,
            transaction:  _unitOfWork.DbTransaction,
            commandType:  CommandType.StoredProcedure);
    }

    // Método generado por GitHub Copilot
    /// <inheritdoc/>
    public async Task<T?> QuerySingleAsync<T>(string procedureName, object? parameters = null)
    {
        // Inicio refactorización/optimización por GitHub Copilot
        await _unitOfWork.EnsureOpenAsync().ConfigureAwait(false);
        // Fin refactorización/optimización por GitHub Copilot
        return await _unitOfWork.DbConnection.QueryFirstOrDefaultAsync<T>(
            procedureName,
            parameters,
            transaction:  _unitOfWork.DbTransaction,
            commandType:  CommandType.StoredProcedure);
    }
}
// Fin código generado por GitHub Copilot
