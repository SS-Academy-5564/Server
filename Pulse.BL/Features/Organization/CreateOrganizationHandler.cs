using FluentResults;
using Pulse.BL.Common.Security.CurrentUser;
using Pulse.BL.Common.Security.Tokens;
using Pulse.DAL.Commands.Organization;
using Pulse.DAL.Common.Repository;

namespace Pulse.BL.Features.Organization;

public class CreateOrganizationHandler : ICreateOrganizationHandler
{
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IOrganizationCommands _organizationCommands;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ICurrentUserService _currentUserService;

    public CreateOrganizationHandler(
        IUnitOfWorkFactory unitOfWorkFactory,
        IOrganizationCommands organizationCommands,
        IJwtTokenGenerator jwtTokenGenerator,
        ICurrentUserService currentUserService)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _organizationCommands = organizationCommands;
        _jwtTokenGenerator = jwtTokenGenerator;
        _currentUserService = currentUserService;
    }

    public async Task<Result<CreateOrganizationResult>> CreateOrganizationAsync(
        CreateOrganizationCommand command,
        CancellationToken ct)
    {
        await using IUnitOfWork uow =
            await _unitOfWorkFactory.CreateAsync(ct);

        Guid organizationId = await _organizationCommands.CreateOrganizationAsync(new CreateOrganizationInput(command.Name), uow, ct);

        await uow.CommitAsync(ct);

        Guid userId = _currentUserService.UserId;
        string role = _currentUserService.Role;

        GeneratedJwtToken jwt = _jwtTokenGenerator.GenerateToken(userId, role, organizationId);

        string accessToken = jwt.Token;

        return Result.Ok(new CreateOrganizationResult(
            organizationId,
            accessToken));
    }
}
