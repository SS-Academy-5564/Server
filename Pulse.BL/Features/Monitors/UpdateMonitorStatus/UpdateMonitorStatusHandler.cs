using FluentResults;
using Pulse.BL.Common.Errors;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Common.Security;
using Pulse.DAL.Commands.Monitors;
using Pulse.DAL.Common.Repository;

namespace Pulse.BL.Features.Monitors.UpdateMonitorStatus;

/// <summary>
/// Handles updates to a monitor's status for the current organization.
/// </summary>
public class UpdateMonitorStatusHandler : IAsyncHandler<UpdateMonitorStatusCommand, Result>
{
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IMonitorCommands _monitorCommands;
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateMonitorStatusHandler"/> class.
    /// </summary>
    /// <param name="unitOfWorkFactory">The factory used to create unit-of-work instances.</param>
    /// <param name="monitorCommands">The monitor commands used to update monitor status values.</param>
    /// <param name="currentUserService">The service used to resolve the current organization identifier.</param>
    public UpdateMonitorStatusHandler(
        IUnitOfWorkFactory unitOfWorkFactory,
        IMonitorCommands monitorCommands,
        ICurrentUserService currentUserService)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _monitorCommands = monitorCommands;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Updates the status of a monitor for the current organization.
    /// </summary>
    /// <param name="command">The monitor status update command.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A result indicating whether the monitor status update succeeded.</returns>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
    public async Task<Result> HandleAsync(UpdateMonitorStatusCommand command, CancellationToken ct = default)
    {
        Result<Guid> organizationIdResult = _currentUserService.RequireOrganizationId();

        if (organizationIdResult.IsFailed)
        {
            return organizationIdResult.ToResult();
        }

        if (command.Status is not MonitorStatus.Enabled and not MonitorStatus.Disabled)
        {
            return Result.Fail(new ValidationError("Monitor status must be Enabled or Disabled."));
        }

        await using IUnitOfWork uow = await _unitOfWorkFactory.CreateAsync(ct: ct);

        int affectedRows = await _monitorCommands.UpdateStatusAsync(
            new UpdateMonitorStatusInput(
                command.MonitorId,
                organizationIdResult.Value,
                command.Status.ToString()),
            ct);

        if (affectedRows == 0)
        {
            return Result.Fail(new NotFoundError($"Monitor '{command.MonitorId}' was not found."));
        }

        await uow.CommitAsync(ct);

        return Result.Ok();
    }
}
