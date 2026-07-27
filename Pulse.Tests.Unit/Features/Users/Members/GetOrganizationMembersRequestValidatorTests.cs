using FluentValidation.TestHelper;
using Pulse.API.Features.Users.Members;
using Pulse.BL.Common.Pagination;

namespace Pulse.Tests.Unit.Features.Users.Members;

public class GetOrganizationMembersRequestValidatorTests
{
    private readonly GetOrganizationMembersRequestValidator _validator = new();

    [Fact]
    public void Validate_WhenPaginationIsMissing_ShouldNotHaveValidationErrors()
    {
        TestValidationResult<GetOrganizationMembersRequest> result = _validator.TestValidate(
            new GetOrganizationMembersRequest(null, null));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(PaginationDefaults.MaxPageNumber)]
    public void Validate_WithAllowedPageNumber_ShouldNotHaveValidationError(int pageNumber)
    {
        TestValidationResult<GetOrganizationMembersRequest> result = _validator.TestValidate(
            new GetOrganizationMembersRequest(pageNumber, PaginationDefaults.PageSize));

        result.ShouldNotHaveValidationErrorFor(x => x.PageNumber);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(PaginationDefaults.MaxPageNumber + 1)]
    public void Validate_WithPageNumberOutsideAllowedRange_ShouldHaveValidationError(int pageNumber)
    {
        TestValidationResult<GetOrganizationMembersRequest> result = _validator.TestValidate(
            new GetOrganizationMembersRequest(pageNumber, PaginationDefaults.PageSize));

        result.ShouldHaveValidationErrorFor(x => x.PageNumber)
            .WithErrorMessage($"Page number must be between 1 and {PaginationDefaults.MaxPageNumber}");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(PaginationDefaults.MaxPageSize)]
    public void Validate_WithAllowedPageSize_ShouldNotHaveValidationError(int pageSize)
    {
        TestValidationResult<GetOrganizationMembersRequest> result = _validator.TestValidate(
            new GetOrganizationMembersRequest(PaginationDefaults.PageNumber, pageSize));

        result.ShouldNotHaveValidationErrorFor(x => x.PageSize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(PaginationDefaults.MaxPageSize + 1)]
    public void Validate_WithPageSizeOutsideAllowedRange_ShouldHaveValidationError(int pageSize)
    {
        TestValidationResult<GetOrganizationMembersRequest> result = _validator.TestValidate(
            new GetOrganizationMembersRequest(PaginationDefaults.PageNumber, pageSize));

        result.ShouldHaveValidationErrorFor(x => x.PageSize)
            .WithErrorMessage($"Page size must be between 1 and {PaginationDefaults.MaxPageSize}.");
    }
}
