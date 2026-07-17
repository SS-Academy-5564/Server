using FluentAssertions;
using Microsoft.Extensions.Options;
using Pulse.BL.Common.Security.Ssrf;

namespace Pulse.Tests.Unit.Features.Security.Ssrf;

public class SsrfProtectionOptionsValidatorTests
{
    private readonly SsrfProtectionOptionsValidator _validator = new();

    [Fact]
    public void Validate_WhenCidrsAreValid_ReturnsSuccess()
    {
        SsrfProtectionOptions options = new()
        {
            AllowedCidrs = ["10.0.0.0/8", "192.168.1.5"],
            BlockedCidrs = ["203.0.113.0/24", "fc00::/7"]
        };

        ValidateOptionsResult result = _validator.Validate(null, options);

        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenAllowedCidrIsInvalid_ReturnsFailure()
    {
        SsrfProtectionOptions options = new()
        {
            AllowedCidrs = ["not-a-cidr"]
        };

        ValidateOptionsResult result = _validator.Validate(null, options);

        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain(f => f.Contains("AllowedCidrs") && f.Contains("not-a-cidr"));
    }

    [Theory]
    [InlineData("10.0.0.0/33")]
    [InlineData("300.0.0.0/8")]
    [InlineData("garbage")]
    public void Validate_WhenBlockedCidrIsInvalid_ReturnsFailure(string cidr)
    {
        SsrfProtectionOptions options = new()
        {
            BlockedCidrs = [cidr]
        };

        ValidateOptionsResult result = _validator.Validate(null, options);

        result.Failed.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenCidrsAreEmpty_ReturnsSuccess()
    {
        ValidateOptionsResult result = _validator.Validate(null, new SsrfProtectionOptions());

        result.Succeeded.Should().BeTrue();
    }
}
