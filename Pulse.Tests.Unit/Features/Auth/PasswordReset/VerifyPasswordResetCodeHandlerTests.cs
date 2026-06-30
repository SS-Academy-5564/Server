using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Pulse.BL.Common.Errors;
using Pulse.BL.Common.Security.Passwords;
using Pulse.BL.Common.Security.Tokens;
using Pulse.BL.Features.Auth.PasswordReset;
using Pulse.BL.Features.Auth.PasswordReset.VerifyCode;
using Pulse.DAL.Commands.PasswordResetCodes;
using Pulse.DAL.Queries.PasswordResetCodes;
using Pulse.DAL.Queries.Users;

namespace Pulse.Tests.Unit.Features.Auth.PasswordReset;

public class VerifyPasswordResetCodeHandlerTests
{
    private readonly Mock<IUserQueries> _userQueriesMock;
    private readonly Mock<IPasswordResetCodeQueries> _codeQueriesMock;
    private readonly Mock<IPasswordResetCodeCommands> _codeCommandsMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock;
    private readonly Mock<TimeProvider> _timeProviderMock;
    private readonly VerifyPasswordResetCodeHandler _sut;

    public VerifyPasswordResetCodeHandlerTests()
    {
        _userQueriesMock = new Mock<IUserQueries>();
        _codeQueriesMock = new Mock<IPasswordResetCodeQueries>();
        _codeCommandsMock = new Mock<IPasswordResetCodeCommands>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        _timeProviderMock = new Mock<TimeProvider>();
        Mock<IOptions<PasswordResetOptions>> optionsMock = new();

        optionsMock.Setup(x => x.Value).Returns(new PasswordResetOptions
        {
            CodeTtlMinutes = 5,
            MaxFailedAttempts = 5,
            ResetTokenLifetimeMinutes = 10
        });

        _sut = new VerifyPasswordResetCodeHandler(
            _userQueriesMock.Object,
            _codeQueriesMock.Object,
            _codeCommandsMock.Object,
            _passwordHasherMock.Object,
            _jwtTokenGeneratorMock.Object,
            _timeProviderMock.Object,
            optionsMock.Object,
            new Mock<ILogger<VerifyPasswordResetCodeHandler>>().Object);
    }

