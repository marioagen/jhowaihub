namespace WoopiAiHub.Domain.Interfaces.Utils
{
    /// <summary>
    /// Provides access to the currently authenticated user's identity and claims for the current request.
    /// Resolved from the JWT when available (e.g. in HTTP request pipeline).
    /// </summary>
    public interface ICurrentUserService
    {
        bool IsAuthenticated { get; }
        Guid? Id { get; }
        string? UserId { get; }
        string? Email { get; }
        bool IsAdmin { get; }

        /// <summary>
        /// Tenant bound to the current access token, when present in the JWT.
        /// </summary>
        string? Tenant { get; }
        string? FindClaim(string claimType);
    }
}
