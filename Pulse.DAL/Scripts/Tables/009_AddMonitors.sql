IF OBJECT_ID(N'dbo.Monitors', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Monitors
        (
            Id                     UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
            Name                   NVARCHAR(64)     NOT NULL,
            Url                    NVARCHAR(2083)   NOT NULL,
            HttpMethod             NVARCHAR(20)     NOT NULL,
            ResultPath             NVARCHAR(255),
            CurrentValue           NVARCHAR(MAX),
            LastCheckedAt          DATETIMEOFFSET,
            StatusId               UNIQUEIDENTIFIER NOT NULL,
            PollingIntervalSeconds INT              NOT NULL,
            PollingTimeoutSeconds  INT              NOT NULL,

            CONSTRAINT PK_Monitors_Id PRIMARY KEY (Id),
            CONSTRAINT FK_Monitors_Status_Id FOREIGN KEY (StatusId) REFERENCES dbo.MonitorStatuses (Id),
            CONSTRAINT CK_Monitors_PollingIntervalSeconds CHECK (PollingIntervalSeconds BETWEEN 60 AND 24 * 60 * 60),
            CONSTRAINT CK_Monitors_PollingTimeoutSeconds CHECK (PollingTimeoutSeconds BETWEEN 5 AND 30),
            CONSTRAINT CK_Monitors_ResultPath CHECK (LEN(ResultPath) BETWEEN 1 AND 255),
            CONSTRAINT CK_Monitors_HttpMethod CHECK (HttpMethod IN (
                                                                    'GET',
                                                                    'POST',
                                                                    'PUT',
                                                                    'PATCH',
                                                                    'DELETE',
                                                                    'HEAD',
                                                                    'OPTIONS'))
        );
    END;
