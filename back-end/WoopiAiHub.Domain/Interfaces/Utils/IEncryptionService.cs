namespace WoopiAiHub.Domain.Interfaces.Utils
{
    /// <summary>
    /// Service for encrypting and decrypting sensitive data
    /// </summary>
    public interface IEncryptionService
    {
        string Encrypt(string plainText);
        string Decrypt(string encryptedText);
        bool IsEncrypted(string text);
    }
}
