ALTER TABLE UserLoginAttempts
ADD Identifier NVARCHAR(255) NOT NULL
    CONSTRAINT DF_UserLoginAttempts_Identifier DEFAULT (N'');

ALTER TABLE UserLoginAttempts
DROP CONSTRAINT PK_UserLoginAttempts;

ALTER TABLE UserLoginAttempts
ADD CONSTRAINT PK_UserLoginAttempts PRIMARY KEY (UserId, Identifier);
