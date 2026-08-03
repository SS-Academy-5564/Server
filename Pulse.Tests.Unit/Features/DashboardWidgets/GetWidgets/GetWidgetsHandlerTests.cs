using System.Data;
using FluentAssertions;
using FluentResults;
using Moq;
using Pulse.BL.Common.Errors;
using Pulse.BL.Common.Security;
using Pulse.BL.Features.DashboardWidgets.GetWidgets;
using Pulse.DAL.Common.Repository;
using Pulse.DAL.Queries.DashboardWidgets.GetWidgets;

namespace Pulse.Tests.Unit.Features.DashboardWidgets.GetWidgets;

public class GetWidgetsHandlerTests
{
    private readonly Mock<IWidgetQueries> _queriesMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<IUnitOfWorkFactory> _uowFactoryMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();

    private readonly GetWidgetsHandler _sut;

    public GetWidgetsHandlerTests()
    {
        _uowFactoryMock
            .Setup(x => x.CreateAsync(It.IsAny<IsolationLevel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_uowMock.Object);

        _currentUserServiceMock
            .Setup(x => x.OrganizationId)
            .Returns(Guid.Parse("B1000000-0000-0000-0000-000000000001"));

        _sut = new GetWidgetsHandler(
            _queriesMock.Object,
            _currentUserServiceMock.Object,
            _uowFactoryMock.Object);
    }

    private static GetWidgetsQuery ValidQuery()
        => new(Guid.Parse("929ACDAA-A91E-4C60-95F7-E770D2BEDC77"));

    [Fact]
    public async Task HandleAsync_ReturnsWidgets()
    {
        List<WidgetQueryResult> widgets =
        [
            new(
                Guid.NewGuid(),
                Guid.Parse("929ACDAA-A91E-4C60-95F7-E770D2BEDC77"),
                "LineChart",
                "ResponseTime",
                "Last24Hours",
                "{}")
        ];

        _queriesMock
            .Setup(x => x.GetByTabIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(widgets);

        Result<IReadOnlyList<GetWidgetsResult>> result =
            await _sut.HandleAsync(ValidQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].Type.Should().Be("LineChart");
        result.Value[0].Metric.Should().Be("ResponseTime");
        result.Value[0].TimeRange.Should().Be("Last24Hours");
        result.Value[0].Settings.Should().Be("{}");
    }

    [Fact]
    public async Task HandleAsync_CallsQueries()
    {
        _queriesMock
            .Setup(x => x.GetByTabIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        await _sut.HandleAsync(ValidQuery(), CancellationToken.None);

        _queriesMock.Verify(x => x.GetByTabIdAsync(
            Guid.Parse("929ACDAA-A91E-4C60-95F7-E770D2BEDC77"),
            Guid.Parse("B1000000-0000-0000-0000-000000000001"),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_NoOrganizationId_ReturnsUnauthorizedError()
    {
        _currentUserServiceMock
            .Setup(x => x.OrganizationId)
            .Returns((Guid?)null);

        Result<IReadOnlyList<GetWidgetsResult>> result =
            await _sut.HandleAsync(ValidQuery(), CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is UnauthorizedError);

        _queriesMock.Verify(x => x.GetByTabIdAsync(
            It.IsAny<Guid>(),
            It.IsAny<Guid>(),
            It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
