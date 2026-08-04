using FluentAssertions;
using Pulse.API.Features.Auth.EmailVerification;

namespace Pulse.Tests.Unit.Features.Auth.EmailVerification;

public class VerifyEmailRequestValidatorTests
{
    private readonly VerifyEmailRequestValidator _validator = new();

    [Fact]
    public void Validate_TokenProvided_ReturnsValid()
    {
        var request = new VerifyEmailRequest("verification-token");

        FluentValidation.Results.ValidationResult result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_TokenMissing_ReturnsInvalid(string? token)
    {
        var request = new VerifyEmailRequest(token!);

        FluentValidation.Results.ValidationResult result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(VerifyEmailRequest.Token));
    }
}
