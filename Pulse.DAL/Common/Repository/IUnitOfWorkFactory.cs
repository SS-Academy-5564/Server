namespace Pulse.DAL.Common.Repository;

public interface IUnitOfWorkFactory
{
    /// <summary>
    /// Creates a new unit of work with an open connection and an active transaction.
    /// </summary>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A new <see cref="IUnitOfWork"/> ready to execute commands.</returns>
    Task<IUnitOfWork> CreateAsync(CancellationToken ct = default);

    /// <summary>
    /// Executes a delegate within a unit of work, committing on success and rolling back on failure.
    /// </summary>
    /// <param name="work">The delegate containing the operations to execute transactionally.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    Task ExecuteAsync(Func<IUnitOfWork, Task> work, CancellationToken ct = default);
}
