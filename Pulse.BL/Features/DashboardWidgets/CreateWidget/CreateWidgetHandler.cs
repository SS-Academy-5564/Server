using FluentResults;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Common.Security;
using Pulse.DAL.Commands.DashboardWidgets;
using Pulse.DAL.Commands.DashboardWidgets.CreateWidget;
using Pulse.DAL.Common.Repository;

namespace Pulse.BL.Features.DashboardWidgets.CreateWidget;

public class CreateWidgetHandler : IAsyncHandler<CreateWidgetCommand, Result<CreateWidgetResult>>
{
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IWidgetCommands _widgetCommands;
    private readonly ICurrentUserService _currentUserService;

    public CreateWidgetHandler(
        IUnitOfWorkFactory unitOfWorkFactory,
        IWidgetCommands widgetCommands,
        ICurrentUserService currentUserService)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _widgetCommands = widgetCommands;
        _currentUserService = currentUserService;
    }

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
                organizationIdResult.Value),
            ct);

        await uow.CommitAsync(ct);

        return Result.Ok(
            new CreateWidgetResult(widgetId));
    }
}
