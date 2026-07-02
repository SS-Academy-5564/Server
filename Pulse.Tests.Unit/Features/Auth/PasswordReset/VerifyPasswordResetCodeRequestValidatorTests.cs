using FluentAssertions;
using FluentValidation.Results;
using Pulse.API.Features.Auth.PasswordReset;

namespace Pulse.Tests.Unit.Features.Auth.PasswordReset;

public class VerifyPasswordResetCodeRequestValidatorTests
{
    private readonly VerifyPasswordResetCodeRequestValidator _sut;

    public VerifyPasswordResetCodeRequestValidatorTests()
    {
        _sut = new VerifyPasswordResetCodeRequestValidator();
    }

    [Fact]
    public async Task Validate_WhenRequestIsValid_ReturnsNoErrorsAsync()
    {
        // Arrange
        VerifyPasswordResetCodeRequest request = new("test@example.com", "123456");

        // Act
        ValidationResult result = await _sut.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WhenEmailIsInvalid_ReturnsErrorAsync()
    {
        // Arrange
        VerifyPasswordResetCodeRequest request = new("invalid-email", "123456");

        // Act
        ValidationResult result = await _sut.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email" && e.ErrorMessage.Contains("valid email"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("12345")]
    [InlineData("1234567")]
    [InlineData("abcdef")]
    public async Task Validate_WhenCodeIsInvalid_ReturnsErrorAsync(string invalidCode)
    {
        // Arrange
        VerifyPasswordResetCodeRequest request = new("test@example.com", invalidCode);

        // Act
        ValidationResult result = await _sut.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Code");
    }
}
