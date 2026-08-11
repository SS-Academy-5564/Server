using FluentResults;
using Pulse.BL.Common.Errors;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Common.Security;
using Pulse.DAL.Commands.Monitors;
using Pulse.DAL.Common.Repository;

namespace Pulse.BL.Features.Monitors.UpdateMonitorStatus;

public class UpdateMonitorStatusHandler : IAsyncHandler<UpdateMonitorStatusCommand, Result>
{
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IMonitorCommands _monitorCommands;
    private readonly ICurrentUserService _currentUserService;

    public UpdateMonitorStatusHandler(
        IUnitOfWorkFactory unitOfWorkFactory,
        IMonitorCommands monitorCommands,
        ICurrentUserService currentUserService)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _monitorCommands = monitorCommands;
        _currentUserService = currentUserService;
    }

    public async Task<Result> HandleAsync(UpdateMonitorStatusCommand command, CancellationToken ct = default)
    {
        Result<Guid> organizationIdResult = _currentUserService.RequireOrganizationId();

        if (organizationIdResult.IsFailed)
        {
            return organizationIdResult.ToResult();
        }

        if (command.Status is not MonitorStatus.Enabled and not MonitorStatus.Disabled)
        {
            return Result.Fail(new ValidationError("Monitor status cannot be set to Error manually."));
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
