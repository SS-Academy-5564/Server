using FluentResults;
using Pulse.BL.Common.Errors;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Common.Security;
using Pulse.DAL.Commands.DashboardWidgets;
using Pulse.DAL.Commands.DashboardWidgets.UpdateWidget;
using Pulse.DAL.Common.Repository;

namespace Pulse.BL.Features.DashboardWidgets.UpdateWidget;

/// <summary>
/// Updates the configuration of an existing widget.
/// </summary>
public class UpdateWidgetHandler : IAsyncHandler<UpdateWidgetCommand, Result>
{
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IWidgetCommands _widgetCommands;
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateWidgetHandler"/> class.
    /// </summary>
    /// <param name="unitOfWorkFactory">The unit of work factory.</param>
    /// <param name="widgetCommands">The widget commands.</param>
    /// <param name="currentUserService">The current user service.</param>
    public UpdateWidgetHandler(
        IUnitOfWorkFactory unitOfWorkFactory,
        IWidgetCommands widgetCommands,
        ICurrentUserService currentUserService)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _widgetCommands = widgetCommands;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Handles the update of a widget.
    /// </summary>
    /// <param name="command">The update command.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A success result when the widget was updated, or an error result otherwise.</returns>
    public async Task<Result> HandleAsync(
        UpdateWidgetCommand command,
        CancellationToken ct = default)
    {
        Result<Guid> organizationIdResult =
            _currentUserService.RequireOrganizationId();

        if (organizationIdResult.IsFailed)
        {
            return organizationIdResult.ToResult();
        }

        await using IUnitOfWork uow =
            await _unitOfWorkFactory.CreateAsync(ct: ct);

        bool updated = await _widgetCommands.UpdateAsync(
            new UpdateWidgetInput(
                command.WidgetId,
                command.Type,
                command.Title,
                command.Subtitle,
                command.Metric,
                command.TimeRange,
                command.Settings,
                organizationIdResult.Value),
            ct);

        if (!updated)
        {
            return Result.Fail(new NotFoundError("Widget not found."));
        }

        await uow.CommitAsync(ct);

        return Result.Ok();
    }
}
