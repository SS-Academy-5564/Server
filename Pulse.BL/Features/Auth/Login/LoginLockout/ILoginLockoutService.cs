namespace Pulse.BL.Features.Auth.Login.LoginLockout;

public interface ILoginLockoutService
{
    Task<bool> IsUserAllowedAsync(Guid userId, CancellationToken ct);
}
