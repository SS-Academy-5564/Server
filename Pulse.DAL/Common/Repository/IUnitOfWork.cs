using System.Data;

namespace Pulse.DAL.Common.Repository;

public interface IUnitOfWork : IAsyncDisposable
{
    IDbTransaction Transaction { get; }
    Task CommitAsync(CancellationToken ct = default);
}
