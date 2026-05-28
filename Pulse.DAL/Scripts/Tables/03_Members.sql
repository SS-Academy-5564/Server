CREATE TABLE Members
(
    Id             UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    UserId         UNIQUEIDENTIFIER NOT NULL,
    OrganizationId UNIQUEIDENTIFIER NOT NULL,
    Role           VARCHAR(10)      NOT NULL,
    JoinedAt       DATETIME2        NOT NULL,
    UpdatedAt      DATETIME2        NOT NULL,

    CONSTRAINT PK_Members          PRIMARY KEY (Id),
    CONSTRAINT FK_Members_Users    FOREIGN KEY (UserId)         REFERENCES Users (Id)         ON DELETE CASCADE,
    CONSTRAINT FK_Members_Orgs     FOREIGN KEY (OrganizationId) REFERENCES Organizations (Id) ON DELETE CASCADE,
    CONSTRAINT UQ_Members_UserOrg  UNIQUE (UserId, OrganizationId),
    CONSTRAINT CK_Members_Role     CHECK (Role IN ('User', 'Viewer'))
);