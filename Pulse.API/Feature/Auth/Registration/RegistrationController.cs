using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Pulse.API.Attributes;
using Pulse.API.Controllers;
using Pulse.BL.Feature.Auth.Registration;

namespace Pulse.API.Feature.Auth.Registration;

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
    public async Task<IActionResult> Register([Validate] RegistrationRequest request, CancellationToken ct)
    {
        var command = new RegistrationCommand
        {
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Password = request.Password
        };
        Result result = await _handler.RegisterAsync(command, ct);
        return ToActionResult(result);
    }
}
