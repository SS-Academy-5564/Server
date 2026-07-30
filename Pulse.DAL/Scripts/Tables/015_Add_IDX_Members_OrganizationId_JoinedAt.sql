CREATE INDEX IDX_Members_OrganizationId_JoinedAt
    ON Members (OrganizationId, JoinedAt, Id)
    INCLUDE (UserId, RoleId);
