using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using WoopiAiHub.Domain.Interfaces.Utils;

namespace WoopiAiHub.Application.Utils
{
    /// <summary>
    /// Resolves the current user from the request's JWT (HttpContext.User).
    /// When no HTTP context or no authenticated user is present (e.g. background consumers), all values return null/false.
    /// </summary>
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

        /// <inheritdoc />
        public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

        /// <inheritdoc />
        public Guid? Id
        {
            get
            {
                var value = User?.FindFirst("userId")?.Value;
                return Guid.TryParse(value, out var id) ? id : null;
            }
        }

        /// <inheritdoc />
        public string? UserId =>
            User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User?.FindFirst(ClaimTypes.Name)?.Value
            ?? User?.FindFirst("sub")?.Value;

        /// <inheritdoc />
        public string? Email =>
            User?.FindFirst(ClaimTypes.Email)?.Value
            ?? User?.FindFirst("email")?.Value
            ?? User?.FindFirst("sub")?.Value;

        /// <inheritdoc />
        public bool IsAdmin
        {
            get
            {
                var value = User?.FindFirst("isAdmin")?.Value;
                return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <inheritdoc />
        public string? FindClaim(string claimType) => User?.FindFirst(claimType)?.Value;
    }
}
