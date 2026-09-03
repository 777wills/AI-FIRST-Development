// Inicio código generado por GitHub Copilot
using System.Data;

namespace Olimpia.Domain.Repositories;

/// <summary>
/// Contrato para la unidad de trabajo transaccional.
/// Expone la conexión y transacción activas para que los repositorios
/// puedan participar de la misma transacción sin conocer la implementación concreta.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>Conexión de base de datos activa.</summary>
    IDbConnection DbConnection { get; }

    /// <summary>Transacción activa, o <c>null</c> si no hay transacción en curso.</summary>
    IDbTransaction? DbTransaction { get; }

    // Inicio refactorización/optimización por GitHub Copilot
    /// <summary>
    /// Abre la conexión de forma asíncrona si aún no está abierta.
    /// Es idempotente: si la conexión ya está abierta no realiza ninguna operación.
    /// </summary>
    Task EnsureOpenAsync(CancellationToken cancellationToken = default);
    // Fin refactorización/optimización por GitHub Copilot

    // Método generado por GitHub Copilot
    Task BeginTransactionAsync();

    // Método generado por GitHub Copilot
    Task CommitAsync();

    // Método generado por GitHub Copilot
    Task RollbackAsync();
}
// Fin código generado por GitHub Copilot