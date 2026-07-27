IF OBJECT_ID(N'dbo.RequestStatus', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.RequestStatus
        (
            Id     UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
            Status VARCHAR(50)      NOT NULL,

            CONSTRAINT PK_RequestStatus_Id PRIMARY KEY (Id),
            CONSTRAINT UQ_RequestStatus_Status UNIQUE (Status)
        );
    END;
