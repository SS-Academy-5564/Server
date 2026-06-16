using FluentAssertions;
using Moq;
using Pulse.BL.Common.Security;
using Pulse.BL.Feature.Auth.Registration;
using Pulse.DAL.Commands.Members;
using Pulse.DAL.Commands.Users;
using Pulse.DAL.Common.Constants;
using Pulse.DAL.Queries.Users;

namespace Pulse.Tests.Unit.Feature.Auth.Registration;

public class RegistrationHandlerTests
{
    private readonly Mock<IUserQueries> _userQueries = new();
    private readonly Mock<IUserCommands> _userCommands = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IMemberCommands> _memberCommands = new();
    private readonly RegistrationHandler _handler;

    public RegistrationHandlerTests()
    {
        _handler = new RegistrationHandler(
            _userCommands.Object,
            _userQueries.Object,
            _passwordHasher.Object,
            _memberCommands.Object);
    }

    [Fact]
    public async Task Register_EmailAlreadyExists_ReturnsFailResult()
    {
        _userQueries
            .Setup(q => q.EmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.Register(ValidCommand(), CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        _userCommands.Verify(c => c.CreateAsync(It.IsAny<CreateUserInput>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Register_CreateAsyncReturnsEmptyGuid_ReturnsFailResult()
    {
        _userQueries
            .Setup(q => q.EmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _passwordHasher
            .Setup(h => h.HashPassword(It.IsAny<string>()))
            .Returns("hashed");
        _userCommands
            .Setup(c => c.CreateAsync(It.IsAny<CreateUserInput>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.Empty);

        var result = await _handler.Register(ValidCommand(), CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        _memberCommands.Verify(m => m.CreateMemberAsync(It.IsAny<CreateMemberInput>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Register_ValidCommand_HashesPasswordAndCreatesUserAndMember()
    {
        var command = ValidCommand();
        var userId = Guid.NewGuid();
        const string hashedPassword = "hashed_password";

        _userQueries
            .Setup(q => q.EmailExistsAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _passwordHasher
            .Setup(h => h.HashPassword(command.Password))
            .Returns(hashedPassword);
        _userCommands
            .Setup(c => c.CreateAsync(It.IsAny<CreateUserInput>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userId);

        var result = await _handler.Register(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        _userCommands.Verify(c => c.CreateAsync(
            It.Is<CreateUserInput>(u =>
                u.Email == command.Email &&
                u.FirstName == command.FirstName &&
                u.LastName == command.LastName &&
                u.PasswordHash == hashedPassword),
            It.IsAny<CancellationToken>()), Times.Once);

        _memberCommands.Verify(m => m.CreateMemberAsync(
            It.Is<CreateMemberInput>(mi =>
                mi.UserId == userId &&
                mi.OrganizationId == SeededIds.Organizations.Default &&
                mi.RoleId == SeededIds.Roles.User),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    private static RegistrationCommand ValidCommand() => new()
    {
        Email = "john.doe@example.com",
        FirstName = "John",
        LastName = "Doe",
        Password = "SecurePass1"
    };
}
