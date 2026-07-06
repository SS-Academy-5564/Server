using System.Data;
using System.Data.Common;

namespace Pulse.DAL.Common.Repository;

public class UnitOfWork : IUnitOfWork, IDbSession, IDisposable
{
    private bool _committed;
    private readonly DbConnection _connection;
    private readonly DbTransaction _transaction;
    private readonly IDbSessionAccessor _sessionAccessor;

    /// <inheritdoc/>
    IDbConnection IDbSession.Connection => _connection;

    /// <inheritdoc/>
    IDbTransaction IDbSession.Transaction => _transaction;

    /// <summary>
    /// Initializes a new instance with an already-open connection and an active transaction.
    /// Sets itself as the ambient session on <paramref name="sessionAccessor"/>.
    /// </summary>
    /// <param name="connection">An open database connection.</param>
    /// <param name="transaction">An active transaction on <paramref name="connection"/>.</param>
    /// <param name="sessionAccessor">The scoped accessor used to expose this session to repositories.</param>
    public UnitOfWork(DbConnection connection, DbTransaction transaction, IDbSessionAccessor sessionAccessor)
    {
        _connection = connection;
        _transaction = transaction;
        _sessionAccessor = sessionAccessor;
        _sessionAccessor.Session = this;
    }

    /// <inheritdoc/>
    public async Task CommitAsync(CancellationToken ct = default)
    {
        await _transaction.CommitAsync(ct);
        _committed = true;
    }

    /// <summary>
    /// Rolls back the transaction if it was not committed, then disposes the transaction and connection,
    /// and clears the ambient session.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (!_committed)
        {
            try
            {
                await _transaction.RollbackAsync();
            }
            catch (Exception ex) when (ex is DbException or InvalidOperationException or ObjectDisposedException) { }
        }

        await _transaction.DisposeAsync();
        await _connection.DisposeAsync();
        _sessionAccessor.Session = null;
    }

    /// <summary>
    /// Rolls back the transaction if it was not committed, then disposes the transaction and connection,
    /// and clears the ambient session.
    /// </summary>
    public void Dispose()
    {
        if (!_committed)
        {
            try
            {
                _transaction.Rollback();
            }
            catch (Exception ex) when (ex is DbException or InvalidOperationException or ObjectDisposedException) { }
        }

        _transaction.Dispose();
        _connection.Dispose();
        _sessionAccessor.Session = null;
        GC.SuppressFinalize(this);
    }
}
