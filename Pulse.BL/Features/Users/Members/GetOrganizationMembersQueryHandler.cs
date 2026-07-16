using FluentResults;
using Pulse.BL.Common.Errors;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Common.Security;
using Pulse.DAL.Queries.Members;

namespace Pulse.BL.Features.Users.Members;

/// <summary>
/// Handles queries for organization members.
/// </summary>
public sealed class GetOrganizationMembersQueryHandler : IAsyncQueryHandler<Result<OrganizationMembersResult>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IMemberQueries _memberQueries;

    public GetOrganizationMembersQueryHandler(
        ICurrentUserService currentUserService,
        IMemberQueries memberQueries)
    {
        _currentUserService = currentUserService;
        _memberQueries = memberQueries;
    }

    /// <summary>
    /// Retrieves the current organization's member list.
    /// </summary>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The organization members result.</returns>
    public async Task<Result<OrganizationMembersResult>> HandleAsync(CancellationToken ct = default)
    {
        Guid? organizationId = _currentUserService.OrganizationId;
        if (organizationId is null)
        {
            return Result.Fail(new UnauthorizedError("Organization identity not found."));
        }

        IReadOnlyList<MemberRecord> memberRecords = await _memberQueries.GetMembersByOrganizationIdAsync(organizationId.Value, ct);

        var members = memberRecords
            .Select(record => new OrganizationMemberResult(
                record.UserId,
                $"{record.FirstName} {record.LastName}",
                record.Email,
                record.RoleName,
                record.JoinedAt))
            .ToList();

        var result = new OrganizationMembersResult(members.Count, members);
        return Result.Ok(result);
    }
}
