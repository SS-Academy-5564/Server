using System.Data;
using Dapper;

namespace Pulse.DAL.Commands.Members;

public class MemberCommands : IMemberCommands
{
    /// <inheritdoc/>
    public async Task CreateMemberAsync(CreateMemberInput input, IDbTransaction transaction, CancellationToken ct)
    {
        await transaction.Connection!.ExecuteAsync(
            new CommandDefinition(
                "INSERT INTO Members (UserId, OrganizationId, RoleId, JoinedAt, UpdatedAt) " +
                "VALUES (@UserId, @OrganizationId, @RoleId, @Now, @Now)",
                new { UserId = input.UserId, OrganizationId = input.OrganizationId, RoleId = input.RoleId, Now = DateTimeOffset.UtcNow },
                transaction: transaction,
                cancellationToken: ct));
    }
}
