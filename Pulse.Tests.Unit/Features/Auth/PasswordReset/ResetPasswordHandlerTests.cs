using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using Pulse.BL.Common.Errors;
using Pulse.BL.Common.Security.Passwords;
using Pulse.BL.Common.Security.Tokens;
using Pulse.BL.Features.Auth.PasswordReset.ResetPassword;
using Pulse.DAL.Commands.PasswordResetCodes;
using Pulse.DAL.Commands.Users;

namespace Pulse.Tests.Unit.Features.Auth.PasswordReset;

public class ResetPasswordHandlerTests
{
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock;
    private readonly Mock<IUserCommands> _userCommandsMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IPasswordResetCodeCommands> _codeCommandsMock;
    private readonly ResetPasswordHandler _sut;

    public ResetPasswordHandlerTests()
    {
        _jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        _userCommandsMock = new Mock<IUserCommands>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _codeCommandsMock = new Mock<IPasswordResetCodeCommands>();

        _sut = new ResetPasswordHandler(
            _jwtTokenGeneratorMock.Object,
            _passwordHasherMock.Object,
            _userCommandsMock.Object,
            new Mock<ILogger<ResetPasswordHandler>>().Object);
    }

    [Fact]
    public async Task ResetAsync_WhenTokenIsValid_UpdatesPassword_ReturnsOk()
    {
        // Arrange
        string token = "valid_reset_token";
        string newPassword = "NewPassword123";
        Guid userId = Guid.NewGuid();
        string jti = Guid.NewGuid().ToString();
        ResetPasswordCommand command = new(token, newPassword);

        _jwtTokenGeneratorMock.Setup(x => x.ValidatePasswordResetTokenAsync(token)).ReturnsAsync((userId, jti));
        _passwordHasherMock.Setup(x => x.HashPassword(newPassword)).Returns("new_hashed_password");

        _userCommandsMock.Setup(x => x.ConsumeResetTokenAndUpdatePasswordAsync(userId, jti, "new_hashed_password", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        Result result = await _sut.ResetAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _userCommandsMock.Verify(x => x.ConsumeResetTokenAndUpdatePasswordAsync(userId, jti, "new_hashed_password", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ResetAsync_WhenTokenIsInvalid_ReturnsUnauthorizedError()
    {
        // Arrange
        string token = "invalid_token";
        ResetPasswordCommand command = new(token, "NewPassword123");

        _jwtTokenGeneratorMock.Setup(x => x.ValidatePasswordResetTokenAsync(token)).ReturnsAsync(((Guid, string)?)null);

        // Act
        Result result = await _sut.ResetAsync(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Should().BeOfType<UnauthorizedError>().Which.Message.Should().Be("The reset token is invalid or has expired.");

        _userCommandsMock.Verify(x => x.ConsumeResetTokenAndUpdatePasswordAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ResetAsync_WhenTokenAlreadyConsumedOrUserNotFound_ReturnsUnauthorizedError()
    {
        // Arrange
        string token = "valid_reset_token";
        string newPassword = "NewPassword123";
        Guid userId = Guid.NewGuid();
        string jti = Guid.NewGuid().ToString();
        ResetPasswordCommand command = new(token, newPassword);

        _jwtTokenGeneratorMock.Setup(x => x.ValidatePasswordResetTokenAsync(token)).ReturnsAsync((userId, jti));
        _passwordHasherMock.Setup(x => x.HashPassword(newPassword)).Returns("new_hashed_password");

        _userCommandsMock.Setup(x => x.ConsumeResetTokenAndUpdatePasswordAsync(userId, jti, "new_hashed_password", It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        Result result = await _sut.ResetAsync(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Should().BeOfType<UnauthorizedError>().Which.Message.Should().Be("The reset token is invalid or has already been used.");

        _userCommandsMock.Verify(x => x.ConsumeResetTokenAndUpdatePasswordAsync(userId, jti, "new_hashed_password", It.IsAny<CancellationToken>()), Times.Once);
    }
}
