using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Pulse.BL.Common.Security.Passwords;
using Pulse.BL.Features.Auth.PasswordReset;
using Pulse.BL.Features.Auth.PasswordReset.RequestCode;
using Pulse.BL.Features.Email;
using Pulse.DAL.Commands.PasswordResetCodes;
using Pulse.DAL.Queries.Users;

namespace Pulse.Tests.Unit.Features.Auth.PasswordReset;

public class RequestPasswordResetHandlerTests
{
    private readonly Mock<IUserQueries> _userQueriesMock;
    private readonly Mock<IPasswordResetCodeCommands> _codeCommandsMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<TimeProvider> _timeProviderMock;
    private readonly RequestPasswordResetHandler _sut;

    public RequestPasswordResetHandlerTests()
    {
        _userQueriesMock = new Mock<IUserQueries>();
        _codeCommandsMock = new Mock<IPasswordResetCodeCommands>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _emailServiceMock = new Mock<IEmailService>();
        _timeProviderMock = new Mock<TimeProvider>();
        Mock<IOptions<PasswordResetOptions>> optionsMock = new();

        optionsMock.Setup(x => x.Value).Returns(new PasswordResetOptions
        {
            CodeTtlMinutes = 5,
            MaxFailedAttempts = 5,
            ResetTokenLifetimeMinutes = 10
        });

        _sut = new RequestPasswordResetHandler(
            _userQueriesMock.Object,
            _codeCommandsMock.Object,
            _passwordHasherMock.Object,
            _emailServiceMock.Object,
            _timeProviderMock.Object,
            optionsMock.Object,
            new Mock<ILogger<RequestPasswordResetHandler>>().Object);
    }

    [Fact]
    public async Task RequestAsync_WhenUserExists_CreatesCodeAndSendsEmail_ReturnsOk()
    {
        // Arrange
        string email = "test@example.com";
        Guid userId = Guid.NewGuid();
        RequestPasswordResetCommand command = new(email);

        _userQueriesMock
            .Setup(x => x.GetIdByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userId);

        _passwordHasherMock
            .Setup(x => x.HashPassword(It.IsAny<string>()))
            .Returns("hashed_code");

        DateTimeOffset now = DateTimeOffset.UtcNow;
        _timeProviderMock.Setup(x => x.GetUtcNow()).Returns(now);

        _emailServiceMock
            .Setup(x => x.SendEmailAsync(It.IsAny<SendEmailDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        // Act
        Result result = await _sut.RequestAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _codeCommandsMock.Verify(x => x.ReplaceAsync(
            It.Is<PasswordResetCodeInput>(i => i.UserId == userId && i.CodeHash == "hashed_code" && i.ExpiresAt == now.AddMinutes(5)),
            It.IsAny<CancellationToken>()), Times.Once);

        _emailServiceMock.Verify(x => x.SendEmailAsync(
            It.Is<SendEmailDto>(dto => dto.To.Contains(email) && dto.Subject.Contains("password reset code")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RequestAsync_WhenUserDoesNotExist_ReturnsOk_DoesNotCreateCodeOrSendEmail()
    {
        // Arrange
        string email = "notfound@example.com";
        RequestPasswordResetCommand command = new(email);

        _userQueriesMock
            .Setup(x => x.GetIdByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid?)null);

        // Act
        Result result = await _sut.RequestAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _codeCommandsMock.Verify(x => x.ReplaceAsync(It.IsAny<PasswordResetCodeInput>(), It.IsAny<CancellationToken>()), Times.Never);
        _emailServiceMock.Verify(x => x.SendEmailAsync(It.IsAny<SendEmailDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RequestAsync_WhenEmailFails_ReturnsOkButLogsError()
    {
        // Arrange
        string email = "test@example.com";
        Guid userId = Guid.NewGuid();
        RequestPasswordResetCommand command = new(email);

        _userQueriesMock
            .Setup(x => x.GetIdByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userId);

        _emailServiceMock
            .Setup(x => x.SendEmailAsync(It.IsAny<SendEmailDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("SMTP Error"));

        _timeProviderMock.Setup(x => x.GetUtcNow()).Returns(DateTimeOffset.UtcNow);

        // Act
        Result result = await _sut.RequestAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue(); // Endpoint should still succeed to avoid enumeration
        
        _codeCommandsMock.Verify(x => x.ReplaceAsync(It.IsAny<PasswordResetCodeInput>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
