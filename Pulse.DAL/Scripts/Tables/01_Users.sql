CREATE TABLE Users
(
    Id           UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    Email        VARCHAR(256)     NOT NULL,
    PasswordHash VARCHAR(256)     NOT NULL,
    CreatedAt    DATETIME2        NOT NULL,
    UpdatedAt    DATETIME2        NOT NULL,
 
    CONSTRAINT PK_Users PRIMARY KEY (Id)
);