using FluentAssertions;
using FluentValidation.Results;
using Pulse.API.Features.Auth.PasswordReset;

namespace Pulse.Tests.Unit.Features.Auth.PasswordReset;

public class RequestPasswordResetRequestValidatorTests
{
    private readonly RequestPasswordResetRequestValidator _sut;

    public RequestPasswordResetRequestValidatorTests()
    {
        _sut = new RequestPasswordResetRequestValidator();
    }

    [Fact]
    public async Task Validate_WhenEmailIsValid_ReturnsNoErrorsAsync()
    {
        // Arrange
        RequestPasswordResetRequest request = new("test@example.com");

        // Act
        ValidationResult result = await _sut.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WhenEmailIsEmpty_ReturnsErrorAsync()
    {
        // Arrange
        RequestPasswordResetRequest request = new(string.Empty);

        // Act
        ValidationResult result = await _sut.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email" && e.ErrorMessage.Contains("required"));
    }

    [Fact]
    public async Task Validate_WhenEmailIsInvalid_ReturnsErrorAsync()
    {
        // Arrange
        RequestPasswordResetRequest request = new("invalid-email");

        // Act
        ValidationResult result = await _sut.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email" && e.ErrorMessage.Contains("valid email"));
    }
}
