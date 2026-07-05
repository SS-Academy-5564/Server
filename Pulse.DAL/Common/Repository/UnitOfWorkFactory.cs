using System.Data;
using System.Data.Common;
using Pulse.DAL.Connection;

namespace Pulse.DAL.Common.Repository;

public class UnitOfWorkFactory : IUnitOfWorkFactory
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IDbSessionAccessor _sessionAccessor;

    public UnitOfWorkFactory(IDbConnectionFactory connectionFactory, IDbSessionAccessor sessionAccessor)
    {
        _connectionFactory = connectionFactory;
        _sessionAccessor = sessionAccessor;
    }

    /// <inheritdoc/>
    public async Task<IUnitOfWork> CreateAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken ct = default)
    {
        DbConnection connection = _connectionFactory.CreateConnection();
        try
        {
            await connection.OpenAsync(ct);
            DbTransaction transaction = await connection.BeginTransactionAsync(isolationLevel, ct);
            var uow = new UnitOfWork(connection, transaction);
            _sessionAccessor.Session = uow;
            return uow;
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }
    }
}
