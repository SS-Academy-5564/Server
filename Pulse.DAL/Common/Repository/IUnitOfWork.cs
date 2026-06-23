using System.Data;

namespace Pulse.DAL.Common.Repository;

public interface IUnitOfWork : IAsyncDisposable
{
    /// <summary>
    /// The active database connection for this unit of work.
    /// </summary>
    IDbConnection Connection { get; }

    /// <summary>
    /// The active database transaction for this unit of work.
    /// </summary>
    IDbTransaction Transaction { get; }

    /// <summary>
    /// Commits the active transaction, persisting all changes made within this unit of work.
    /// </summary>
    /// <param name="ct">A token to cancel the operation.</param>
    Task CommitAsync(CancellationToken ct = default);
}
