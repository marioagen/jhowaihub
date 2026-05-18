namespace WoopiAiHub.Domain.Utils
{
    /// <summary>
    /// Well-known JWT claim type names used across token issuance and request validation.
    /// </summary>
    public static class JwtClaimNames
    {
        /// <summary>
        /// Claim that binds the access token to a single tenant; must match the X-Tenant header on API requests.
        /// </summary>
        public const string Tenant = "tenant";
    }
}
