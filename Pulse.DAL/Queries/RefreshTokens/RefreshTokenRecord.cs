namespace Pulse.DAL.Queries.RefreshTokens;

public record RefreshTokenRecord(
    Guid Id,
    Guid UserId,
    string TokenHash,
    Guid FamilyId,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? UsedAt = null,
    DateTimeOffset? RevokedAt = null,
    Guid? ReplacedByTokenId = null,
    string? RevocationReason = null
);
