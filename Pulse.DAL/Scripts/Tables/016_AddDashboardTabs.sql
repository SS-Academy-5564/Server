CREATE TABLE DashboardTabs
(
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    OrganizationId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(100) NOT NULL,

    CONSTRAINT FK_DashboardTabs_Organizations
        FOREIGN KEY (OrganizationId)
        REFERENCES Organizations(Id)
);

CREATE INDEX IX_DashboardTabs_OrganizationId
ON DashboardTabs(OrganizationId);
