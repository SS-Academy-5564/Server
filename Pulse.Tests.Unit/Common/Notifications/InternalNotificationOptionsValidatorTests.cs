using FluentAssertions;
using Microsoft.Extensions.Options;
using Pulse.API.Filters.InternalNotificatiom;

namespace Pulse.Tests.Unit.Common.Notifications;

public sealed class InternalNotificationOptionsValidatorTests
{
    private readonly InternalNotificationOptionsValidator _validator = new();

    [Fact]
    public void Validate_WithApiKey_ReturnsSuccess()
    {
        InternalNotificationOptions options = new()
        {
            ApiKey = "internal-api-key"
        };

        ValidateOptionsResult result = _validator.Validate(null, options);

        result.Should().Be(ValidateOptionsResult.Success);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_WithoutApiKey_ReturnsFailure(string apiKey)
    {
        InternalNotificationOptions options = new()
        {
            ApiKey = apiKey
        };

        ValidateOptionsResult result = _validator.Validate(null, options);

        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("InternalNotifications:ApiKey is required.");
    }
}
