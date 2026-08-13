ALTER TABLE DashboardWidgets
    ADD MonitorId UNIQUEIDENTIFIER NULL;
GO

UPDATE w
SET w.MonitorId = m.Id
FROM DashboardWidgets w
CROSS APPLY (
    SELECT TOP 1 Id
    FROM Monitors
    WHERE OrganizationId = w.OrganizationId
    ORDER BY CreatedAt, Id
) m
WHERE w.MonitorId IS NULL;
GO

ALTER TABLE DashboardWidgets
    ALTER COLUMN MonitorId UNIQUEIDENTIFIER NOT NULL;
GO

ALTER TABLE DashboardWidgets
    ADD CONSTRAINT FK_DashboardWidgets_Monitors
        FOREIGN KEY (MonitorId) REFERENCES Monitors(Id);

