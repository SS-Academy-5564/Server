IF OBJECT_ID(N'dbo.HttpMethods', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.HttpMethods
        (
            Id   UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
            Name NVARCHAR(20)     NOT NULL,

            CONSTRAINT PK_HttpMethods_Id PRIMARY KEY (Id),
        );
    END;
