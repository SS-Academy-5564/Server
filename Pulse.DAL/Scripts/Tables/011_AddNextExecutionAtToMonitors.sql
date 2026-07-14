IF COL_LENGTH(N'dbo.Monitors', N'NextExecutionAt') IS NULL
    BEGIN
        ALTER TABLE dbo.Monitors
            ADD NextExecutionAt DATETIME2 NOT NULL
                CONSTRAINT DF_Monitors_NextExecutionAt DEFAULT SYSUTCDATETIME();
    END;
