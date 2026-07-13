using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pulse.API.Controllers;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Features.Users.Members;

namespace Pulse.API.Features.Users;

[ApiController]
[Route("api/members")]
[Authorize]
public sealed class MembersController : PulseControllerBase
{
    private readonly IAsyncQueryHandler<Result<OrganizationMembersResult>> _query;

    public MembersController(IAsyncQueryHandler<Result<OrganizationMembersResult>> query)
    {
        _query = query;
    }

    /// <summary>
    /// Retrieves the members of the current user's organization.
    /// </summary>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>An action result containing the organization member list.</returns>
    [HttpGet]
    public async Task<IActionResult> GetOrganizationMembersAsync(CancellationToken ct)
    {
        return ToActionResult(await _query.HandleAsync(ct));
    }
}
