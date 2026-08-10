using Pulse.DAL.Commands.DashboardWidgets.CreateWidget;
using Pulse.DAL.Commands.DashboardWidgets.UpdateWidget;
using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Commands.DashboardWidgets;

public interface IWidgetCommands : ICommands
{
    Task<Guid> CreateAsync(CreateWidgetInput input, CancellationToken ct);

    /// <summary>
    /// Updates a widget within the current organization.
    /// </summary>
    /// <param name="input">The widget configuration to persist.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>Whether a matching widget was updated.</returns>
    Task<bool> UpdateAsync(UpdateWidgetInput input, CancellationToken ct);
}
