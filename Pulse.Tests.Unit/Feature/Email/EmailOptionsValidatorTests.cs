using FluentAssertions;
using Microsoft.Extensions.Options;
using Pulse.BL.Feature.Email;

namespace Pulse.Tests.Unit.Feature.Email;

public class EmailOptionsValidatorTests
{
    private readonly EmailOptionsValidator _validator = new();

    [Fact]
    public void Validate_WhenDummyProviderWithValidOptions_ReturnsSuccess()
    {
        // Arrange
        var options = new EmailOptions
        {
            Provider = "dummy",
            FromAddress = "noreply@pulse.com",
            FromName = "Pulse"
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        result.Should().Be(ValidateOptionsResult.Success);
    }

    [Fact]
    public void Validate_WhenResendProviderWithApiKey_ReturnsSuccess()
    {
        // Arrange
        var options = new EmailOptions
        {
            Provider = "resend",
            ApiKey = "re_test_key",
            FromAddress = "noreply@pulse.com",
            FromName = "Pulse"
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        result.Should().Be(ValidateOptionsResult.Success);
    }

    [Fact]
    public void Validate_WhenProviderMissing_ReturnsFail()
    {
        // Arrange
        var options = new EmailOptions
        {
            FromAddress = "noreply@pulse.com",
            FromName = "Pulse"
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("Email:Provider is required.");
    }

    [Fact]
    public void Validate_WhenProviderInvalid_ReturnsFail()
    {
        // Arrange
        var options = new EmailOptions
        {
            Provider = "smtp",
            FromAddress = "noreply@pulse.com",
            FromName = "Pulse"
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("Email:Provider must be either 'dummy' or 'resend'.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Validate_WhenFromAddressInvalid_ReturnsFail(string fromAddress)
    {
        // Arrange
        var options = new EmailOptions
        {
            Provider = "dummy",
            FromAddress = fromAddress,
            FromName = "Pulse"
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenFromNameMissing_ReturnsFail()
    {
        // Arrange
        var options = new EmailOptions
        {
            Provider = "dummy",
            FromAddress = "noreply@pulse.com",
            FromName = ""
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("Email:FromName is required.");
    }

    [Fact]
    public void Validate_WhenResendProviderWithoutApiKey_ReturnsFail()
    {
        // Arrange
        var options = new EmailOptions
        {
            Provider = "resend",
            FromAddress = "noreply@pulse.com",
            FromName = "Pulse"
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("Email:ApiKey is required when Email:Provider is 'resend'.");
    }
}