    [Fact]
    public async Task VerifyAsync_WhenCodeValid_ReturnsResetToken_DeletesCode()
    {
        // Arrange
        string email = "test@example.com";
        string code = "123456";
        Guid userId = Guid.NewGuid();
        Guid codeId = Guid.NewGuid();
        string resetToken = "generated_jwt_token";

        PasswordResetCodeRecord record = new(codeId, userId, "hashed_code", DateTimeOffset.UtcNow.AddMinutes(5), 0);

        _userQueriesMock.Setup(x => x.GetIdByEmailAsync(email, It.IsAny<CancellationToken>())).ReturnsAsync(userId);
        _codeQueriesMock.Setup(x => x.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(record);
        _timeProviderMock.Setup(x => x.GetUtcNow()).Returns(DateTimeOffset.UtcNow);
        _passwordHasherMock.Setup(x => x.VerifyHashedPassword("hashed_code", code)).Returns(true);
        _jwtTokenGeneratorMock.Setup(x => x.GeneratePasswordResetToken(userId, It.IsAny<string>(), TimeSpan.FromMinutes(10))).Returns(resetToken);
        _codeCommandsMock.Setup(x => x.MarkAsVerifiedAsync(codeId, It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        VerifyPasswordResetCodeCommand command = new(email, code);

        // Act
        Result<VerifyCodeResult> result = await _sut.VerifyAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ResetToken.Should().Be(resetToken);

        _codeCommandsMock.Verify(x => x.MarkAsVerifiedAsync(codeId, It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task VerifyAsync_WhenCodeAlreadyConsumed_ReturnsValidationError()
    {
        // Arrange
        string email = "test@example.com";
        string code = "123456";
        Guid userId = Guid.NewGuid();
        Guid codeId = Guid.NewGuid();

        PasswordResetCodeRecord record = new(codeId, userId, "hashed_code", DateTimeOffset.UtcNow.AddMinutes(5), 0);

        _userQueriesMock.Setup(x => x.GetIdByEmailAsync(email, It.IsAny<CancellationToken>())).ReturnsAsync(userId);
        _codeQueriesMock.Setup(x => x.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(record);
        _timeProviderMock.Setup(x => x.GetUtcNow()).Returns(DateTimeOffset.UtcNow);
        _passwordHasherMock.Setup(x => x.VerifyHashedPassword("hashed_code", code)).Returns(true);

        // Setup MarkAsVerifiedAsync to return false, simulating concurrent consumption
        _codeCommandsMock.Setup(x => x.MarkAsVerifiedAsync(codeId, It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        VerifyPasswordResetCodeCommand command = new(email, code);

        // Act
        Result<VerifyCodeResult> result = await _sut.VerifyAsync(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Should().BeOfType<ValidationError>().Which.Message.Should().Be("Invalid code.");

        _codeCommandsMock.Verify(x => x.MarkAsVerifiedAsync(codeId, It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Once);
        _jwtTokenGeneratorMock.Verify(x => x.GeneratePasswordResetToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<TimeSpan>()), Times.Never);
    }

    [Fact]
    public async Task VerifyAsync_WhenUserDoesNotExist_ReturnsValidationError()
    {
        // Arrange
        string email = "notfound@example.com";
        VerifyPasswordResetCodeCommand command = new(email, "123456");

        _userQueriesMock.Setup(x => x.GetIdByEmailAsync(email, It.IsAny<CancellationToken>())).ReturnsAsync((Guid?)null);

        // Act
        Result<VerifyCodeResult> result = await _sut.VerifyAsync(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Should().BeOfType<ValidationError>().Which.Message.Should().Be("Invalid code.");
    }

    [Fact]
    public async Task VerifyAsync_WhenCodeDoesNotExist_ReturnsValidationError()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        VerifyPasswordResetCodeCommand command = new("test@example.com", "123456");

        _userQueriesMock.Setup(x => x.GetIdByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(userId);
        _codeQueriesMock.Setup(x => x.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((PasswordResetCodeRecord?)null);

        // Act
        Result<VerifyCodeResult> result = await _sut.VerifyAsync(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Should().BeOfType<ValidationError>().Which.Message.Should().Be("Invalid code.");
    }

    [Fact]
    public async Task VerifyAsync_WhenCodeExpired_DeletesCode_ReturnsValidationError()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        PasswordResetCodeRecord record = new(Guid.NewGuid(), userId, "hash", DateTimeOffset.UtcNow.AddMinutes(-1), 0);
        VerifyPasswordResetCodeCommand command = new("test@example.com", "123456");

        _userQueriesMock.Setup(x => x.GetIdByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(userId);
        _codeQueriesMock.Setup(x => x.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(record);
        _timeProviderMock.Setup(x => x.GetUtcNow()).Returns(DateTimeOffset.UtcNow);

        // Act
        Result<VerifyCodeResult> result = await _sut.VerifyAsync(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Should().BeOfType<ValidationError>().Which.Message.Should().Be("The code has expired. Please request a new one.");
        _codeCommandsMock.Verify(x => x.DeleteByIdAsync(record.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task VerifyAsync_WhenCodeIsWrong_IncrementsFailedAttempts_ReturnsValidationError()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid codeId = Guid.NewGuid();
        PasswordResetCodeRecord record = new(codeId, userId, "hash", DateTimeOffset.UtcNow.AddMinutes(5), 0);
        VerifyPasswordResetCodeCommand command = new("test@example.com", "wrong!");

        _userQueriesMock.Setup(x => x.GetIdByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(userId);
        _codeQueriesMock.Setup(x => x.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(record);
        _timeProviderMock.Setup(x => x.GetUtcNow()).Returns(DateTimeOffset.UtcNow);
        _passwordHasherMock.Setup(x => x.VerifyHashedPassword("hash", "wrong!")).Returns(false);

        _codeCommandsMock.Setup(x => x.IncrementFailedAttemptsAsync(codeId, It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        Result<VerifyCodeResult> result = await _sut.VerifyAsync(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Should().BeOfType<ValidationError>().Which.Message.Should().Be("Invalid code.");
        _codeCommandsMock.Verify(x => x.IncrementFailedAttemptsAsync(codeId, It.IsAny<CancellationToken>()), Times.Once);
        _codeCommandsMock.Verify(x => x.DeleteByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);

    }

    [Fact]
    public async Task VerifyAsync_WhenCodeIsWrong_ExceedsMaxAttempts_DeletesCode_ReturnsValidationError()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid codeId = Guid.NewGuid();
        PasswordResetCodeRecord record = new(codeId, userId, "hash", DateTimeOffset.UtcNow.AddMinutes(5), 4);
        VerifyPasswordResetCodeCommand command = new("test@example.com", "wrong!");

        _userQueriesMock.Setup(x => x.GetIdByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(userId);
        _codeQueriesMock.Setup(x => x.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(record);
        _timeProviderMock.Setup(x => x.GetUtcNow()).Returns(DateTimeOffset.UtcNow);
        _passwordHasherMock.Setup(x => x.VerifyHashedPassword("hash", "wrong!")).Returns(false);

        _codeCommandsMock.Setup(x => x.IncrementFailedAttemptsAsync(codeId, It.IsAny<CancellationToken>())).ReturnsAsync(5); // Returns max attempts

        // Act
        Result<VerifyCodeResult> result = await _sut.VerifyAsync(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Should().BeOfType<ValidationError>().Which.Message.Should().Be("Invalid code.");
        _codeCommandsMock.Verify(x => x.DeleteByIdAsync(record.Id, It.IsAny<CancellationToken>()), Times.Once);
    }
}
