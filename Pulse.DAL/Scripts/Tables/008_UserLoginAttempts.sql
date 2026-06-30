Create table UserLoginAttempts(
    UserId UniqueIdentifier NOT NULL,
    FailedAttempts INT NOT NULL CONSTRAINT DF_UserLoginAttempts_FailedAttempts DEFAULT(0),
    LockedUntil DateTime2 NULL,

    CONSTRAINT PK_UserLoginAttempts PRIMARY KEY(UserId),
    CONSTRAINT FK_UserLoginAttempts_Users FOREIGN KEY(UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT CK_UserLoginAttempts_FailedAttempts CHECK ( FailedAttempts>=0 )
)
