using FluentAssertions;
using Microsoft.Extensions.Options;
using Pulse.API.Common.Security.RateLimiting;

namespace Pulse.Tests.Unit.Common.Security.RateLimiting;

public class RateLimitRuleOptionsValidatorTests
{
    private readonly RateLimitRuleOptionsValidator _sut = new();

    [Fact]
    public void Validate_WhenAllRequiredFieldsSet_ReturnsSuccess()
    {
        // Arrange
        RateLimitRuleOptions options = new()
        {
            MaxAttempts = 20,
            PeriodMinutes = 15
        };

        // Act
        ValidateOptionsResult result = _sut.Validate(RateLimitSections.Login, options);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WhenMaxAttemptsZeroOrNegative_ReturnsFail(int maxAttempts)
    {
        // Arrange
        RateLimitRuleOptions options = new()
        {
            MaxAttempts = maxAttempts,
            PeriodMinutes = 15
        };

        // Act
        ValidateOptionsResult result = _sut.Validate(RateLimitSections.Login, options);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failures.Should().Contain("RateLimit:Login:MaxAttempts must be greater than zero.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WhenPeriodMinutesZeroOrNegative_ReturnsFail(int periodMinutes)
    {
        // Arrange
        RateLimitRuleOptions options = new()
        {
            MaxAttempts = 20,
            PeriodMinutes = periodMinutes
        };

        // Act
        ValidateOptionsResult result = _sut.Validate(RateLimitSections.Login, options);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failures.Should().Contain("RateLimit:Login:PeriodMinutes must be greater than zero.");
    }

    [Fact]
    public void Validate_WhenMultipleErrorsExist_ReturnsAllErrors()
    {
        // Arrange
        RateLimitRuleOptions options = new()
        {
            MaxAttempts = 0,
            PeriodMinutes = -1
        };

        // Act
        ValidateOptionsResult result = _sut.Validate(RateLimitSections.Login, options);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failures.Should().Contain("RateLimit:Login:MaxAttempts must be greater than zero.");
        result.Failures.Should().Contain("RateLimit:Login:PeriodMinutes must be greater than zero.");
    }
}
