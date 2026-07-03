using FluentResults;

namespace Pulse.BL.Features.Users.Me;

public interface ICurrentUserHandler
{
    Task<Result<UserProfileResult>> GetCurrentUserAsync(CancellationToken ct);
}
