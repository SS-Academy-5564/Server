-- Cleanup the old Token column and its index 
-- This runs in the Indexes phase so it executes AFTER Indexes/RefreshTokens.sql on a fresh DB

DROP INDEX IF EXISTS IDX_RefreshTokens_Token ON RefreshTokens;
ALTER TABLE RefreshTokens DROP COLUMN Token;
