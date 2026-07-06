using FluentAssertions;
using Moq;
using Pulse.BL.Common.Security.Tokens;
using Pulse.BL.Features.Organization;
using Pulse.DAL.Commands.Members;
using Pulse.DAL.Commands.Organization;
using Pulse.DAL.Common.Repository;
using Pulse.BL.Common.Security.CurrentUser;

namespace Pulse.Tests.Unit.Features.Organization;

public class CreateOrganizationHandlerTests
{
    private readonly Mock<IUnitOfWorkFactory> _uowFactoryMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IOrganizationCommands> _commandsMock = new();
    private readonly Mock<IJwtTokenGenerator> _jwtMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IMemberCommands> _memberCommandsMock = new();

    private readonly CreateOrganizationHandler _handler;

    public CreateOrganizationHandlerTests()
    {
        _uowFactoryMock
            .Setup(x => x.CreateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_uowMock.Object);

        _commandsMock
          .Setup(x => x.CreateOrganizationAsync(It.IsAny<CreateOrganizationInput>(), _uowMock.Object, It.IsAny<CancellationToken>()))
          .ReturnsAsync(Guid.NewGuid());

        _jwtMock
           .Setup(x => x.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>()))
           .Returns(new GeneratedJwtToken("test-token", DateTimeOffset.UtcNow.AddMinutes(30)));

        _handler = new CreateOrganizationHandler(
            _uowFactoryMock.Object,
            _commandsMock.Object,
            _jwtMock.Object,
            _currentUserMock.Object,
            _memberCommandsMock.Object
        );

    }

    [Fact]
    public async Task Should_create_organization_successfully()
    {
        CreateOrganizationCommand command = new("Test Org");

        CreateOrganizationResult result =
            (await _handler.HandleAsync(command, CancellationToken.None)).Value;

        result.OrganizationId.Should().NotBeEmpty();
        result.AccessToken.Should().Be("test-token");
    }

    [Fact]
    public async Task Should_return_organization_id()
    {
        var expectedId = Guid.NewGuid();

        _commandsMock
           .Setup(x => x.CreateOrganizationAsync(It.IsAny<CreateOrganizationInput>(), _uowMock.Object, It.IsAny<CancellationToken>()))
           .ReturnsAsync(expectedId);

        CreateOrganizationCommand command = new("Test Org");

        CreateOrganizationResult result =
            (await _handler.HandleAsync(command, CancellationToken.None)).Value;

        result.OrganizationId.Should().Be(expectedId);
    }

    [Fact]
    public async Task Should_commit_unit_of_work()
    {
        var command = new CreateOrganizationCommand("Test Org");

        await _handler.HandleAsync(command, CancellationToken.None);

        _uowMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
