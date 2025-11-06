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
        /// This method is maintained for interface compatibility but is no longer used.
        /// With database storage, encryption happens directly in the service layer before saving.
        /// </summary>
        /// <param name="key">Unused - kept for interface compatibility</param>
        /// <param name="value">Unused - kept for interface compatibility</param>
        /// <returns>Completed task</returns>
        /// <remarks>
        /// In the new architecture, API keys are encrypted in ToolServices before being saved to the database.
        /// This method exists only to maintain compatibility with the IKeyVaultServices interface.
        /// </remarks>
        public Task SetSecretAsync(string key, string value)
        {
            // No operation needed - encryption happens at the service layer
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
            catch (FormatException)
            {
                // Invalid base64 format - likely corrupted data
                return Task.FromResult<string?>(null);
            }
            catch (System.Security.Cryptography.CryptographicException)
            {
                // Decryption failed - wrong key or tampered data
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
