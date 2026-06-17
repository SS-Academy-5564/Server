using Microsoft.AspNetCore.Mvc;
using Pulse.API.Controllers;
using Pulse.API.Filters;
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
        var result = await _handler.RegisterAsync(command, ct);
        return ToActionResult(result);
    }
}
