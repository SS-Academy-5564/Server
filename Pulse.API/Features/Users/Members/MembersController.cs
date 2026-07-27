using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pulse.API.Attributes;
using Pulse.API.Controllers;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Common.Pagination;
using Pulse.BL.Features.Users.Members;

namespace Pulse.API.Features.Users.Members;

[ApiController]
[Route("api/members")]
[Authorize]
public sealed class MembersController : PulseControllerBase
{
    private readonly IAsyncHandler<
        GetOrganizationMembersQuery,
        Result<PagedResult<OrganizationMemberResult>>> _handler;

    public MembersController(
        IAsyncHandler<GetOrganizationMembersQuery, Result<PagedResult<OrganizationMemberResult>>> handler)
    {
        _handler = handler;
    }

    /// <summary>
    /// Retrieves the members of the current user's organization.
    /// </summary>
    /// <param name="request">The pagination parameters.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>An action result containing the organization member list.</returns>
    [HttpGet]
    public async Task<IActionResult> GetOrganizationMembersAsync(
        [FromQuery][Validate] GetOrganizationMembersRequest request,
        CancellationToken ct)
    {
        Result<PagedResult<OrganizationMemberResult>> result = await _handler.HandleAsync(
            new GetOrganizationMembersQuery(request.PageNumber, request.PageSize),
            ct);

        return ToPagedActionResult(result);
    }
}
