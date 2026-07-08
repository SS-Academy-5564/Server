using FluentResults;
using Pulse.BL.Common.Errors;
using Pulse.BL.Common.Security;
using Pulse.BL.Common.Security.Tokens;
using Pulse.DAL.Commands.Members;
using Pulse.DAL.Commands.Organization;
using Pulse.DAL.Common.Constants;
using Pulse.DAL.Common.Repository;

namespace Pulse.BL.Features.Organization;

public class CreateOrganizationHandler
{
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IOrganizationCommands _organizationCommands;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMemberCommands _memberCommands;

    public CreateOrganizationHandler(
        IUnitOfWorkFactory unitOfWorkFactory,
        IOrganizationCommands organizationCommands,
        IJwtTokenGenerator jwtTokenGenerator,
        ICurrentUserService currentUserService,
        IMemberCommands memberCommands)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _organizationCommands = organizationCommands;
        _jwtTokenGenerator = jwtTokenGenerator;
        _currentUserService = currentUserService;
        _memberCommands = memberCommands;
    }

    public async Task<Result<CreateOrganizationResult>> HandleAsync(
        CreateOrganizationCommand command,
        CancellationToken ct)
    {
        await using IUnitOfWork uow =
            await _unitOfWorkFactory.CreateAsync(ct);

        Guid? userId = _currentUserService.UserId;

        if (userId is null)
        {
            return Result.Fail(new UnauthorizedError("User identity not found."));
        }

        Guid currentUserId = userId.Value;
        string role = _currentUserService.Role;

        Guid organizationId =
            await _organizationCommands.CreateOrganizationAsync(
                new CreateOrganizationInput(command.Name),
                uow,
                ct);

        await _memberCommands.CreateMemberAsync(
            new CreateMemberInput(
                currentUserId,
                organizationId,
                SeededIds.Roles.User
            ),
            uow,
            ct);

        await uow.CommitAsync(ct);

        GeneratedJwtToken jwt = _jwtTokenGenerator.GenerateToken(currentUserId, role, organizationId);

        string accessToken = jwt.Token;

        return Result.Ok(new CreateOrganizationResult(
            organizationId,
            accessToken));
    }
}
