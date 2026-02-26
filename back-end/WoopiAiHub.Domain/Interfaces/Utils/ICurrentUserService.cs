namespace WoopiAiHub.Domain.Interfaces.Utils
{
    /// <summary>
    /// Provides access to the currently authenticated user's identity and claims for the current request.
    /// Resolved from the JWT when available (e.g. in HTTP request pipeline).
    /// </summary>
    public interface ICurrentUserService
    {
        /// <summary>
        /// Whether the current request has an authenticated user (JWT valid and present).
        /// </summary>
        bool IsAuthenticated { get; }

        /// <summary>
        /// Database primary key of the current user (from the "userId" claim). Set when the token was issued with this claim.
        /// </summary>
        Guid? Id { get; }

        /// <summary>
        /// User identifier (subject or name identifier from the token). May be email depending on token configuration.
        /// </summary>
        string? UserId { get; }

        /// <summary>
        /// Email of the current user from the token.
        /// </summary>
        string? Email { get; }

        /// <summary>
        /// Whether the current user has the admin role/claim.
        /// </summary>
        bool IsAdmin { get; }

        /// <summary>
        /// Gets the value of a claim by type, or null if not present.
        /// </summary>
        /// <param name="claimType">Claim type (e.g. "sub", "email", "isAdmin", "permissions").</param>
        string? GetClaim(string claimType);
    }
}
