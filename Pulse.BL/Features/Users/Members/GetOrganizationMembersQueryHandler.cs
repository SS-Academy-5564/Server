using FluentResults;
using Pulse.BL.Common.Errors;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Common.Pagination;
using Pulse.BL.Common.Security;
using Pulse.DAL.Common.Pagination;
using Pulse.DAL.Queries.Members;

namespace Pulse.BL.Features.Users.Members;

/// <summary>
/// Handles queries for organization members.
/// </summary>
public sealed class GetOrganizationMembersQueryHandler
    : IAsyncHandler<GetOrganizationMembersQuery, Result<PagedResult<OrganizationMemberResult>>>
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
    /// <param name="query">The pagination parameters.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The organization members result.</returns>
    public async Task<Result<PagedResult<OrganizationMemberResult>>> HandleAsync(
        GetOrganizationMembersQuery query,
        CancellationToken ct = default)
    {
        Guid? organizationId = _currentUserService.OrganizationId;
        if (organizationId is null)
        {
            return Result.Fail(new UnauthorizedError("Organization identity not found."));
        }

        int pageNumber = query.PageNumber ?? PaginationDefaults.PageNumber;
        int pageSize = query.PageSize ?? PaginationDefaults.PageSize;

        PagedRecords<MemberRecord> memberRecords = await _memberQueries.GetMembersByOrganizationIdAsync(
            organizationId.Value,
            pageNumber,
            pageSize,
            ct);

        IReadOnlyList<OrganizationMemberResult> members = memberRecords.Items
            .Select(record => new OrganizationMemberResult(
                record.UserId,
                $"{record.FirstName} {record.LastName}",
                record.Email,
                record.RoleName,
                record.JoinedAt))
            .ToList()
            .AsReadOnly();

        return Result.Ok(
            new PagedResult<OrganizationMemberResult>(
                members,
                pageNumber,
                pageSize,
                memberRecords.TotalCount));
    }
}
