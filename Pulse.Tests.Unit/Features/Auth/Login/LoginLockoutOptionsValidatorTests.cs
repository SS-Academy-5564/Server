using FluentAssertions;
using Microsoft.Extensions.Options;
using Pulse.BL.Features.Auth.Login.LoginLockout;

namespace Pulse.Tests.Unit.Features.Auth.Login;

public class LoginLockoutOptionsValidatorTests
{
    private readonly LoginLockoutOptionsValidator _sut = new();

    [Fact]
    public void Validate_WhenAllRequiredFieldsSet_ReturnsSuccess()
    {
        // Arrange
        LoginLockoutOptions options = new()
        {
            MaxFailedAttempts = 5,
            LockoutDurationMinutes = 15
        };

        // Act
        ValidateOptionsResult result = _sut.Validate(null, options);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WhenMaxFailedAttemptsZeroOrNegative_ReturnsFail(
        int maxFailedAttempts)
    {
        // Arrange
        LoginLockoutOptions options = new()
        {
            MaxFailedAttempts = maxFailedAttempts,
            LockoutDurationMinutes = 15
        };

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
    public void Validate_WhenLockoutDurationMinutesZeroOrNegative_ReturnsFail(
        int lockoutDurationMinutes)
    {
        // Arrange
        LoginLockoutOptions options = new()
        {
            MaxFailedAttempts = 5,
            LockoutDurationMinutes = lockoutDurationMinutes
        };

        // Act
        ValidateOptionsResult result = _sut.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain(
            $"{LoginLockoutOptions.SectionName}:LockoutDurationMinutes must be greater than zero.");
    }

    [Fact]
    public void Validate_WhenMultipleErrorsExist_ReturnsAllErrors()
    {
        // Arrange
        LoginLockoutOptions options = new()
        {
            MaxFailedAttempts = 0,
            LockoutDurationMinutes = 0
        };

        // Act
        ValidateOptionsResult result = _sut.Validate(null, options);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failures.Should().Contain(
            $"{LoginLockoutOptions.SectionName}:MaxFailedAttempts must be greater than zero.");
        result.Failures.Should().Contain(
            $"{LoginLockoutOptions.SectionName}:LockoutDurationMinutes must be greater than zero.");
    }
}
