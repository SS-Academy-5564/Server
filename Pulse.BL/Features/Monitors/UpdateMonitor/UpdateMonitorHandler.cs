using FluentResults;
using Pulse.BL.Common.Errors;
using Pulse.BL.Common.Handlers;
using Pulse.DAL.Commands.Monitors;
using Pulse.DAL.Common.Repository;
using Pulse.DAL.Queries.Monitors;

namespace Pulse.BL.Features.Monitors.UpdateMonitor;

public class UpdateMonitorHandler : IAsyncHandler<UpdateMonitorCommand, Result<MonitorListResult>>
{
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IMonitorCommands _monitorCommands;
    private readonly IMonitorQueries _monitorQueries;

    public UpdateMonitorHandler(
        IUnitOfWorkFactory unitOfWorkFactory,
        IMonitorCommands monitorCommands,
        IMonitorQueries monitorQueries)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _monitorCommands = monitorCommands;
        _monitorQueries = monitorQueries;
    }

    /// <summary>
    /// Handles the updating of an existing monitor.
    /// </summary>
    /// <param name="command">The monitor configuration to update.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The updated monitor in list-projection shape.</returns>
    public async Task<Result<MonitorListResult>> HandleAsync(UpdateMonitorCommand command, CancellationToken ct)
    {
        MonitorRecord? existingMonitor = await _monitorQueries.GetByIdAsync(command.Id, ct);

        if (existingMonitor is null)
        {
            return Result.Fail(new NotFoundError("Monitor with this Id does not exist."));
        }

        await using IUnitOfWork uow = await _unitOfWorkFactory.CreateAsync(ct: ct);

        (Guid id, Guid organizationId) commandResult = await _monitorCommands.UpdateAsync(
            new UpdateMonitorInput(
                command.Id,
                command.Name,
                command.Url,
                command.HttpMethod,
                command.ResultPath,
                command.Status.ToString(),
                command.PollingIntervalSeconds,
                command.PollingTimeoutSeconds),
            ct);

        await uow.CommitAsync(ct);

        MonitorListResult result = new(
            commandResult.id,
            command.Name,
            command.Url,
            CurrentValue: null,
            LastCheckedAt: null,
            command.Status,
            command.PollingIntervalSeconds,
            commandResult.organizationId);

        return Result.Ok(result);
    }
}
