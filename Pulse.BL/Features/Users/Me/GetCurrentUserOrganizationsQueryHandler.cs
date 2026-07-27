using FluentResults;
using Pulse.BL.Common.Errors;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Common.Security;
using Pulse.DAL.Queries.Members;

namespace Pulse.BL.Features.Users.Me;

public sealed class GetCurrentUserOrganizationsQueryHandler
    : IAsyncQueryHandler<Result<IReadOnlyList<UserOrganizationResult>>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IMemberQueries _memberQueries;

    public GetCurrentUserOrganizationsQueryHandler(
        ICurrentUserService currentUserService,
        IMemberQueries memberQueries)
    {
        _currentUserService = currentUserService;
        _memberQueries = memberQueries;
    }

    public async Task<Result<IReadOnlyList<UserOrganizationResult>>> HandleAsync(CancellationToken ct = default)
    {
        Guid? userId = _currentUserService.UserId;

        if (userId is null)
        {
            return Result.Fail(new UnauthorizedError("User identity not found."));
        }

        IReadOnlyList<UserOrganizationRecord> organizations =
            await _memberQueries.GetOrganizationsByUserIdAsync(userId.Value, ct);

        IReadOnlyList<UserOrganizationResult> result = organizations
            .Select(o => new UserOrganizationResult(
                o.OrganizationId,
                o.OrganizationName,
                o.RoleId,
                o.RoleName,
                o.JoinedAt))
            .ToList();

        return Result.Ok(result);
    }
}
