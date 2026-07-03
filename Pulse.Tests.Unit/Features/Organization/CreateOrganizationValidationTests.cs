using FluentValidation.TestHelper;
using Pulse.API.Features.Organization;

namespace Pulse.Tests.Unit.Features.Organization;

public class CreateOrganizationValidationTests
{
    private readonly CreateOrganizationRequestValidator _validator = new();

    // Name
    [Fact]
    public void Should_fail_when_name_is_empty()
    {
        CreateOrganizationRequest model = new() { Name = "" };

        TestValidationResult<CreateOrganizationRequest> result =
            _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Organization name is required");
    }

    [Fact]
    public void Should_fail_when_name_is_too_short()
    {
        CreateOrganizationRequest model = new() { Name = "ab" };

        TestValidationResult<CreateOrganizationRequest> result =
           _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Min length is 3");
    }

    [Fact]
    public void Should_fail_when_name_is_too_long()
    {
        CreateOrganizationRequest model = new() { Name = new string('a', 51) };

        TestValidationResult<CreateOrganizationRequest> result =
            _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Max length is 50");
    }

    [Fact]
    public void Should_pass_when_name_is_valid()
    {
        CreateOrganizationRequest model = new() { Name = "Valid Org" };

        TestValidationResult<CreateOrganizationRequest> result =
            _validator.TestValidate(model);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
