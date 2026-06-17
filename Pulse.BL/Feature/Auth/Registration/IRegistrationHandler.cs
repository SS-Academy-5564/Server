using FluentResults;
using Pulse.BL.Common.Handlers;

namespace Pulse.BL.Feature.Auth.Registration;

public interface IRegistrationHandler : IAsyncHandler
{
    Task<Result> RegisterAsync(RegistrationCommand request, CancellationToken ct);
}
