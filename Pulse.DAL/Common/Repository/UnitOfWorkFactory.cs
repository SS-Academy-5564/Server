using System.Data;
using Pulse.DAL.Connection;

namespace Pulse.DAL.Common.Repository;

public class UnitOfWorkFactory : IUnitOfWorkFactory
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UnitOfWorkFactory(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <inheritdoc/>
    public Task<IUnitOfWork> CreateAsync(CancellationToken ct = default)
    {
        IDbConnection connection = _connectionFactory.CreateConnection();
        connection.Open();
        IDbTransaction transaction = connection.BeginTransaction();
        return Task.FromResult<IUnitOfWork>(new UnitOfWork(connection, transaction));
    }
}
