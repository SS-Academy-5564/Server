IF OBJECT_ID(N'dbo.Monitors', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Monitors
        (
            Id                     UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
            Name                   VARCHAR(64)      NOT NULL,
            Url                    NVARCHAR(2083)   NOT NULL,
            CurrentValue           NVARCHAR(MAX),
            LastCheckedAt          DATETIMEOFFSET,
            StatusId               UNIQUEIDENTIFIER NOT NULL,
            PollingIntervalSeconds INT              NOT NULL,
            PollingTimeoutSeconds  INT              NOT NULL,

            CONSTRAINT PK_Monitors_Id PRIMARY KEY (Id),
            CONSTRAINT FK_Monitors_Status_Id FOREIGN KEY (StatusId) REFERENCES dbo.MonitorStatuses (Id),
            CONSTRAINT CK_Monitors_PollingIntervalSeconds CHECK (PollingIntervalSeconds >= 60 AND PollingIntervalSeconds <= 24 * 60 * 60),
            CONSTRAINT CK_Monitors_PollingTimeoutSeconds CHECK (PollingIntervalSeconds >= 5 AND PollingIntervalSeconds <= 30)
        );
    END;
