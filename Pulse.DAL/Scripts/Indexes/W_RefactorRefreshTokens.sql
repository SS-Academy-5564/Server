DROP INDEX IF EXISTS IDX_RefreshTokens_Token ON RefreshTokens;

ALTER TABLE RefreshTokens DROP COLUMN Token;
