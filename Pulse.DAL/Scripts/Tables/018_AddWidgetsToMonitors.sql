ALTER TABLE DashboardWidgets
    ADD MonitorId UNIQUEIDENTIFIER NOT NULL;

ALTER TABLE DashboardWidgets
    ADD CONSTRAINT FK_DashboardWidgets_Monitors
        FOREIGN KEY (MonitorId) REFERENCES Monitors(Id);
