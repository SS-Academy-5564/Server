using Dapper;
using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Commands.Members;

public class MemberCommands : IMemberCommands
{
    /// <inheritdoc/>
    public async Task CreateMemberAsync(CreateMemberInput input, IUnitOfWork uow, CancellationToken ct)
    {
        await uow.Connection.ExecuteAsync(
            new CommandDefinition(
                "INSERT INTO Members (UserId, OrganizationId, RoleId, JoinedAt, UpdatedAt) " +
                "VALUES (@UserId, @OrganizationId, @RoleId, @Now, @Now)",
                new { input.UserId, input.OrganizationId, input.RoleId, Now = DateTimeOffset.UtcNow },
                transaction: uow.Transaction,
                cancellationToken: ct));
    }
}
