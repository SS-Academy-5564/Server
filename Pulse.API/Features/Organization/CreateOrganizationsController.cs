using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pulse.API.Controllers;
using Pulse.BL.Features.Organization;

namespace Pulse.API.Features.Organization;

[ApiController]
[Route("api/organizations")]
[Authorize]
public class CreateOrganizationsController : PulseControllerBase
{

    private readonly ICreateOrganizationHandler _handler;

    public CreateOrganizationsController(ICreateOrganizationHandler handler)
    {
        _handler = handler;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrganizationAsync(CreateOrganizationRequest request, CancellationToken ct)
    {
        CreateOrganizationCommand command = new(request.Name);
        Result<CreateOrganizationResult> result = await _handler.HandleAsync(command, ct);
        return ToActionResult(result);
    }
}
