using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Commands.Monitors;

public interface IMonitorCommands : ICommands
{
    Task UpdateAfterPollAsync(UpdateMonitorAfterPollInput input, IDbSession session, CancellationToken ct);
}
