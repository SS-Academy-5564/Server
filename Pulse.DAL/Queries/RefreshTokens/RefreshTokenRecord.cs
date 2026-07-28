namespace Pulse.DAL.Queries.RefreshTokens;

public record RefreshTokenRecord(
    Guid Id,
    Guid UserId,
    string TokenHash,
    Guid FamilyId,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? UsedAt,
    DateTimeOffset? RevokedAt,
    Guid? ReplacedByTokenId,
    string? RevocationReason
);
