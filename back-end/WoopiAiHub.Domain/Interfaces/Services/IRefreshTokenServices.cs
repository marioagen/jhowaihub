namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IRefreshTokenServices
    {
        Task SaveAsync(string userEmail, string refreshToken);
        Task<string?> FindUserByRefreshTokenAsync(string refreshToken);
        Task RevokeAsync(string refreshToken);
    }
}
