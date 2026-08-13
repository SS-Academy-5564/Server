IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Monitors') AND name = 'CreatedAt')
    BEGIN
        ALTER TABLE dbo.Monitors
            ADD CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME();
    END;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Monitors') AND name = 'LastModifiedAt')
    BEGIN
        ALTER TABLE dbo.Monitors
            ADD LastModifiedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME();
    END;
