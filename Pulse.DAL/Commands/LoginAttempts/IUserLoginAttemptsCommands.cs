using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Commands.LoginAttempts;

public interface IUserLoginAttemptsCommands : ICommands
{
    Task<bool> TryReserveLoginAttemptAsync(
        Guid userId,
        int maxAttempts,
        DateTime now,
        DateTime lockedUntil,
        CancellationToken ct);

    Task ResetAttemptsAsync(Guid userId, CancellationToken ct);
}
