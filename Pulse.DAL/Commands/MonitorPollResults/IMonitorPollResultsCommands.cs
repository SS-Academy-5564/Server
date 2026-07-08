using Pulse.DAL.Commands.Monitors;
using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Commands.MonitorPollResults;

public interface IMonitorPollResultsCommands : ICommands
{
    Task CreateAsync(CreateMonitorPollResultsInput monitorPollResultsInput, IUnitOfWork uow, CancellationToken ct);
}
