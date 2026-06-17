using Pulse.DAL.Connection;

namespace Pulse.DAL.Common.Repository;

public class UnitOfWorkFactory : IUnitOfWorkFactory
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UnitOfWorkFactory(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public Task<IUnitOfWork> CreateAsync(CancellationToken ct = default)
    {
        var connection = _connectionFactory.CreateConnection();
        connection.Open();
        var transaction = connection.BeginTransaction();
        return Task.FromResult<IUnitOfWork>(new UnitOfWork(connection, transaction));
    }

    public async Task ExecuteAsync(Func<IUnitOfWork, Task> work, CancellationToken ct = default)
    {
        await using var uow = await CreateAsync(ct);
        await work(uow);
        await uow.CommitAsync(ct);
    }
}
