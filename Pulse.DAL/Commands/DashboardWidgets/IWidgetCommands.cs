using Pulse.DAL.Commands.DashboardWidgets.CreateWidget;
using Pulse.DAL.Commands.DashboardWidgets.UpdateWidget;
using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Commands.DashboardWidgets;

/// <summary>
/// Provides commands for persisting dashboard widgets.
/// </summary>
public interface IWidgetCommands : ICommands
{
    /// <summary>
    /// Creates a widget within the current organization.
    /// </summary>
    /// <param name="input">The widget configuration to persist.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The identifier of the created widget.</returns>
    Task<Guid> CreateAsync(CreateWidgetInput input, CancellationToken ct);

    /// <summary>
    /// Updates a widget within the current organization.
    /// </summary>
    /// <param name="input">The widget configuration to persist.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>Whether a matching widget was updated.</returns>
    Task<bool> UpdateAsync(UpdateWidgetInput input, CancellationToken ct);
}
