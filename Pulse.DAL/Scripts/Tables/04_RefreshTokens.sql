CREATE TABLE RefreshTokens
(
    Id        UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    UserId    UNIQUEIDENTIFIER NOT NULL,
    Token     VARCHAR(100)     NOT NULL,
    CreatedAt DATETIME2        NOT NULL,
    ExpiresAt DATETIME2        NOT NULL,
    RevokedAt DATETIME2        NULL,

    CONSTRAINT PK_RefreshTokens       PRIMARY KEY (Id),
    CONSTRAINT FK_RefreshTokens_Users FOREIGN KEY (UserId) REFERENCES Users (Id) ON DELETE CASCADE,
    CONSTRAINT CHK_RefreshTokens_Expiry CHECK (ExpiresAt > CreatedAt)
);