IF OBJECT_ID(N'dbo.MonitorStatuses', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.MonitorStatuses
        (
            Id   UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
            Name NVARCHAR(20)     NOT NULL,

            CONSTRAINT PK_MonitorStatuses_Id PRIMARY KEY (Id),
            CONSTRAINT UQ_MonitorStatuses_Name UNIQUE (Name)
        );
    END;
