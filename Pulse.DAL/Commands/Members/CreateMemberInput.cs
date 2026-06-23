namespace Pulse.DAL.Commands.Members;

public record CreateMemberInput(Guid UserId, Guid OrganizationId, Guid RoleId);
