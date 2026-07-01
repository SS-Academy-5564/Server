namespace Pulse.BL.Features.Auth.Login.LoginLockout;

public interface ILoginLockoutService
{
    Task<bool> TryReserveLoginAttemptAsync(Guid userId, CancellationToken ct);
    Task ResetAttemptsAsync(Guid userId, CancellationToken ct);
}
