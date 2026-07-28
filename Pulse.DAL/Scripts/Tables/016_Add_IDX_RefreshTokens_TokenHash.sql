CREATE NONCLUSTERED INDEX IDX_RefreshTokens_TokenHash
ON RefreshTokens (TokenHash);

CREATE NONCLUSTERED INDEX IDX_RefreshTokens_UserId_FamilyId
ON RefreshTokens (UserId, FamilyId);
