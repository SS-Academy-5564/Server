
using Pulse.BL.Common.Handlers;

namespace Pulse.BL.Features.Auth.Registration;

public interface IRegistrationHandler : IAsyncHandler
{
    Task Register(RegistrationCommand request, CancellationToken ct);
}
