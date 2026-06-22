using System.Data;

namespace Pulse.DAL.Common.Repository;

public class UnitOfWork : IUnitOfWork
{
    private bool _committed;

    /// <inheritdoc/>
    public IDbConnection Connection { get; }

    /// <inheritdoc/>
    public IDbTransaction Transaction { get; }

    public UnitOfWork(IDbConnection connection, IDbTransaction transaction)
    {
        Connection = connection;
        Transaction = transaction;
    }

    /// <inheritdoc/>
    public Task CommitAsync(CancellationToken ct = default)
    {
        Transaction.Commit();
        _committed = true;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Rolls back the transaction if it was not committed, then disposes the transaction and connection.
    /// </summary>
    public ValueTask DisposeAsync()
    {
        if (!_committed)
        {
            Transaction.Rollback();
        }

        Transaction.Dispose();
        Connection.Dispose();
        return ValueTask.CompletedTask;
    }
}
