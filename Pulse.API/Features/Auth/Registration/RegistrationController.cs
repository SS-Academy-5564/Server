using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Pulse.API.Attributes;
using Pulse.API.Common.Localization;
using Pulse.API.Controllers;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Features.Auth.Registration;

namespace Pulse.API.Features.Auth.Registration;

/// <summary>
/// Exposes user registration operations.
/// </summary>
[ApiController]
[Route("api/auth")]
public class RegistrationController : PulseControllerBase
{
    private readonly IAsyncHandler<RegistrationCommand, Result<RegistrationResult>> _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegistrationController"/> class.
    /// </summary>
    /// <param name="handler">The registration command handler.</param>
    public RegistrationController(IAsyncHandler<RegistrationCommand, Result<RegistrationResult>> handler)
    {
        _handler = handler;
    }

    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <param name="request">The registration payload containing email, name, and password.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <param name="acceptLanguage">The preferred language supplied in the Accept-Language header.</param>
    /// <returns>200 OK with resend cooldown guidance on success, or a problem details response on failure.</returns>
    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync(
        [Validate] RegistrationRequest request,
        CancellationToken ct,
        [FromHeader(Name = "Accept-Language")] string? acceptLanguage = null)
    {
        string language = EmailLanguageResolver.Resolve(acceptLanguage ?? Request.Headers.AcceptLanguage.ToString());
        RegistrationCommand command = new(
            request.Email,
            request.FirstName,
            request.LastName,
            request.Password,
            language);
        Result<RegistrationResult> result = await _handler.HandleAsync(command, ct);

        return ToActionResult(result);
    }
}
