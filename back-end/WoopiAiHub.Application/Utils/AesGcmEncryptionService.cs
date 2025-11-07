using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Domain.Utils;

namespace WoopiAiHub.Application.Utils
{
    /// <summary>
    /// Encryption service using AES-256-GCM for secure symmetric encryption
    /// </summary>
    public class AesGcmEncryptionService : IEncryptionService
    {
        private readonly byte[] _key;
        private const int NonceSize = 12;
        private const int TagSize = 16;

        public AesGcmEncryptionService(IOptions<EncryptionSettings> options)
        {
            var keyString = options.Value.Key;
            
            if (string.IsNullOrEmpty(keyString))
            {
                throw new ArgumentException("Encryption key cannot be null or empty.", nameof(options));
            }
            _key = DeriveKey(keyString);
        }

        /// <summary>
        /// Encrypts plain text using AES-256-GCM
        /// </summary>
        /// <param name="plainText">The text to encrypt</param>
        /// <returns>Base64 encoded string containing nonce + ciphertext + tag</returns>
        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                throw new ArgumentException("Plain text cannot be null or empty.", nameof(plainText));
            }

            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var nonce = new byte[NonceSize];
            var ciphertext = new byte[plainBytes.Length];
            var tag = new byte[TagSize];

            RandomNumberGenerator.Fill(nonce);

            using var aesGcm = new AesGcm(_key, TagSize);
            aesGcm.Encrypt(nonce, plainBytes, ciphertext, tag);

            var result = new byte[NonceSize + ciphertext.Length + TagSize];
            Buffer.BlockCopy(nonce, 0, result, 0, NonceSize);
            Buffer.BlockCopy(ciphertext, 0, result, NonceSize, ciphertext.Length);
            Buffer.BlockCopy(tag, 0, result, NonceSize + ciphertext.Length, TagSize);

            return Convert.ToBase64String(result);
        }

        /// <summary>
        /// Decrypts encrypted text using AES-256-GCM
        /// </summary>
        /// <param name="encryptedText">Base64 encoded string containing nonce + ciphertext + tag</param>
        /// <returns>Decrypted plain text</returns>
        public string Decrypt(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
            {
                throw new ArgumentException("Encrypted text cannot be null or empty.", nameof(encryptedText));
            }

            var encryptedBytes = Convert.FromBase64String(encryptedText);

            if (encryptedBytes.Length < NonceSize + TagSize)
            {
                throw new ArgumentException("Invalid encrypted data format.", nameof(encryptedText));
            }

            var nonce = new byte[NonceSize];
            var ciphertext = new byte[encryptedBytes.Length - NonceSize - TagSize];
            var tag = new byte[TagSize];

            Buffer.BlockCopy(encryptedBytes, 0, nonce, 0, NonceSize);
            Buffer.BlockCopy(encryptedBytes, NonceSize, ciphertext, 0, ciphertext.Length);
            Buffer.BlockCopy(encryptedBytes, NonceSize + ciphertext.Length, tag, 0, TagSize);

            var plainBytes = new byte[ciphertext.Length];

            using var aesGcm = new AesGcm(_key, TagSize);
            aesGcm.Decrypt(nonce, ciphertext, tag, plainBytes);

            return Encoding.UTF8.GetString(plainBytes);
        }

        /// <summary>
        /// Derives a 256-bit key from the input string using SHA256
        /// </summary>
        /// <param name="keyString">Input key string</param>
        /// <returns>32-byte key suitable for AES-256</returns>
        private static byte[] DeriveKey(string keyString)
        {
            using var sha256 = SHA256.Create();
            return sha256.ComputeHash(Encoding.UTF8.GetBytes(keyString));
        }
    }
}
