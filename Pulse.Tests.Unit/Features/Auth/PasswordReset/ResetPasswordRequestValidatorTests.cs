using FluentAssertions;
using FluentValidation.Results;
using Pulse.API.Features.Auth.PasswordReset;

namespace Pulse.Tests.Unit.Features.Auth.PasswordReset;

public class ResetPasswordRequestValidatorTests
{
    private readonly ResetPasswordRequestValidator _sut;

    public ResetPasswordRequestValidatorTests()
    {
        _sut = new ResetPasswordRequestValidator();
    }

    [Fact]
    public async Task Validate_WhenRequestIsValid_ReturnsNoErrorsAsync()
    {
        // Arrange
        ResetPasswordRequest request = new("valid-token", "ValidPassword123", "ValidPassword123");

        // Act
        ValidationResult result = await _sut.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WhenResetTokenIsEmpty_ReturnsErrorAsync()
    {
        // Arrange
        ResetPasswordRequest request = new("", "ValidPassword123", "ValidPassword123");

        // Act
        ValidationResult result = await _sut.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ResetToken" && e.ErrorMessage.Contains("required"));
    }

    [Fact]
    public async Task Validate_WhenNewPasswordIsTooShort_ReturnsErrorAsync()
    {
        // Arrange
        ResetPasswordRequest request = new("valid-token", "Short1!", "Short1!");

        // Act
        ValidationResult result = await _sut.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewPassword" && e.ErrorMessage.Contains("8 characters"));
    }

    [Fact]
    public async Task Validate_WhenPasswordsDoNotMatch_ReturnsErrorAsync()
    {
        // Arrange
        ResetPasswordRequest request = new("valid-token", "ValidPassword123", "DifferentPassword123");

        // Act
        ValidationResult result = await _sut.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ConfirmPassword" && e.ErrorMessage.Contains("match"));
    }
}
