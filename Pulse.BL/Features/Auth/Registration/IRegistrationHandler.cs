using FluentResults;
using Pulse.BL.Common.Handlers;

namespace Pulse.BL.Features.Auth.Registration;

public interface IRegistrationHandler : IAsyncHandler
{
    /// <summary>
    /// Registers a new user with the provided registration details.
    /// </summary>
    /// <param name="request">The command containing the user's registration data.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A <see cref="Result"/> indicating success or a failure with error details.</returns>
    Task<Result> RegisterAsync(RegistrationCommand request, CancellationToken ct);
}
