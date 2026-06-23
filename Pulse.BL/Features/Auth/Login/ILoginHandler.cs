using FluentResults;
using Pulse.BL.Common.Handlers;

namespace Pulse.BL.Features.Auth.Login;

public interface ILoginHandler : IAsyncHandler
{
    /// <summary>
    /// Authenticates a user using provided credentials and returns a JWT token if successful.
    /// </summary>
    /// <param name="command">The login command containing email and password.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>
    /// A result containing <see cref="LoginResult"/> on success,
    /// or a failure result if authentication fails.
    /// </returns>
    Task<Result<LoginResult>> LoginAsync(LoginCommand command, CancellationToken ct);
}