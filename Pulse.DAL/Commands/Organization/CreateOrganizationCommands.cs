using Dapper;
using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Commands.Organization;

public class CreateOrganizationCommands : IOrganizationCommands
{
    private readonly IDbSessionAccessor _sessionAccessor;

    public CreateOrganizationCommands(IDbSessionAccessor sessionAccessor)
    {
        _sessionAccessor = sessionAccessor;
    }
    public async Task<Guid> CreateOrganizationAsync(CreateOrganizationInput input, CancellationToken ct)
    {
        IDbSession session = _sessionAccessor.Session
            ?? throw new InvalidOperationException("No active unit of work.");

        return await session.Connection.ExecuteScalarAsync<Guid>(
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
                transaction: session.Transaction,
                cancellationToken: ct));
    }
}
