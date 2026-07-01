using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Commands.LoginAttempts;

public interface IUserLoginAttemptsCommands : ICommands
{
    Task AddFailedAttemptAsync(
        Guid userId,
        int maxFailedAttempts,
        DateTime now,
        DateTime lockedUntil,
        CancellationToken ct);

    Task ResetAttemptsAsync(Guid userId, CancellationToken ct);
}
