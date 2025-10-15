using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using WoopiAiHub.Domain.Interfaces.Utils;

namespace WoopiAiHub.Application.Utils
{
    public class AzureKeyVaultServices : IKeyVaultServices
    {
        private readonly SecretClient _client;

        public AzureKeyVaultServices(IConfiguration configuration)
        {
            var vaultUrl = configuration["Azure:VaultUrl"];
            if (string.IsNullOrEmpty(vaultUrl))
            {
                throw new ArgumentNullException(nameof(vaultUrl));
            }

            var tenantId = configuration["Azure:TenantId"];
            var clientId = configuration["Azure:ClientId"];
            var clientSecret = configuration["Azure:ClientSecret"];

            if (string.IsNullOrWhiteSpace(clientSecret))
                _client = new SecretClient(new Uri(vaultUrl), new DefaultAzureCredential()); // usa login local (CLI, VS, VS Code)
            else
                _client = new SecretClient(new Uri(vaultUrl), new ClientSecretCredential(tenantId, clientId, clientSecret));
        }

        public async Task SetSecretAsync(string key, string value)
        {
            await _client.SetSecretAsync(key, value);
        }

        public async Task<string?> GetSecretAsync(string key)
        {
            var secret = await _client.GetSecretAsync(key);
            return secret.Value.Value;
        }

        public async Task DeleteSecretAsync(string key)
        {
            await _client.StartDeleteSecretAsync(key);
        }
    }
}
