using FluentAssertions;
using FluentValidation.Results;
using Pulse.API.Features.Auth.Login;

namespace Pulse.Tests.Unit.Features.Auth.Login;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _sut;

    public LoginRequestValidatorTests()
    {
        _sut = new LoginRequestValidator();
    }

    [Fact]
    public async Task Validate_WhenRequestValid_ReturnsNoErrorsAsync()
    {
        // Arrange
        LoginRequest request = new("user@example.com", "ValidPassword123");

        // Act
        ValidationResult result = await _sut.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WhenEmailEmpty_ReturnsErrorAsync()
    {
        // Arrange
        LoginRequest request = new(string.Empty, "ValidPassword123");

        // Act
        ValidationResult result = await _sut.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email" && e.ErrorMessage.Contains("required"));
    }

    [Fact]
    public async Task Validate_WhenEmailInvalid_ReturnsErrorAsync()
    {
        // Arrange
        LoginRequest request = new("not-an-email", "ValidPassword123");

        // Act
        ValidationResult result = await _sut.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email" && e.ErrorMessage.Contains("valid email"));
    }

    [Fact]
    public async Task Validate_WhenEmailTooLong_ReturnsErrorAsync()
    {
        // Arrange
        LoginRequest request = new(new string('a', 250) + "@example.com", "ValidPassword123");

        // Act
        ValidationResult result = await _sut.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email" && e.ErrorMessage.Contains("256"));
    }

    [Fact]
    public async Task Validate_WhenPasswordEmpty_ReturnsErrorAsync()
    {
        // Arrange
        LoginRequest request = new("user@example.com", string.Empty);

        // Act
        ValidationResult result = await _sut.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password" && e.ErrorMessage.Contains("required"));
    }

    [Fact]
    public async Task Validate_WhenPasswordTooLong_ReturnsErrorAsync()
    {
        // Arrange
        LoginRequest request = new("user@example.com", new string('a', 257));

        // Act
        ValidationResult result = await _sut.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password" && e.ErrorMessage.Contains("256"));
    }

    [Fact]
    public async Task Validate_WhenBothFieldsEmpty_ReturnsMultipleErrorsAsync()
    {
        // Arrange
        LoginRequest request = new(string.Empty, string.Empty);

        // Act
        ValidationResult result = await _sut.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(2);
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }
}
