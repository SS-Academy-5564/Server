CREATE TABLE PasswordResetCodes
(
    Id             UNIQUEIDENTIFIER  NOT NULL DEFAULT NEWSEQUENTIALID(),
    UserId         UNIQUEIDENTIFIER  NOT NULL,
    CodeHash       VARCHAR(256)      NOT NULL,
    Jti            VARCHAR(36)       NULL,
    ExpiresAt      DATETIMEOFFSET    NOT NULL,
    FailedAttempts INT               NOT NULL DEFAULT 0,

    CONSTRAINT PK_PasswordResetCodes        PRIMARY KEY (Id),
    CONSTRAINT UQ_PasswordResetCodes_UserId UNIQUE (UserId),
    CONSTRAINT FK_PasswordResetCodes_Users  FOREIGN KEY (UserId) REFERENCES Users (Id) ON DELETE CASCADE,
    CONSTRAINT CHK_PasswordResetCodes_Expiry CHECK (ExpiresAt > SYSDATETIMEOFFSET())
);
