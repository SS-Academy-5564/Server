using FluentValidation.TestHelper;
using Pulse.API.Feature.Auth.Registration;

namespace Pulse.Tests.Unit.Feature.Auth.Registration;

public class RegistrationRequestValidatorTests
{
    private readonly RegistrationRequestValidator _validator = new();

    [Fact]
    public void Validate_ValidRequest_PassesWithNoErrors()
    {
        var result = _validator.TestValidate(ValidRequest());

        result.ShouldNotHaveAnyValidationErrors();
    }

    // Email
    [Fact]
    public void Validate_EmptyEmail_FailsWithRequiredMessage()
    {
        var result = _validator.TestValidate(ValidRequest(email: ""));

        result.ShouldHaveValidationErrorFor(r => r.Email)
            .WithErrorMessage("Email is required.");
    }

    [Fact]
    public void Validate_InvalidEmailFormat_FailsWithFormatMessage()
    {
        var result = _validator.TestValidate(ValidRequest(email: "not-an-email"));

        result.ShouldHaveValidationErrorFor(r => r.Email)
            .WithErrorMessage("Invalid email format.");
    }

    [Fact]
    public void Validate_EmailExceeds256Chars_FailsWithLengthMessage()
    {
        var result = _validator.TestValidate(ValidRequest(email: new string('a', 251) + "@b.com"));

        result.ShouldHaveValidationErrorFor(r => r.Email)
            .WithErrorMessage("Email must not exceed 256 characters.");
    }

    // FirstName
    [Fact]
    public void Validate_EmptyFirstName_FailsWithRequiredMessage()
    {
        var result = _validator.TestValidate(ValidRequest(firstName: ""));

        result.ShouldHaveValidationErrorFor(r => r.FirstName)
            .WithErrorMessage("First name is required.");
    }

    [Fact]
    public void Validate_FirstNameExceeds100Chars_FailsWithLengthMessage()
    {
        var result = _validator.TestValidate(ValidRequest(firstName: new string('a', 101)));

        result.ShouldHaveValidationErrorFor(r => r.FirstName)
            .WithErrorMessage("First name must not exceed 100 characters.");
    }

    // LastName
    [Fact]
    public void Validate_EmptyLastName_FailsWithRequiredMessage()
    {
        var result = _validator.TestValidate(ValidRequest(lastName: ""));

        result.ShouldHaveValidationErrorFor(r => r.LastName)
            .WithErrorMessage("Last name is required.");
    }

    [Fact]
    public void Validate_LastNameExceeds100Chars_FailsWithLengthMessage()
    {
        var result = _validator.TestValidate(ValidRequest(lastName: new string('a', 101)));

        result.ShouldHaveValidationErrorFor(r => r.LastName)
            .WithErrorMessage("Last name must not exceed 100 characters.");
    }

    // Password
    [Fact]
    public void Validate_EmptyPassword_FailsWithRequiredMessage()
    {
        var result = _validator.TestValidate(ValidRequest(password: ""));

        result.ShouldHaveValidationErrorFor(r => r.Password)
            .WithErrorMessage("Password is required.");
    }

    [Fact]
    public void Validate_PasswordUnder8Chars_FailsWithMinLengthMessage()
    {
        var result = _validator.TestValidate(ValidRequest(password: "Ab1"));

        result.ShouldHaveValidationErrorFor(r => r.Password)
            .WithErrorMessage("Password must be at least 8 characters.");
    }

    [Fact]
    public void Validate_PasswordExceeds256Chars_FailsWithMaxLengthMessage()
    {
        var result = _validator.TestValidate(ValidRequest(password: "Aa1" + new string('x', 254)));

        result.ShouldHaveValidationErrorFor(r => r.Password)
            .WithErrorMessage("Password must not exceed 256 characters.");
    }

    [Fact]
    public void Validate_PasswordMissingUppercase_FailsWithUppercaseMessage()
    {
        var result = _validator.TestValidate(ValidRequest(password: "lowercase1"));

        result.ShouldHaveValidationErrorFor(r => r.Password)
            .WithErrorMessage("Password must contain at least one uppercase letter.");
    }

    [Fact]
    public void Validate_PasswordMissingLowercase_FailsWithLowercaseMessage()
    {
        var result = _validator.TestValidate(ValidRequest(password: "UPPERCASE1"));

        result.ShouldHaveValidationErrorFor(r => r.Password)
            .WithErrorMessage("Password must contain at least one lowercase letter.");
    }

    [Fact]
    public void Validate_PasswordMissingDigit_FailsWithDigitMessage()
    {
        var result = _validator.TestValidate(ValidRequest(password: "NoDigitsHere"));

        result.ShouldHaveValidationErrorFor(r => r.Password)
            .WithErrorMessage("Password must contain at least one digit.");
    }

    private static RegistrationRequest ValidRequest(
        string email = "john.doe@example.com",
        string firstName = "John",
        string lastName = "Doe",
        string password = "SecurePass1") => new()
        {
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Password = password
        };
}
