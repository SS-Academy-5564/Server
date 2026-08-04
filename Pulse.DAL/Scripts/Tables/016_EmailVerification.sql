ALTER TABLE Users
ADD EmailVerifiedAt DATETIMEOFFSET NULL;

EXEC(N'
    UPDATE Users
    SET EmailVerifiedAt = CreatedAt
    WHERE EmailVerifiedAt IS NULL;
');

CREATE TABLE EmailVerificationTokens
(
    Id        UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    UserId    UNIQUEIDENTIFIER NOT NULL,
    TokenHash CHAR(64)         NOT NULL,
    ExpiresAt DATETIMEOFFSET   NOT NULL,
    CreatedAt DATETIMEOFFSET   NOT NULL,
    UsedAt    DATETIMEOFFSET   NULL,

    CONSTRAINT PK_EmailVerificationTokens PRIMARY KEY (Id),
    CONSTRAINT UQ_EmailVerificationTokens_TokenHash UNIQUE (TokenHash),
    CONSTRAINT FK_EmailVerificationTokens_Users FOREIGN KEY (UserId) REFERENCES Users (Id) ON DELETE CASCADE,
    CONSTRAINT CHK_EmailVerificationTokens_Expiry CHECK (ExpiresAt > CreatedAt)
);

CREATE INDEX IX_EmailVerificationTokens_UserId
    ON EmailVerificationTokens (UserId);
