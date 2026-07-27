IF NOT EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('PasswordResetCodes')
      AND name = 'CreatedAt'
)
BEGIN
    ALTER TABLE PasswordResetCodes
        ADD CreatedAt DATETIMEOFFSET NOT NULL
            CONSTRAINT DF_PasswordResetCodes_CreatedAt DEFAULT SYSDATETIMEOFFSET();
END
