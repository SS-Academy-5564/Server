using FluentValidation.TestHelper;
using Pulse.API.Features.Users.Members;
using Pulse.BL.Common.Pagination;

namespace Pulse.Tests.Unit.Features.Users.Members;

/// <summary>
/// Contains unit tests for the <see cref="GetOrganizationMembersRequestValidator"/>.
/// </summary>
public class GetOrganizationMembersRequestValidatorTests
{
    private readonly GetOrganizationMembersRequestValidator _validator = new();

    /// <summary>
    /// Tests that validation succeeds when pagination parameters are missing.
    /// </summary>
    [Fact]
    public void Validate_WhenPaginationIsMissing_ShouldNotHaveValidationErrors()
    {
        TestValidationResult<GetOrganizationMembersRequest> result = _validator.TestValidate(
            new GetOrganizationMembersRequest(null, null));

        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Tests that validation succeeds for valid page numbers.
    /// </summary>
    /// <param name="pageNumber">The allowed page number.</param>
    [Theory]
    [InlineData(1)]
    [InlineData(PaginationDefaults.MaxPageNumber)]
    public void Validate_WithAllowedPageNumber_ShouldNotHaveValidationError(int pageNumber)
    {
        TestValidationResult<GetOrganizationMembersRequest> result = _validator.TestValidate(
            new GetOrganizationMembersRequest(pageNumber, PaginationDefaults.PageSize));

        result.ShouldNotHaveValidationErrorFor(x => x.PageNumber);
    }

    /// <summary>
    /// Tests that validation fails when the page number is outside the allowed range.
    /// </summary>
    /// <param name="pageNumber">The invalid page number.</param>
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

    /// <summary>
    /// Tests that validation succeeds for valid page sizes.
    /// </summary>
    /// <param name="pageSize">The allowed page size.</param>
    [Theory]
    [InlineData(1)]
    [InlineData(PaginationDefaults.MaxPageSize)]
    public void Validate_WithAllowedPageSize_ShouldNotHaveValidationError(int pageSize)
    {
        TestValidationResult<GetOrganizationMembersRequest> result = _validator.TestValidate(
            new GetOrganizationMembersRequest(PaginationDefaults.PageNumber, pageSize));

        result.ShouldNotHaveValidationErrorFor(x => x.PageSize);
    }

    /// <summary>
    /// Tests that validation fails when the page size is outside the allowed range.
    /// </summary>
    /// <param name="pageSize">The invalid page size.</param>
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
