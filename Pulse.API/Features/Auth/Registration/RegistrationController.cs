using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Pulse.API.Attributes;
using Pulse.API.Controllers;
using Pulse.BL.Features.Auth.Registration;

namespace Pulse.API.Features.Auth.Registration;

[ApiController]
[Route("api/auth")]
public class RegistrationController : PulseControllerBase
{
    private readonly IRegistrationHandler _handler;
    public RegistrationController(IRegistrationHandler handler)
    {
        _handler = handler;
    }

    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <param name="request">The registration payload containing email, name, and password.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>200 OK on success, or a problem details response on failure.</returns>
    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync([Validate] RegistrationRequest request, CancellationToken ct)
    {
        RegistrationCommand command = new(request.Email, request.FirstName, request.LastName, request.Password);
        Result result = await _handler.RegisterAsync(command, ct);

        return ToActionResult(result);
    }
}
