using FluentResults;
using Pulse.BL.Common.Errors;
using Pulse.BL.Common.Security;
using Pulse.DAL.Queries.Users;

namespace Pulse.BL.Features.Users.Me;

public sealed class CurrentUserHandler : ICurrentUserHandler
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserQueries _userQueries;

    public CurrentUserHandler(ICurrentUserService currentUserService, IUserQueries userQueries)
    {
        _currentUserService = currentUserService;
        _userQueries = userQueries;
    }

    public async Task<Result<UserProfileResult>> GetCurrentUserAsync(CancellationToken ct)
    {
        Guid? userId = _currentUserService.UserId;

        if (userId is null)
        {
            return Result.Fail(new UnauthorizedError("User identity not found."));
        }

        UserProfileRecord? user = await _userQueries.GetByIdAsync(userId.Value, ct);

        if (user is null)
        {
            return Result.Fail(new NotFoundError("User not found."));
        }

        return Result.Ok(new UserProfileResult(user.Id, user.Email, user.CreatedAt, user.UpdatedAt));
    }
}
