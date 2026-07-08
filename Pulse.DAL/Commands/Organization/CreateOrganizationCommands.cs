using Dapper;
using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Commands.Organization;

public class CreateOrganizationCommands : IOrganizationCommands
{
    public async Task<Guid> CreateOrganizationAsync(CreateOrganizationInput input, IUnitOfWork uow, CancellationToken ct)
    {
        return await uow.Connection.ExecuteScalarAsync<Guid>(
            new CommandDefinition(
                """
                INSERT INTO Organizations (Name, CreatedAt, UpdatedAt)
                OUTPUT INSERTED.Id
                VALUES (@Name, @Now, @Now)
                """,
                new
                {
                    input.Name,
                    Now = DateTimeOffset.UtcNow
                },
                transaction: uow.Transaction,
                cancellationToken: ct));
    }
}
