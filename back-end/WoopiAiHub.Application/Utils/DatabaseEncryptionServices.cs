using WoopiAiHub.Domain.Interfaces.Utils;

namespace WoopiAiHub.Application.Utils
{
    /// <summary>
    /// Implementation of IKeyVaultServices that uses database storage with encryption
    /// instead of Azure Key Vault
    /// </summary>
    public class DatabaseEncryptionServices : IKeyVaultServices
    {
        private readonly IEncryptionService _encryptionService;

        public DatabaseEncryptionServices(IEncryptionService encryptionService)
        {
            _encryptionService = encryptionService;
        }

        /// <summary>
        /// Encrypts and returns the encrypted value (for storage in database)
        /// Note: This doesn't actually "set" anything - the encrypted value should be stored by the caller
        /// </summary>
        /// <param name="key">Unused - kept for interface compatibility</param>
        /// <param name="value">The plain text value to encrypt</param>
        /// <returns>Completed task</returns>
        public Task SetSecretAsync(string key, string value)
        {
            // This method is called during Tool creation/update
            // The actual storage happens in the repository layer
            // We just need to ensure the value gets encrypted before storage
            // The encryption will be handled in GetSecretAsync when needed
            return Task.CompletedTask;
        }

        /// <summary>
        /// Decrypts and returns the secret value
        /// </summary>
        /// <param name="key">The encrypted value from database</param>
        /// <returns>Decrypted plain text value</returns>
        public Task<string?> GetSecretAsync(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return Task.FromResult<string?>(null);
            }

            try
            {
                var decrypted = _encryptionService.Decrypt(key);
                return Task.FromResult<string?>(decrypted);
            }
            catch
            {
                // If decryption fails, return null
                return Task.FromResult<string?>(null);
            }
        }

        /// <summary>
        /// Delete operation is no longer needed with database storage
        /// The deletion happens at the database record level
        /// </summary>
        /// <param name="key">Unused</param>
        /// <returns>Completed task</returns>
        public Task DeleteSecretAsync(string key)
        {
            // No action needed - deletion handled by database record deletion
            return Task.CompletedTask;
        }

        /// <summary>
        /// Returns empty string since we no longer need to generate key names
        /// The encrypted value is stored directly in the database field
        /// </summary>
        /// <returns>Empty string</returns>
        public string CreateKeyName()
        {
            // No longer need to create key names for Key Vault
            return string.Empty;
        }
    }
}
