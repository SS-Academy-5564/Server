IF NOT EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE Name = N'OrganizationId'
      AND Object_ID = Object_ID(N'dbo.Monitors')
)
BEGIN
    ALTER TABLE dbo.Monitors
    ADD OrganizationId UNIQUEIDENTIFIER NOT NULL
    CONSTRAINT DF_Monitors_OrganizationId DEFAULT 'B1000000-0000-0000-0000-000000000001';
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_Monitors_Organization_Id'
)
BEGIN
    ALTER TABLE dbo.Monitors
    ADD CONSTRAINT FK_Monitors_Organization_Id
    FOREIGN KEY (OrganizationId) REFERENCES dbo.Organizations(Id);
END;