namespace Pulse.DAL.Common.Repository;

public interface IUnitOfWork : IDbSession, IAsyncDisposable
{
    /// <summary>
    /// Commits the active transaction, persisting all changes made within this unit of work.
    /// </summary>
    /// <param name="ct">A token to cancel the operation.</param>
    Task CommitAsync(CancellationToken ct = default);
}
