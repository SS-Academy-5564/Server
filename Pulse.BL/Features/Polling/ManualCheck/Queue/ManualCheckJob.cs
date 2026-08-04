namespace Pulse.BL.Features.Polling.ManualCheck.Queue;

public sealed record ManualCheckJob(Guid MonitorId, Guid OrganizationId);
