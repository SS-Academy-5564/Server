using FluentResults;
using Pulse.BL.Common.Handlers;

namespace Pulse.BL.Features.Auth.Login;

public interface ILoginHandler : IAsyncHandler
{
    Task<Result<LoginResult>> LoginAsync(LoginCommand command, CancellationToken ct);
}
