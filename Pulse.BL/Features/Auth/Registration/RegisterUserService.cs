using Pulse.BL.Common.Security;
using Pulse.DAL.Commands.Members;
using Pulse.DAL.Commands.Users;
using Pulse.DAL.Queries.Roles;
using Pulse.DAL.Queries.Users;

namespace Pulse.BL.Features.Auth.Registration;

public class RegisterUserService : IRegisterUserService
{
    private readonly IUserCommands _userCommands;
    private readonly IUserQueries _userQueries;
    private readonly IPasswordHasher _passwordHasher;

    // remove later
    private readonly IMemberCommands _memberCommands;
    private readonly IRoleQueries _roleQueries;


    public RegisterUserService(
        IUserCommands userCommands,
        IUserQueries userQueries,
        IPasswordHasher passwordHasher,
        IMemberCommands memberCommands,
        IRoleQueries roleQueries)
    {
        _userCommands = userCommands;
        _userQueries = userQueries;
        _passwordHasher = passwordHasher;
        _memberCommands = memberCommands;
        _roleQueries = roleQueries;
    }
    public async Task Register(RegisterUserRequest request)
    {
        var userExists = await _userQueries.EmailExistsAsync(request.Email);

        if (!userExists)
        {
            //return fail here
            return;
        }

        var passwordHash = _passwordHasher.HashPassword(request.Password);

        var userId = await _userCommands.CreateAsync(new CreateUserInput
        {
            Email = request.Email,
            PasswordHash = passwordHash
        });

        if (userId == Guid.Empty)
        {
            // return failure
            return;
        }

        var userRole = await _roleQueries.GetRoleByNameAsync("User");
        if (userRole is null)
        {
            // return failure
            return;
        }

        await _memberCommands.CreateMemberAsync(new CreateMemberInput
        {
            UserId = userId,
            OrganizationId = Guid.Parse("B1000000-0000-0000-0000-000000000001"),
            RoleId = userRole.Id
        });

        // return success
    }
}
