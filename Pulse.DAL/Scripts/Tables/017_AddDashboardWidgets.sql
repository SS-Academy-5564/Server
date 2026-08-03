CREATE TABLE DashboardWidgets
(
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,

    DashboardTabId UNIQUEIDENTIFIER NOT NULL,
    OrganizationId UNIQUEIDENTIFIER NOT NULL,

    Type NVARCHAR(50) NOT NULL,
    Metric NVARCHAR(100) NOT NULL,
    TimeRange NVARCHAR(50) NOT NULL,

    Settings NVARCHAR(MAX) NULL,

    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT FK_DashboardWidgets_Tabs
        FOREIGN KEY (DashboardTabId)
        REFERENCES DashboardTabs(Id)
        ON DELETE CASCADE,

    CONSTRAINT FK_DashboardWidgets_Organizations
        FOREIGN KEY (OrganizationId)
        REFERENCES Organizations(Id)
);
