using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Commands.Monitors;

/// <summary>
/// Defines write operations for monitors.
/// </summary>
public interface IMonitorCommands : ICommands
{
    /// <summary>
    /// Creates a new monitor within the active unit of work.
    /// The monitor is created in the <c>Enabled</c> status and becomes eligible for polling immediately.
    /// </summary>
    /// <param name="input">The monitor configuration to persist.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The identifier of the newly created monitor.</returns>
    Task<Guid> CreateAsync(CreateMonitorInput input, CancellationToken ct);

    /// <summary>
    /// Updates a monitor after a polling attempt within the specified database session.
    /// When the current value is <see langword="null"/>, the previously stored value is preserved.
    /// </summary>
    /// <param name="input">The monitor ID and polling state to persist.</param>
    /// <param name="session">The database session whose connection and transaction are used.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateAfterPollAsync(UpdateMonitorAfterPollInput input, IDbSession session, CancellationToken ct);
}
