IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Monitors_OrganizationId' AND object_id = OBJECT_ID('dbo.Monitors'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Monitors_OrganizationId
    ON dbo.Monitors (OrganizationId);
END;