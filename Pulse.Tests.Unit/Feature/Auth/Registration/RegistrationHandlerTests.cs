using System.Data;
using FluentAssertions;
using Moq;
using Pulse.BL.Common.Errors;
using Pulse.BL.Common.Security;
using Pulse.BL.Feature.Auth.Registration;
using Pulse.DAL.Commands.Members;
using Pulse.DAL.Commands.Users;
using Pulse.DAL.Common.Constants;
using Pulse.DAL.Common.Repository;
using Pulse.DAL.Exceptions;
using Pulse.DAL.Queries.Users;

namespace Pulse.Tests.Unit.Feature.Auth.Registration;

public class RegistrationHandlerTests
{
    private readonly Mock<IUnitOfWorkFactory> _unitOfWorkFactory = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IUserQueries> _userQueries = new();
    private readonly Mock<IUserCommands> _userCommands = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IMemberCommands> _memberCommands = new();
    private readonly RegistrationHandler _handler;

    public RegistrationHandlerTests()
    {
        _unitOfWork.Setup(u => u.Transaction).Returns(Mock.Of<IDbTransaction>());
        _unitOfWorkFactory
            .Setup(f => f.ExecuteAsync(It.IsAny<Func<IUnitOfWork, Task>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<IUnitOfWork, Task>, CancellationToken>((work, ct) => work(_unitOfWork.Object));

        _handler = new RegistrationHandler(
            _unitOfWorkFactory.Object,
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

        var result = await _handler.RegisterAsync(ValidCommand(), CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        _userCommands.Verify(c => c.CreateUserAsync(It.IsAny<CreateUserInput>(), It.IsAny<IDbTransaction>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsConflictErrorAndMemberIsNotCreated()
    {
        _userQueries
            .Setup(q => q.EmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _passwordHasher
            .Setup(h => h.HashPassword(It.IsAny<string>()))
            .Returns("hashed");
        _userCommands
            .Setup(c => c.CreateUserAsync(It.IsAny<CreateUserInput>(), It.IsAny<IDbTransaction>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DuplicateKeyException("Email"));

        var result = await _handler.RegisterAsync(ValidCommand(), CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is ConflictError);
        _memberCommands.Verify(m => m.CreateMemberAsync(It.IsAny<CreateMemberInput>(), It.IsAny<IDbTransaction>(), It.IsAny<CancellationToken>()), Times.Never);
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
            .Setup(c => c.CreateUserAsync(It.IsAny<CreateUserInput>(), It.IsAny<IDbTransaction>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userId);

        var result = await _handler.RegisterAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        _userCommands.Verify(c => c.CreateUserAsync(
            It.Is<CreateUserInput>(u =>
                u.Email == command.Email &&
                u.FirstName == command.FirstName &&
                u.LastName == command.LastName &&
                u.PasswordHash == hashedPassword),
            It.IsAny<IDbTransaction>(), It.IsAny<CancellationToken>()), Times.Once);

        _memberCommands.Verify(m => m.CreateMemberAsync(
            It.Is<CreateMemberInput>(mi =>
                mi.UserId == userId &&
                mi.OrganizationId == SeededIds.Organizations.Default &&
                mi.RoleId == SeededIds.Roles.User),
            It.IsAny<IDbTransaction>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    private static RegistrationCommand ValidCommand() => new()
    {
        Email = "john.doe@example.com",
        FirstName = "John",
        LastName = "Doe",
        Password = "SecurePass1"
    };
}
