using FluentResults;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Common.Security;
using Pulse.DAL.Commands.Monitors;
using Pulse.DAL.Common.Repository;

namespace Pulse.BL.Features.Monitors;

public class CreateMonitorHandler : IAsyncHandler<CreateMonitorCommand, Result<MonitorListResult>>
{
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IMonitorCommands _monitorCommands;
    private readonly ICurrentUserService _currentUserService;

    public CreateMonitorHandler(
        IUnitOfWorkFactory unitOfWorkFactory,
        IMonitorCommands monitorCommands,
        ICurrentUserService currentUserService)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _monitorCommands = monitorCommands;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Handles the creation of a new monitor. The monitor is persisted in the
    /// <see cref="MonitorStatus.Enabled"/> status and becomes eligible for polling immediately.
    /// </summary>
    /// <param name="command">The monitor configuration to create.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The newly created monitor in list-projection shape.</returns>
    public async Task<Result<MonitorListResult>> HandleAsync(
        CreateMonitorCommand command,
        CancellationToken ct = default)
    {
        Result<Guid> organizationIdResult = _currentUserService.RequireOrganizationId();

        if (organizationIdResult.IsFailed)
        {
            return organizationIdResult.ToResult();
        }

        await using IUnitOfWork uow = await _unitOfWorkFactory.CreateAsync(ct: ct);

        Guid monitorId = await _monitorCommands.CreateAsync(
            new CreateMonitorInput(
                command.Name,
                command.Url,
                command.HttpMethod,
                command.ResultPath,
                command.PollingIntervalSeconds,
                command.PollingTimeoutSeconds,
                organizationIdResult.Value),
            ct);

        await uow.CommitAsync(ct);

        MonitorListResult result = new(
            monitorId,
            command.Name,
            command.Url,
            CurrentValue: null,
            LastCheckedAt: null,
            MonitorStatus.Enabled,
            command.PollingIntervalSeconds,
            organizationIdResult.Value);

        return Result.Ok(result);
    }
}
