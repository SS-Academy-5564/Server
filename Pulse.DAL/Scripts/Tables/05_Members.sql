CREATE TABLE Members
(
    Id             UNIQUEIDENTIFIER  NOT NULL DEFAULT NEWSEQUENTIALID(),
    UserId         UNIQUEIDENTIFIER  NOT NULL,
    OrganizationId UNIQUEIDENTIFIER  NOT NULL,
    RoleId         UNIQUEIDENTIFIER  NOT NULL,
    JoinedAt       DATETIMEOFFSET    NOT NULL,
    UpdatedAt      DATETIMEOFFSET    NOT NULL,

    CONSTRAINT PK_Members         PRIMARY KEY (Id),
    CONSTRAINT FK_Members_Users   FOREIGN KEY (UserId)         REFERENCES Users (Id)         ON DELETE CASCADE,
    CONSTRAINT FK_Members_Orgs    FOREIGN KEY (OrganizationId) REFERENCES Organizations (Id) ON DELETE CASCADE,
    CONSTRAINT FK_Members_Roles   FOREIGN KEY (RoleId)         REFERENCES Roles (Id),
    CONSTRAINT UQ_Members_UserOrg UNIQUE (UserId, OrganizationId)
);