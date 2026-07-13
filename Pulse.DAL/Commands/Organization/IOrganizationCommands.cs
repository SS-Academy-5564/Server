using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Commands.Organization;

public interface IOrganizationCommands : ICommands
{
    Task<Guid> CreateOrganizationAsync(CreateOrganizationInput input, CancellationToken ct);
}
