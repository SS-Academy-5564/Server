using FluentAssertions;
using Microsoft.Extensions.Options;
using Pulse.BL.Features.Email;

namespace Pulse.Tests.Unit.Features.Email;

public class EmailOptionsValidatorTests
{
    private readonly EmailOptionsValidator _validator = new();

    [Fact]
    public void Validate_WhenDummyProviderWithValidOptions_ReturnsSuccess()
    {
        // Arrange
        var options = new EmailOptions
        {
            Provider = EmailProvider.Dummy,
            FromAddress = "noreply@pulse.com",
            FromName = "Pulse"
        };

        // Act
        ValidateOptionsResult result = _validator.Validate(null, options);

        // Assert
        result.Should().Be(ValidateOptionsResult.Success);
    }

    [Fact]
    public void Validate_WhenResendProviderWithApiKey_ReturnsSuccess()
    {
        // Arrange
        var options = new EmailOptions
        {
            Provider = EmailProvider.Resend,
            ApiKey = "re_test_key",
            FromAddress = "noreply@pulse.com",
            FromName = "Pulse"
        };

        // Act
        ValidateOptionsResult result = _validator.Validate(null, options);

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
        ValidateOptionsResult result = _validator.Validate(null, options);

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
            Provider = (EmailProvider)999,
            FromAddress = "noreply@pulse.com",
            FromName = "Pulse"
        };

        // Act
        ValidateOptionsResult result = _validator.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("Email:Provider must be a valid provider.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Validate_WhenFromAddressInvalid_ReturnsFail(string fromAddress)
    {
        // Arrange
        var options = new EmailOptions
        {
            Provider = EmailProvider.Dummy,
            FromAddress = fromAddress,
            FromName = "Pulse"
        };

        // Act
        ValidateOptionsResult result = _validator.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenFromNameMissing_ReturnsFail()
    {
        // Arrange
        var options = new EmailOptions
        {
            Provider = EmailProvider.Dummy,
            FromAddress = "noreply@pulse.com",
            FromName = ""
        };

        // Act
        ValidateOptionsResult result = _validator.Validate(null, options);

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
            Provider = EmailProvider.Resend,
            FromAddress = "noreply@pulse.com",
            FromName = "Pulse"
        };

        // Act
        ValidateOptionsResult result = _validator.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("Email:ApiKey is required when Email:Provider is 'Resend'.");
    }
}
