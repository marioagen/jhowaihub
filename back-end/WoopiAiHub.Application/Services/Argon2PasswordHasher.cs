using Isopoh.Cryptography.Argon2;
using System.Data.SqlTypes;
using System.Security.Cryptography;
using System.Text;
using WoopiAiHub.Domain.Interfaces.Utils;

namespace WoopiAiHub.Application.Services
{
    public class Argon2PasswordHasher : IPasswordHasher
    {
        /// <summary>
        /// Generates a secure hash for the specified password using the Argon2 algorithm.
        /// </summary>
        /// <param name="password">The password to be hashed. Must not be null or empty.</param>
        /// <param name="saltBytes">The salt to use for hashing, represented as a byte array. Must not be null or empty.</param>
        /// <returns>A byte array representing the hashed password (32 bytes).</returns>
        public byte[] Hash(string password, byte[] saltBytes)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty.", nameof(password));

            if (saltBytes == null || saltBytes.Length == 0)
                throw new ArgumentException("Salt cannot be null or empty.", nameof(saltBytes));

            var config = CreateConfig(password, saltBytes);

            using var argon2 = new Argon2(config);
            using var hashBytes = argon2.Hash();

            // Retorna o array de bytes diretamente para salvar como varbinary
            var result = new byte[hashBytes.Buffer.Length];
            Array.Copy(hashBytes.Buffer, result, hashBytes.Buffer.Length);
            return result;
        }


        /// <summary>
        /// Verifies whether the provided password matches the stored hash using the Argon2 hashing algorithm.
        /// </summary>
        /// <param name="password">The plaintext password to verify.</param>
        /// <param name="storedHash">The binary hash to compare against.</param>
        /// <param name="storedSalt">The binary salt used during the original hashing process.</param>
        /// <returns><see langword="true"/> if the provided password matches the stored hash; otherwise, <see langword="false"/>.</returns>
        public bool Verify(string password, byte[] storedHash, byte[] storedSalt)
        {
            if (string.IsNullOrEmpty(password))
                return false;

            if (storedHash == null || storedSalt == null)
                return false;

            var config = CreateConfig(password, storedSalt);

            using var argon2 = new Argon2(config);
            using var hashBytes = argon2.Hash();

            return CompareBytes(hashBytes.Buffer, storedHash);
        }

        /// <summary>
        /// Generates a random salt of the specified length using a secure random number generator.
        /// </summary>
        /// <param name="length">The length of the salt in bytes. Default is 16 bytes.</param>
        /// <returns>A byte array containing the random salt.</returns>
        public byte[] GenerateSalt(int length = 16)
        {
            if (length <= 0)
                throw new ArgumentException("Salt length must be greater than zero.", nameof(length));

            var salt = new byte[length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);
            return salt;
        }

        /// <summary>
        /// Compares two byte arrays in constant time to prevent timing attacks.
        /// </summary>
        /// <param name="a">First byte array</param>
        /// <param name="b">Second byte array</param>
        /// <returns>True if arrays are equal, false otherwise</returns>
        private static bool CompareBytes(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;

            int result = 0;
            for (int i = 0; i < a.Length; i++)
            {
                result |= a[i] ^ b[i];
            }
            return result == 0;
        }

        /// <summary>
        /// Creates a configuration for the Argon2 hashing algorithm based on the provided password and salt.
        /// </summary>
        /// <param name="password"></param>
        /// <param name="saltBytes"></param>
        /// <returns></returns>
        private static Argon2Config CreateConfig(string password, byte[] saltBytes)
        {
            var config = new Argon2Config
            {
                Type = Argon2Type.DataIndependentAddressing,
                Version = Argon2Version.Nineteen,
                TimeCost = 4,
                MemoryCost = 1 << 16, // 64 MB
                Lanes = 4,
                Threads = 4,
                Password = Encoding.UTF8.GetBytes(password),
                Salt = saltBytes,
                HashLength = 32
            };
            return config;
        }
    }
}