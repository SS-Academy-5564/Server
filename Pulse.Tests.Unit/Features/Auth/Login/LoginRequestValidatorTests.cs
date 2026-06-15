using FluentAssertions;
using FluentValidation;
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
    public async Task Validate_WhenRequestValid_ReturnsNoErrors()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "user@example.com",
            Password = "ValidPassword123"
        };

        // Act
        var result = await _sut.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WhenEmailEmpty_ReturnsError()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = string.Empty,
            Password = "ValidPassword123"
        };

        // Act
        var result = await _sut.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email" && e.ErrorMessage.Contains("required"));
    }

    [Fact]
    public async Task Validate_WhenEmailInvalid_ReturnsError()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "not-an-email",
            Password = "ValidPassword123"
        };

        // Act
        var result = await _sut.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email" && e.ErrorMessage.Contains("valid email"));
    }

    [Fact]
    public async Task Validate_WhenEmailTooLong_ReturnsError()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = new string('a', 250) + "@example.com",
            Password = "ValidPassword123"
        };

        // Act
        var result = await _sut.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email" && e.ErrorMessage.Contains("256"));
    }

    [Fact]
    public async Task Validate_WhenPasswordEmpty_ReturnsError()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "user@example.com",
            Password = string.Empty
        };

        // Act
        var result = await _sut.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password" && e.ErrorMessage.Contains("required"));
    }

    [Fact]
    public async Task Validate_WhenPasswordTooLong_ReturnsError()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "user@example.com",
            Password = new string('a', 257)
        };

        // Act
        var result = await _sut.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password" && e.ErrorMessage.Contains("256"));
    }

    [Fact]
    public async Task Validate_WhenBothFieldsEmpty_ReturnsMultipleErrors()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = string.Empty,
            Password = string.Empty
        };

        // Act
        var result = await _sut.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(2);
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }
}
