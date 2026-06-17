using System.Data;

namespace Pulse.DAL.Common.Repository;

public class UnitOfWork : IUnitOfWork
{
    private readonly IDbConnection _connection;
    private readonly IDbTransaction _transaction;
    private bool _commited;

    public IDbTransaction Transaction => _transaction;

    public UnitOfWork(IDbConnection connection, IDbTransaction transaction)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public Task CommitAsync(CancellationToken ct = default)
    {
        _transaction.Commit();
        _commited = true;
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        if (!_commited)
        {
            _transaction.Rollback();
        }

        _transaction.Dispose();
        _connection.Dispose();
        return ValueTask.CompletedTask;
    }
}
