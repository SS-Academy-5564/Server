namespace Pulse.DAL.Common.Repository;

public interface IUnitOfWorkFactory
{
    Task<IUnitOfWork> CreateAsync(CancellationToken ct = default);
    Task ExecuteAsync(Func<IUnitOfWork, Task> work, CancellationToken ct = default);
}
