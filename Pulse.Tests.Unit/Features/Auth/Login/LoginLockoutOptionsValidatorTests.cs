using FluentAssertions;
using Microsoft.Extensions.Options;
using Pulse.BL.Features.Auth.Login.LoginLockout;

namespace Pulse.Tests.Unit.Features.Auth.Login;

public class LoginLockoutOptionsValidatorTests
{
    private readonly LoginLockoutOptionsValidator _sut = new();

    [Fact]
    public void Validate_WhenOptionsValid_ReturnsSuccess()
    {
        // Arrange
        LoginLockoutOptions options = CreateOptions();

        // Act
        ValidateOptionsResult result = _sut.Validate(null, options);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WhenMaxFailedAttemptsNotPositive_ReturnsFailure(
        int maxFailedAttempts)
    {
        // Arrange
        LoginLockoutOptions options = CreateOptions(
            maxFailedAttempts: maxFailedAttempts);

        // Act
        ValidateOptionsResult result = _sut.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain(
            $"{LoginLockoutOptions.SectionName}:MaxFailedAttempts must be greater than zero.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WhenLockoutDurationNotPositive_ReturnsFailure(
        int lockoutDurationMinutes)
    {
        // Arrange
        LoginLockoutOptions options = CreateOptions(
            lockoutDurationMinutes: lockoutDurationMinutes);

        // Act
        ValidateOptionsResult result = _sut.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain(
            $"{LoginLockoutOptions.SectionName}:LockoutDurationMinutes must be greater than zero.");
    }

    [Fact]
    public void Validate_WhenBothValuesInvalid_ReturnsBothFailures()
    {
        // Arrange
        LoginLockoutOptions options = CreateOptions(
            maxFailedAttempts: 0,
            lockoutDurationMinutes: 0);

        // Act
        ValidateOptionsResult result = _sut.Validate(null, options);

        // Assert
        result.Failures.Should().HaveCount(2);
    }

    private static LoginLockoutOptions CreateOptions(
        int maxFailedAttempts = 5,
        int lockoutDurationMinutes = 15)
    {
        return new LoginLockoutOptions
        {
            MaxFailedAttempts = maxFailedAttempts,
            LockoutDurationMinutes = lockoutDurationMinutes
        };
    }
}
