using FluentAssertions;
using Pulse.DAL.Queries.Monitors;

namespace Pulse.Tests.Unit.Features.DashboardWidgets.Common;

public class MonitorMetricTests
{
    [Theory]
    [InlineData("availability", MetricType.Availability)]
    [InlineData("Availability", MetricType.Availability)]
    [InlineData("AVAILABILITY", MetricType.Availability)]
    [InlineData("requests", MetricType.Requests)]
    [InlineData("Requests", MetricType.Requests)]
    [InlineData("errors", MetricType.Errors)]
    [InlineData("Errors", MetricType.Errors)]
    [InlineData("responseTime", MetricType.ResponseTime)]
    [InlineData("ResponseTime", MetricType.ResponseTime)]
    [InlineData("unknownMetric", MetricType.ResponseTime)]
    [InlineData("", MetricType.ResponseTime)]
    public void FromWidget_ParsesMetricNameCorrectly(string metricName, MetricType expectedType)
    {
        // Arrange
        Guid monitorId = Guid.NewGuid();
        DateTimeOffset from = DateTimeOffset.UtcNow.AddHours(-1);

        // Act
        MonitorMetric result = MonitorMetric.FromWidget(monitorId, metricName, from);

        // Assert
        result.MonitorId.Should().Be(monitorId);
        result.Metric.Should().Be(expectedType);
        result.From.Should().Be(from);
    }

    [Fact]
    public void StructuralEquality_SameValues_AreEqualAndHaveSameHashCode()
    {
        // Arrange
        Guid monitorId = Guid.NewGuid();
        DateTimeOffset from = DateTimeOffset.UtcNow.AddHours(-24);

        MonitorMetric metric1 = new(monitorId, MetricType.Availability, from);
        MonitorMetric metric2 = new(monitorId, MetricType.Availability, from);

        // Assert
        metric1.Should().Be(metric2);
        metric1.GetHashCode().Should().Be(metric2.GetHashCode());
    }

    [Fact]
    public void StructuralEquality_DifferentValues_AreNotEqual()
    {
        // Arrange
        Guid monitorId1 = Guid.NewGuid();
        Guid monitorId2 = Guid.NewGuid();
        DateTimeOffset from = DateTimeOffset.UtcNow.AddHours(-24);

        MonitorMetric metric1 = new(monitorId1, MetricType.Availability, from);
        MonitorMetric metric2 = new(monitorId2, MetricType.Availability, from);
        MonitorMetric metric3 = new(monitorId1, MetricType.Requests, from);
        MonitorMetric metric4 = new(monitorId1, MetricType.Availability, from.AddHours(-1));

        // Assert
        metric1.Should().NotBe(metric2);
        metric1.Should().NotBe(metric3);
        metric1.Should().NotBe(metric4);
    }
}
