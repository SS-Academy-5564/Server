using FluentAssertions;
using Microsoft.Extensions.Options;
using Pulse.BL.Common.Security.Tokens;

namespace Pulse.Tests.Unit.Features.Auth.Login;

public class JwtOptionsValidatorTests
{
    private readonly JwtOptionsValidator _sut;

    public JwtOptionsValidatorTests()
    {
        _sut = new JwtOptionsValidator();
    }

    [Fact]
    public void Validate_WhenAllRequiredFieldsSet_ReturnsSuccess()
    {
        // Arrange
        var options = new JwtOptions
        {
            Issuer = "https://pulse.local",
            Audience = "https://pulse.local",
            SecretKey = "super-secret-key-minimum-32-characters-long!",
            ExpirationMinutes = 60
        };

        // Act
        var result = _sut.Validate(null, options);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenIssuerMissing_ReturnsFail()
    {
        // Arrange
        var options = new JwtOptions
        {
            Issuer = string.Empty,
            Audience = "https://pulse.local",
            SecretKey = "super-secret-key-minimum-32-characters-long!",
            ExpirationMinutes = 60
        };

        // Act
        var result = _sut.Validate(null, options);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failures.Should().Contain("Jwt:Issuer is required.");
    }

    [Fact]
    public void Validate_WhenAudienceMissing_ReturnsFail()
    {
        // Arrange
        var options = new JwtOptions
        {
            Issuer = "https://pulse.local",
            Audience = string.Empty,
            SecretKey = "super-secret-key-minimum-32-characters-long!",
            ExpirationMinutes = 60
        };

        // Act
        var result = _sut.Validate(null, options);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failures.Should().Contain("Jwt:Audience is required.");
    }

    [Fact]
    public void Validate_WhenSecretKeyMissing_ReturnsFail()
    {
        // Arrange
        var options = new JwtOptions
        {
            Issuer = "https://pulse.local",
            Audience = "https://pulse.local",
            SecretKey = string.Empty,
            ExpirationMinutes = 60
        };

        // Act
        var result = _sut.Validate(null, options);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failures.Should().Contain("Jwt:SecretKey is required.");
    }

    [Fact]
    public void Validate_WhenSecretKeyTooShort_ReturnsFail()
    {
        // Arrange
        var options = new JwtOptions
        {
            Issuer = "https://pulse.local",
            Audience = "https://pulse.local",
            SecretKey = "short-key",
            ExpirationMinutes = 60
        };

        // Act
        var result = _sut.Validate(null, options);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failures.Should().Contain("Jwt:SecretKey must be at least 32 characters long.");
    }

    [Fact]
    public void Validate_WhenExpirationMinutesZeroOrNegative_ReturnsFail()
    {
        // Arrange
        var options = new JwtOptions
        {
            Issuer = "https://pulse.local",
            Audience = "https://pulse.local",
            SecretKey = "super-secret-key-minimum-32-characters-long!",
            ExpirationMinutes = 0
        };

        // Act
        var result = _sut.Validate(null, options);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failures.Should().Contain("Jwt:ExpirationMinutes must be greater than zero.");
    }

    [Fact]
    public void Validate_WhenMultipleErrorsExist_ReturnsAllErrors()
    {
        // Arrange
        var options = new JwtOptions
        {
            Issuer = string.Empty,
            Audience = string.Empty,
            SecretKey = "short",
            ExpirationMinutes = -1
        };

        // Act
        var result = _sut.Validate(null, options);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failures.Should().Contain("Jwt:Issuer is required.");
        result.Failures.Should().Contain("Jwt:Audience is required.");
        result.Failures.Should().Contain("Jwt:SecretKey must be at least 32 characters long.");
        result.Failures.Should().Contain("Jwt:ExpirationMinutes must be greater than zero.");
    }
}
