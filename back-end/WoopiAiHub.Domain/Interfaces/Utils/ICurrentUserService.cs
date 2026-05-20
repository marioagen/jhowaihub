namespace WoopiAiHub.Domain.Interfaces.Utils
{
    public interface ICurrentUserService
    {
        bool IsAuthenticated { get; }
        Guid? Id { get; }
        string? UserId { get; }
        string? Email { get; }
        bool IsAdmin { get; }
        string? Tenant { get; }
        string? FindClaim(string claimType);
    }
}
