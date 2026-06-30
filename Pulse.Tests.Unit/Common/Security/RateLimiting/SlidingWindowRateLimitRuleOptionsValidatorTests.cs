using FluentAssertions;
using Microsoft.Extensions.Options;
using Pulse.API.Common.Security.RateLimiting;

namespace Pulse.Tests.Unit.Common.Security.RateLimiting;

public class SlidingWindowRateLimitRuleOptionsValidatorTests
{
    private readonly SlidingWindowRateLimitRuleOptionsValidator _sut = new();

    [Fact]
    public void Validate_WhenAllRequiredFieldsSet_ReturnsSuccess()
    {
        // Arrange
        SlidingWindowRateLimitRuleOptions options = new()
        {
            MaxAttempts = 20,
            PeriodMinutes = 15,
            Segments = 10
        };

        // Act
        ValidateOptionsResult result = _sut.Validate(RateLimitSections.PasswordReset, options);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WhenMaxAttemptsZeroOrNegative_ReturnsFail(int maxAttempts)
    {
        // Arrange
        SlidingWindowRateLimitRuleOptions options = new()
        {
            MaxAttempts = maxAttempts,
            PeriodMinutes = 15,
            Segments = 10
        };

        // Act
        ValidateOptionsResult result = _sut.Validate(RateLimitSections.PasswordReset, options);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failures.Should().Contain("RateLimit:PasswordReset:MaxAttempts must be greater than zero.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WhenPeriodMinutesZeroOrNegative_ReturnsFail(int periodMinutes)
    {
        // Arrange
        SlidingWindowRateLimitRuleOptions options = new()
        {
            MaxAttempts = 20,
            PeriodMinutes = periodMinutes,
            Segments = 10
        };

        // Act
        ValidateOptionsResult result = _sut.Validate(RateLimitSections.PasswordReset, options);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failures.Should().Contain("RateLimit:PasswordReset:PeriodMinutes must be greater than zero.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WhenSegmentsZeroOrNegative_ReturnsFail(int segments)
    {
        // Arrange
        SlidingWindowRateLimitRuleOptions options = new()
        {
            MaxAttempts = 20,
            PeriodMinutes = 15,
            Segments = segments
        };

        // Act
        ValidateOptionsResult result = _sut.Validate(RateLimitSections.PasswordReset, options);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failures.Should().Contain("RateLimit:PasswordReset:Segments must be greater than zero.");
    }

    [Fact]
    public void Validate_WhenMultipleErrorsExist_ReturnsAllErrors()
    {
        // Arrange
        SlidingWindowRateLimitRuleOptions options = new()
        {
            MaxAttempts = 0,
            PeriodMinutes = -1,
            Segments = -1
        };

        // Act
        ValidateOptionsResult result = _sut.Validate(RateLimitSections.PasswordReset, options);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failures.Should().Contain("RateLimit:PasswordReset:MaxAttempts must be greater than zero.");
        result.Failures.Should().Contain("RateLimit:PasswordReset:PeriodMinutes must be greater than zero.");
        result.Failures.Should().Contain("RateLimit:PasswordReset:Segments must be greater than zero.");
    }
}
