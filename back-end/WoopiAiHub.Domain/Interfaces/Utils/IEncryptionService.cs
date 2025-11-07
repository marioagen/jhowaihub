namespace WoopiAiHub.Domain.Interfaces.Utils
{
    public interface IEncryptionService
    {
        string Encrypt(string plainText);
        string Decrypt(string encryptedText);
    }
}
