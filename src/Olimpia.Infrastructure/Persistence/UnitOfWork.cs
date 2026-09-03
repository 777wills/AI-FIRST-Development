// Inicio refactorización/optimización por GitHub Copilot
using System.Data;
using Microsoft.Data.SqlClient;
using Olimpia.Domain.Repositories;

namespace Olimpia.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork, IAsyncDisposable, IDisposable
{
    private readonly SqlConnection _connection;
    private IDbTransaction? _transaction;
    private bool _disposed;

    // Método generado por GitHub Copilot
    public UnitOfWork(string connectionString)
    {
        // La conexión se crea pero NO se abre aquí — apertura lazy en EnsureOpenAsync.
        _connection = new SqlConnection(connectionString);
    }

    public IDbConnection DbConnection => _connection;
    public IDbTransaction? DbTransaction => _transaction;

    // Método generado por GitHub Copilot
    /// <inheritdoc/>
    public async Task EnsureOpenAsync(CancellationToken cancellationToken = default)
    {
        if (_connection.State == ConnectionState.Closed)
            await _connection.OpenAsync(cancellationToken).ConfigureAwait(false);
    }

    // Método generado por GitHub Copilot
    public async Task BeginTransactionAsync()
    {
        await EnsureOpenAsync().ConfigureAwait(false);
        _transaction = await _connection.BeginTransactionAsync().ConfigureAwait(false);
    }

    // Método generado por GitHub Copilot
    public async Task CommitAsync()
    {
        try
        {
            if (_transaction is SqlTransaction sqlTx)
                await sqlTx.CommitAsync().ConfigureAwait(false);
        }
        catch
        {
            if (_transaction is SqlTransaction sqlTxRollback)
                await sqlTxRollback.RollbackAsync().ConfigureAwait(false);
            throw;
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }

    // Método generado por GitHub Copilot
    public async Task RollbackAsync()
    {
        if (_transaction is SqlTransaction sqlTx)
        {
            await sqlTx.RollbackAsync().ConfigureAwait(false);
            _transaction.Dispose();
            _transaction = null;
        }
    }

    // Método generado por GitHub Copilot
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        _transaction?.Dispose();
        await _connection.DisposeAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    // Método generado por GitHub Copilot
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    // Método generado por GitHub Copilot
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        _disposed = true;
        if (!disposing) return;
        _transaction?.Dispose();
        _connection.Dispose();
    }
}
// Fin refactorización/optimización por GitHub Copilot
