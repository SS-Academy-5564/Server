using System.Data;
using System.Data.Common;
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
    public async Task<IUnitOfWork> CreateAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken ct = default)
    {
        DbConnection connection = _connectionFactory.CreateConnection();
        try
        {
            await connection.OpenAsync(ct);
            DbTransaction transaction = await connection.BeginTransactionAsync(isolationLevel, ct);
            return new UnitOfWork(connection, transaction);
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }
    }
}
