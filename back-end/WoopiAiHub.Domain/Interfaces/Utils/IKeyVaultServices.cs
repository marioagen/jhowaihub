namespace WoopiAiHub.Domain.Interfaces.Utils
{
    public interface IKeyVaultServices
    {
        Task SetSecretAsync(string key, string value);
        Task<string?> GetSecretAsync(string key);
        Task DeleteSecretAsync(string key);
    }
}
