CREATE TABLE Organizations
(
    Id        UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    Name      VARCHAR(256)     NOT NULL,
    CreatedAt DATETIME2        NOT NULL,
    UpdatedAt DATETIME2        NOT NULL,

    CONSTRAINT PK_Organizations PRIMARY KEY (Id)
);