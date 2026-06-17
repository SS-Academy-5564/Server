using Microsoft.AspNetCore.Mvc;
using Pulse.API.Attributes;
using Pulse.BL.Features.Auth.Registration;

namespace Pulse.API.Features.Auth.Registration;

[ApiController]
[Route("api/auth")]
[AutoValidate]
public class RegistrationController : ControllerBase
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
        await _handler.Register(command, ct);
        return Ok();
    }
}
