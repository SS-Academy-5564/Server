using System.Data;

namespace Pulse.DAL.Common.Repository;

public class UnitOfWork : IUnitOfWork
{
    private readonly IDbConnection _connection;
    private readonly IDbTransaction _transaction;
    private bool _committed;

    /// <inheritdoc/>
    public IDbTransaction Transaction => _transaction;

    public UnitOfWork(IDbConnection connection, IDbTransaction transaction)
    {
        _connection = connection;
        _transaction = transaction;
    }

    /// <inheritdoc/>
    public Task CommitAsync(CancellationToken ct = default)
    {
        _transaction.Commit();
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
            _transaction.Rollback();
        }

        _transaction.Dispose();
        _connection.Dispose();
        return ValueTask.CompletedTask;
    }
}
