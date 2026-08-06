using FluentAssertions;
using Pulse.API.Features.Auth.EmailVerification;

namespace Pulse.Tests.Unit.Features.Auth.EmailVerification;

public class ResendEmailVerificationRequestValidatorTests
{
    private readonly ResendEmailVerificationRequestValidator _validator = new();

    [Fact]
    public void Validate_TokenProvided_ReturnsValid()
    {
        ResendEmailVerificationRequest request = new("expired-token");

        FluentValidation.Results.ValidationResult result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_TokenMissing_ReturnsInvalid(string? token)
    {
        ResendEmailVerificationRequest request = new(token!);

        FluentValidation.Results.ValidationResult result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error =>
            error.PropertyName == nameof(ResendEmailVerificationRequest.Token));
    }
}
