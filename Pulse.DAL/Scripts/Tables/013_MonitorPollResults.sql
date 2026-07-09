IF OBJECT_ID(N'dbo.MonitorPollResults', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.MonitorPollResults
        (
            Id              UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
            Value           NVARCHAR(50),
            CheckedAt       DATETIME2        NOT NULL CONSTRAINT DF_MonitorPollResults_CheckedAt DEFAULT SYSUTCDATETIME(),
            IsSuccess       BIT              NOT NULL,
            ResponseTimeMs  INT              NOT NULL,
            StatusCode      SMALLINT,
            MonitorId       UNIQUEIDENTIFIER NOT NULL,
            RequestStatusId UNIQUEIDENTIFIER NOT NULL,

            CONSTRAINT PK_MonitorPollResults_Id PRIMARY KEY (Id),
            CONSTRAINT FK_MonitorPollResults_Monitor_Id FOREIGN KEY (MonitorId) REFERENCES dbo.Monitors (Id),
            CONSTRAINT FK_MonitorPollResults_RequestStatus_Id FOREIGN KEY (RequestStatusId) REFERENCES dbo.RequestStatus (Id),
            CONSTRAINT CK_MonitorPollResults_StatusCode CHECK (StatusCode BETWEEN 100 AND 599)
        );
    END;
