using Dapper;
using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Commands.Members;

public class MemberCommands : IMemberCommands
{
    /// <inheritdoc/>
    public async Task CreateMemberAsync(CreateMemberInput input, IDbSession session, CancellationToken ct)
    {
        await session.Connection.ExecuteAsync(
            new CommandDefinition(
                "INSERT INTO Members (UserId, OrganizationId, RoleId, JoinedAt, UpdatedAt) " +
                "VALUES (@UserId, @OrganizationId, @RoleId, @Now, @Now)",
                new { input.UserId, input.OrganizationId, input.RoleId, Now = DateTimeOffset.UtcNow },
                transaction: session.Transaction,
                cancellationToken: ct));
    }
}
