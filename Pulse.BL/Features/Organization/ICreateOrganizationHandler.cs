using FluentResults;
using Pulse.BL.Common.Handlers;

namespace Pulse.BL.Features.Organization;

public interface ICreateOrganizationHandler
    : IAsyncHandler<CreateOrganizationCommand, Result<CreateOrganizationResult>>
{
}
