using FluentValidation.TestHelper;
using Pulse.API.Features.Organization;

namespace Pulse.Tests.Unit.Features.Organization;

public class CreateOrganizationValidationTests
{
    private readonly CreateOrganizationRequestValidator _validator = new();

    // Name
    [Fact]
    public void CreateOrganizationRequestValidator_EmptyName_ShouldHaveValidationError()
    {
        CreateOrganizationRequest model = new() { Name = "" };

        TestValidationResult<CreateOrganizationRequest> result =
            _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Organization name is required");
    }

    [Fact]
    public void CreateOrganizationRequestValidator_NameTooShort_ShouldHaveValidationError()
    {
        CreateOrganizationRequest model = new() { Name = "ab" };

        TestValidationResult<CreateOrganizationRequest> result =
           _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Min length is 3");
    }

    [Fact]
    public void CreateOrganizationRequestValidator_NameTooLong_ShouldHaveValidationError()
    {
        CreateOrganizationRequest model = new() { Name = new string('a', 51) };

        TestValidationResult<CreateOrganizationRequest> result =
            _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Max length is 50");
    }

    [Fact]
    public void CreateOrganizationRequestValidator_ValidName_ShouldNotHaveValidationErrors()
    {
        CreateOrganizationRequest model = new() { Name = "Valid Org" };

        TestValidationResult<CreateOrganizationRequest> result =
            _validator.TestValidate(model);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
