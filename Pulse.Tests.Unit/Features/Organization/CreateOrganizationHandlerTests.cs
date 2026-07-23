using System.Data;
using FluentAssertions;
using FluentResults;
using Moq;
using Pulse.BL.Common.Errors;
using Pulse.BL.Common.Security;
using Pulse.BL.Common.Security.Tokens;
using Pulse.BL.Features.Organization;
using Pulse.DAL.Commands.Members;
using Pulse.DAL.Commands.Organization;
using Pulse.DAL.Common.Repository;
using Pulse.DAL.Queries.Users;

namespace Pulse.Tests.Unit.Features.Organization;

public class CreateOrganizationHandlerTests
{
    private readonly Mock<IUnitOfWorkFactory> _uowFactoryMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IOrganizationCommands> _commandsMock = new();
    private readonly Mock<IJwtTokenGenerator> _jwtMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IMemberCommands> _memberCommandsMock = new();
    private readonly Mock<IUserQueries> _userQueriesMock = new();

    private readonly CreateOrganizationHandler _handler;

    public CreateOrganizationHandlerTests()
    {
        _uowFactoryMock
            .Setup(x => x.CreateAsync(
                It.IsAny<IsolationLevel>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(_uowMock.Object);

        _currentUserMock
            .SetupGet(x => x.UserId)
            .Returns(Guid.NewGuid());

        _commandsMock
            .Setup(x => x.CreateOrganizationAsync(
                It.IsAny<CreateOrganizationInput>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        _jwtMock
            .Setup(x => x.GenerateToken(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<string>()))
            .Returns(new GeneratedJwtToken(
                "test-token",
                DateTimeOffset.UtcNow.AddMinutes(30)));

        _userQueriesMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserProfileRecord(Guid.NewGuid(), "test@example.com", "Test", "Test", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow));

        _handler = new CreateOrganizationHandler(
            _uowFactoryMock.Object,
            _commandsMock.Object,
            _jwtMock.Object,
            _currentUserMock.Object,
            _memberCommandsMock.Object,
            _userQueriesMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_ShouldCreateOrganizationSuccessfully()
    {
        CreateOrganizationCommand command = new("Test Org");

        CreateOrganizationResult result =
            (await _handler.HandleAsync(command, CancellationToken.None)).Value;

        result.OrganizationId.Should().NotBeEmpty();
        result.AccessToken.Should().Be("test-token");
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_ShouldReturnOrganizationId()
    {
        var expectedId = Guid.NewGuid();

        _commandsMock
           .Setup(x => x.CreateOrganizationAsync(It.IsAny<CreateOrganizationInput>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync(expectedId);

        CreateOrganizationCommand command = new("Test Org");

        CreateOrganizationResult result =
            (await _handler.HandleAsync(command, CancellationToken.None)).Value;

        result.OrganizationId.Should().Be(expectedId);
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_ShouldCommitUnitOfWork()
    {
        var command = new CreateOrganizationCommand("Test Org");

        await _handler.HandleAsync(command, CancellationToken.None);

        _uowMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_UserDoesNotExist_ShouldReturnUnauthorizedError()
    {
        _userQueriesMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserProfileRecord?)null);

        var command = new CreateOrganizationCommand("Test Org");

        Result<CreateOrganizationResult> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is UnauthorizedError);
        _uowMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _userQueriesMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        _commandsMock.Verify(x => x.CreateOrganizationAsync(It.IsAny<CreateOrganizationInput>(), It.IsAny<CancellationToken>()), Times.Never);
        _memberCommandsMock.Verify(x => x.CreateMemberAsync(It.IsAny<CreateMemberInput>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
