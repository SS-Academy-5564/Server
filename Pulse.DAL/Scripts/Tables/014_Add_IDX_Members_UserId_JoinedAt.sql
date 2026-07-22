CREATE INDEX IDX_Members_UserId_JoinedAt
    ON Members (UserId, JoinedAt DESC)
    INCLUDE (OrganizationId, RoleId);
