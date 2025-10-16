using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Options;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Domain.Utils;

namespace WoopiAiHub.Application.Utils
{
    public class AzureKeyVaultServices : IKeyVaultServices
    {
        private readonly SecretClient _client;
        private readonly KeyVaultSettings _settings;

        public AzureKeyVaultServices(IOptions<KeyVaultSettings> options)
        {
            _settings = options.Value;

            if (string.IsNullOrEmpty(_settings.VaultUrl))
            {
                throw new ArgumentNullException(nameof(_settings.VaultUrl));
            }

            if (string.IsNullOrWhiteSpace(_settings.ClientSecret))
                _client = new SecretClient(new Uri(_settings.VaultUrl), new DefaultAzureCredential()); // usa login local (CLI, VS, VS Code)
            else
                _client = new SecretClient(new Uri(_settings.VaultUrl), new ClientSecretCredential(_settings.TenantId, _settings.ClientId, _settings.ClientSecret));
        }

        /// <summary>
        /// Set a key in Azure vault
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task SetSecretAsync(string key, string value)
        {
            await _client.SetSecretAsync(key, value);
        }

        /// <summary>
        /// Get a key in Azure vault
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<string?> GetSecretAsync(string key)
        {
            var secret = await _client.GetSecretAsync(key);
            return secret.Value.Value;
        }

        /// <summary>
        /// Delete a key in Azure vault 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task DeleteSecretAsync(string key)
        {
            await _client.StartDeleteSecretAsync(key);
        }
    }
}
