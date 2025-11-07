namespace WoopiAiHub.Domain.Interfaces.Utils
{
    /// <summary>
    /// Service for encrypting and decrypting sensitive data
    /// </summary>
    public interface IEncryptionService
    {
        /// <summary>
        /// Encrypts a plain text value
        /// </summary>
        /// <param name="plainText">The text to encrypt</param>
        /// <returns>The encrypted value as a Base64 string</returns>
        string Encrypt(string plainText);

        /// <summary>
        /// Decrypts an encrypted value
        /// </summary>
        /// <param name="encryptedText">The encrypted text as a Base64 string</param>
        /// <returns>The decrypted plain text</returns>
        string Decrypt(string encryptedText);
    }
}
