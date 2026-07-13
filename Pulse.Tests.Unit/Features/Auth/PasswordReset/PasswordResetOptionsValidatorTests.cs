using FluentAssertions;
using Microsoft.Extensions.Options;
using Pulse.BL.Features.Auth.PasswordReset;

namespace Pulse.Tests.Unit.Features.Auth.PasswordReset;

public class PasswordResetOptionsValidatorTests
{
    private readonly PasswordResetOptionsValidator _sut;

    public PasswordResetOptionsValidatorTests()
    {
        _sut = new PasswordResetOptionsValidator();
    }

    [Fact]
    public void Validate_WhenOptionsAreValid_ReturnsSuccess()
    {
        // Arrange
        PasswordResetOptions options = new()
        {
            CodeTtlMinutes = 5,
            MaxFailedAttempts = 5,
            ResetTokenLifetimeMinutes = 10,
            ResendCooldownSeconds = 60
        };

        // Act
        ValidateOptionsResult result = _sut.Validate(PasswordResetOptions.SectionName, options);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WhenCodeTtlMinutesIsInvalid_ReturnsFailure(int invalidValue)
    {
        // Arrange
        PasswordResetOptions options = new()
        {
            CodeTtlMinutes = invalidValue,
            MaxFailedAttempts = 5,
            ResetTokenLifetimeMinutes = 10,
            ResendCooldownSeconds = 60
        };

        // Act
        ValidateOptionsResult result = _sut.Validate(PasswordResetOptions.SectionName, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("CodeTtlMinutes");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WhenMaxFailedAttemptsIsInvalid_ReturnsFailure(int invalidValue)
    {
        // Arrange
        PasswordResetOptions options = new()
        {
            CodeTtlMinutes = 5,
            MaxFailedAttempts = invalidValue,
            ResetTokenLifetimeMinutes = 10,
            ResendCooldownSeconds = 60
        };

        // Act
        ValidateOptionsResult result = _sut.Validate(PasswordResetOptions.SectionName, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("MaxFailedAttempts");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WhenResetTokenLifetimeMinutesIsInvalid_ReturnsFailure(int invalidValue)
    {
        // Arrange
        PasswordResetOptions options = new()
        {
            CodeTtlMinutes = 5,
            MaxFailedAttempts = 5,
            ResetTokenLifetimeMinutes = invalidValue,
            ResendCooldownSeconds = 60
        };

        // Act
        ValidateOptionsResult result = _sut.Validate(PasswordResetOptions.SectionName, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("ResetTokenLifetimeMinutes");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WhenResendCooldownSecondsIsInvalid_ReturnsFailure(int invalidValue)
    {
        // Arrange
        PasswordResetOptions options = new()
        {
            CodeTtlMinutes = 5,
            MaxFailedAttempts = 5,
            ResetTokenLifetimeMinutes = 10,
            ResendCooldownSeconds = invalidValue
        };

        // Act
        ValidateOptionsResult result = _sut.Validate(PasswordResetOptions.SectionName, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("ResendCooldownSeconds");
    }
}
