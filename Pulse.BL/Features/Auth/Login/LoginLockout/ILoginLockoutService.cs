namespace Pulse.BL.Features.Auth.Login.LoginLockout;

public interface ILoginLockoutService
{
    Task<bool> IsUserAllowedAsync(Guid userId, CancellationToken ct);
    Task AddFailedAttemptAsync(Guid userId, CancellationToken ct);
    Task ResetAttemptsAsync(Guid userId, CancellationToken ct);
}
