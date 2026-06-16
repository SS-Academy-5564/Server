
using FluentResults;
using Pulse.BL.Common.Handlers;

namespace Pulse.BL.Feature.Auth.Registration;

public interface IRegistrationHandler : IAsyncHandler
{
    Task<Result> Register(RegistrationCommand request, CancellationToken ct);
}
