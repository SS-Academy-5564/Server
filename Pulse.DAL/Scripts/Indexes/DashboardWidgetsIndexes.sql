CREATE INDEX IX_DashboardWidgets_DashboardTabId
ON DashboardWidgets(DashboardTabId);


CREATE INDEX IX_DashboardWidgets_OrganizationId
ON DashboardWidgets(OrganizationId);


CREATE INDEX IX_DashboardWidgets_OrganizationId_DashboardTabId
ON DashboardWidgets(OrganizationId, DashboardTabId);
