using System.Data;
using System.Data.Common;

namespace Pulse.DAL.Common.Repository;

public class UnitOfWork : IUnitOfWork, IDbSession, IDisposable
{
    private bool _committed;
    private readonly DbConnection _connection;
    private readonly DbTransaction _transaction;

    /// <inheritdoc/>
    IDbConnection IDbSession.Connection => _connection;

    /// <inheritdoc/>
    IDbTransaction IDbSession.Transaction => _transaction;

    /// <summary>
    /// Initializes a new instance with an already-open connection and an active transaction.
    /// </summary>
    /// <param name="connection">An open database connection.</param>
    /// <param name="transaction">An active transaction on <paramref name="connection"/>.</param>
    public UnitOfWork(DbConnection connection, DbTransaction transaction)
    {
        _connection = connection;
        _transaction = transaction;
    }

    /// <inheritdoc/>
    public async Task CommitAsync(CancellationToken ct = default)
    {
        await _transaction.CommitAsync(ct);
        _committed = true;
    }

    /// <summary>
    /// Rolls back the transaction if it was not committed, then disposes the transaction and connection.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (!_committed)
        {
            try
            {
                await _transaction.RollbackAsync();
            }
            catch (DbException) { }
        }

        await _transaction.DisposeAsync();
        await _connection.DisposeAsync();
    }

    /// <summary>
    /// Rolls back the transaction if it was not committed, then disposes the transaction and connection.
    /// </summary>
    public void Dispose()
    {
        if (!_committed)
        {
            try
            {
                _transaction.Rollback();
            }
            catch (DbException) { }
        }

        _transaction.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }
}
