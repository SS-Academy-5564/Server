using FluentAssertions;
using FluentResults;
using Moq;
using Pulse.BL.Common.Errors;
using Pulse.BL.Common.Security;
using Pulse.BL.Features.Users.Me;
using Pulse.DAL.Queries.Users;

namespace Pulse.Tests.Unit.Features.Users.Me;

public class GetCurrentUserQueryHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IUserQueries> _userQueriesMock;
    private readonly GetCurrentUserQueryHandler _sut;

    public GetCurrentUserQueryHandlerTests()
    {
        _currentUserServiceMock = new();
        _userQueriesMock = new();
        _sut = new GetCurrentUserQueryHandler(_currentUserServiceMock.Object, _userQueriesMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenUserIdIsNull_ReturnsUnauthorized()
    {
        _currentUserServiceMock.Setup(x => x.UserId).Returns((Guid?)null);

        Result<UserProfileResult> result = await _sut.HandleAsync(CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.HasError<UnauthorizedError>().Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ReturnsNotFound()
    {
        _currentUserServiceMock.Setup(x => x.UserId).Returns(Guid.NewGuid());
        _userQueriesMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserProfileRecord?)null);

        Result<UserProfileResult> result = await _sut.HandleAsync(CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.HasError<NotFoundError>().Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WhenUserExists_ReturnsProfile()
    {
        Guid userId = Guid.NewGuid();
        var record = new UserProfileRecord(userId, "user@example.com", "John", "Doe", null, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _userQueriesMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);

        Result<UserProfileResult> result = await _sut.HandleAsync(CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(userId);
        result.Value.Email.Should().Be("user@example.com");
    }
}
