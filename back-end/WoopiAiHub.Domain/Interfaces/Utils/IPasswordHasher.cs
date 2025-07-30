namespace WoopiAiHub.Domain.Interfaces.Utils
{
    public interface IPasswordHasher
    {
        byte[] Hash(string password, byte[] saltBytes);
        bool Verify(string password, byte[] storedHash, byte[] storedSalt);
        byte[] GenerateSalt(int length = 16);
    }
}
