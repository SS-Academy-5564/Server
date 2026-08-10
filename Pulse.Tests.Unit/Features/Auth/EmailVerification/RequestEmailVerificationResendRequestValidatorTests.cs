using FluentAssertions;
using FluentValidation.Results;
using Pulse.API.Features.Auth.EmailVerification;

namespace Pulse.Tests.Unit.Features.Auth.EmailVerification;

public class RequestEmailVerificationResendRequestValidatorTests
{
    private readonly RequestEmailVerificationResendRequestValidator _validator = new();

    [Fact]
    public void Validate_ValidEmail_ReturnsValidResult()
    {
        ValidationResult result = _validator.Validate(
            new RequestEmailVerificationResendRequest("user@example.com"));

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Validate_InvalidEmail_ReturnsValidationError(string email)
    {
        ValidationResult result = _validator.Validate(new RequestEmailVerificationResendRequest(email));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error =>
            error.PropertyName == nameof(RequestEmailVerificationResendRequest.Email));
    }
}
