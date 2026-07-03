using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Commands.Organization;

public interface IOrganizationCommands
{
    Task<Guid> CreateOrganizationAsync(CreateOrganizationInput input, IUnitOfWork uow, CancellationToken ct);
}
