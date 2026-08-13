using FluentResults;
using Pulse.BL.Common.Errors;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Common.Security;
using Pulse.DAL.Commands.DashboardWidgets;
using Pulse.DAL.Commands.DashboardWidgets.CreateWidget;
using Pulse.DAL.Common.Repository;
using Pulse.DAL.Queries.Monitors;

namespace Pulse.BL.Features.DashboardWidgets.CreateWidget;

/// <summary>
/// Handles the creation of a new dashboard widget.
/// </summary>
public class CreateWidgetHandler : IAsyncHandler<CreateWidgetCommand, Result<CreateWidgetResult>>
{
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IWidgetCommands _widgetCommands;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMonitorQueries _monitorQueries;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateWidgetHandler"/> class.
    /// </summary>
    /// <param name="unitOfWorkFactory">The factory used to create unit-of-work instances.</param>
    /// <param name="widgetCommands">The commands used to create dashboard widgets.</param>
    /// <param name="currentUserService">The service used to resolve current user and organization information.</param>
    /// <param name="monitorQueries">The queries used to retrieve monitor data.</param>
    public CreateWidgetHandler(
        IUnitOfWorkFactory unitOfWorkFactory,
        IWidgetCommands widgetCommands,
        ICurrentUserService currentUserService,
        IMonitorQueries monitorQueries)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _widgetCommands = widgetCommands;
        _currentUserService = currentUserService;
        _monitorQueries = monitorQueries;
    }

    /// <summary>
    /// Creates a dashboard widget after validating organization access and monitor ownership.
    /// </summary>
    /// <param name="command">The command containing widget creation details.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A result containing the created widget identifier or failure details.</returns>
    public async Task<Result<CreateWidgetResult>> HandleAsync(
        CreateWidgetCommand command,
        CancellationToken ct = default)
    {
        Result<Guid> organizationIdResult =
            _currentUserService.RequireOrganizationId();

        if (organizationIdResult.IsFailed)
        {
            return organizationIdResult.ToResult();
        }

        MonitorRecord? monitor = await _monitorQueries.GetByIdAsync(command.MonitorId, ct);

        if (monitor is null)
        {
            return Result.Fail(new NotFoundError($"Monitor '{command.MonitorId}' was not found."));
        }

        if (monitor.OrganizationId != organizationIdResult.Value)
        {
            return Result.Fail(new ForbiddenError("Monitor belongs to another organization."));
        }

        await using IUnitOfWork uow =
            await _unitOfWorkFactory.CreateAsync(ct: ct);

        Guid widgetId = await _widgetCommands.CreateAsync(
            new CreateWidgetInput(
                command.DashboardTabId,
                command.Type,
                command.Title,
                command.Subtitle,
                command.Metric,
                command.TimeRange,
                command.Settings,
                command.MonitorId,
                organizationIdResult.Value),
            ct);

        await uow.CommitAsync(ct);

        return Result.Ok(
            new CreateWidgetResult(widgetId));
    }
}
