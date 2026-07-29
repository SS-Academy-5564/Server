-- 1. Add new columns allowing NULLs initially
ALTER TABLE RefreshTokens ADD 
    TokenHash VARCHAR(100) NULL,
    FamilyId UNIQUEIDENTIFIER NULL,
    UsedAt DATETIMEOFFSET NULL,
    ReplacedByTokenId UNIQUEIDENTIFIER NULL,
    RevocationReason NVARCHAR(100) NULL;

GO

-- 2. Backfill existing records: Compute hash for Token, set FamilyId to Id
UPDATE r
SET
    r.TokenHash = CAST(N'' AS XML).value('xs:base64Binary(sql:column("CA.BinHash"))', 'VARCHAR(100)'),
    r.FamilyId = r.Id
FROM RefreshTokens r
CROSS APPLY (
    SELECT HASHBYTES('SHA2_256', CAST(r.Token AS VARCHAR(MAX))) AS BinHash
) AS CA;

GO

-- 3. Make columns NOT NULL
ALTER TABLE RefreshTokens ALTER COLUMN TokenHash VARCHAR(100) NOT NULL;
ALTER TABLE RefreshTokens ALTER COLUMN FamilyId UNIQUEIDENTIFIER NOT NULL;

GO

-- (We cannot drop Token here because Indexes/RefreshTokens.sql still needs to run on a fresh DB)
-- The cleanup of Token is moved to the Indexes folder.

-- 5. Add Foreign Key for ReplacedByTokenId
ALTER TABLE RefreshTokens ADD CONSTRAINT FK_RefreshTokens_ReplacedBy FOREIGN KEY (ReplacedByTokenId) REFERENCES RefreshTokens (Id);

GO

-- 6. Add new optimized indexes
CREATE NONCLUSTERED INDEX IDX_RefreshTokens_TokenHash
ON RefreshTokens (TokenHash);

CREATE NONCLUSTERED INDEX IDX_RefreshTokens_FamilyId
ON RefreshTokens (FamilyId);

CREATE NONCLUSTERED INDEX IDX_RefreshTokens_UserId
ON RefreshTokens (UserId);
