using Pulse.DAL.Commands.DashboardWidgets.CreateWidget;
using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Commands.DashboardWidgets;

public interface IWidgetCommands : ICommands
{
    Task<Guid> CreateAsync(CreateWidgetInput input, CancellationToken ct);
}
