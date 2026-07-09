using System.Data;

namespace Pulse.DAL.Common.Repository;

public interface IUnitOfWorkFactory
{
    /// <summary>
    /// Creates a new unit of work with an open connection and an active transaction.
    /// </summary>
    /// <param name="isolationLevel">The isolation level for the transaction.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A new <see cref="IUnitOfWork"/> ready to execute commands.</returns>
    Task<IUnitOfWork> CreateAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken ct = default);
}
