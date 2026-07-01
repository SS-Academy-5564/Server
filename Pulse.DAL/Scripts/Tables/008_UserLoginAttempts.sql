Create table UserLoginAttempts(
    UserId UniqueIdentifier NOT NULL,
    AttemptCount INT NOT NULL CONSTRAINT DF_UserLoginAttempts_AttemptCount DEFAULT(0),
    LockedUntil DateTime2 NULL,

    CONSTRAINT PK_UserLoginAttempts PRIMARY KEY(UserId),
    CONSTRAINT FK_UserLoginAttempts_Users FOREIGN KEY(UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT CHK_UserLoginAttempts_AttemptCount CHECK (AttemptCount >= 0)
)
