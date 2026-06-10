
namespace Pulse.DAL.Commands.Members;

public class CreateMemberInput
{
    public Guid UserId { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid RoleId { get; set; }
}
