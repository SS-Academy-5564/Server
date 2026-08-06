using FluentAssertions;
using Microsoft.Extensions.Options;
using Pulse.BL.Features.Auth.EmailVerification;

namespace Pulse.Tests.Unit.Features.Auth.EmailVerification;

public class EmailVerificationOptionsValidatorTests
{
    private readonly EmailVerificationOptionsValidator _validator = new();

    [Fact]
    public void Validate_ValidHttpsConfiguration_ReturnsSuccess()
    {
        EmailVerificationOptions options = new()
        {
            TokenLifetimeHours = 24,
            VerificationPageUrl = "https://pulse.example.com/verify-email"
        };

        ValidateOptionsResult result = _validator.Validate(null, options);

        result.Should().Be(ValidateOptionsResult.Success);
    }

    [Fact]
    public void Validate_LoopbackHttpConfiguration_ReturnsSuccess()
    {
        EmailVerificationOptions options = new()
        {
            TokenLifetimeHours = 24,
            VerificationPageUrl = "http://localhost:4200/verify-email"
        };

        ValidateOptionsResult result = _validator.Validate(null, options);

        result.Should().Be(ValidateOptionsResult.Success);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(169)]
    public void Validate_InvalidLifetime_ReturnsFailure(int tokenLifetimeHours)
    {
        EmailVerificationOptions options = new()
        {
            TokenLifetimeHours = tokenLifetimeHours,
            VerificationPageUrl = "https://pulse.example.com/verify-email"
        };

        ValidateOptionsResult result = _validator.Validate(null, options);

        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("TokenLifetimeHours");
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("http://pulse.example.com/verify-email")]
    public void Validate_InsecureOrInvalidProductionUrl_ReturnsFailure(string url)
    {
        EmailVerificationOptions options = new()
        {
            TokenLifetimeHours = 24,
            VerificationPageUrl = url
        };

        ValidateOptionsResult result = _validator.Validate(null, options);

        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("VerificationPageUrl");
    }
}
