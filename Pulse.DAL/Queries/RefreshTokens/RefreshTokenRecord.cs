namespace Pulse.DAL.Queries.RefreshTokens;

/// <summary>
/// Represents a refresh token record in the database.
/// </summary>
/// <param name="Id">The unique identifier of the token.</param>
/// <param name="UserId">The ID of the user the token belongs to.</param>
/// <param name="TokenHash">The hashed token string.</param>
/// <param name="FamilyId">The family ID used to group rotated tokens.</param>
/// <param name="CreatedAt">The date and time the token was created.</param>
/// <param name="ExpiresAt">The date and time the token expires.</param>
/// <param name="UsedAt">The date and time the token was used, if any.</param>
/// <param name="RevokedAt">The date and time the token was revoked, if any.</param>
/// <param name="ReplacedByTokenId">The ID of the token that replaced this one, if any.</param>
/// <param name="RevocationReason">The reason the token was revoked, if any.</param>
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
