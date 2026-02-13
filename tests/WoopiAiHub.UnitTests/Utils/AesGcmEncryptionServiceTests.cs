using Microsoft.Extensions.Options;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.Utils;
using Xunit;

namespace WoopiAiHub.UnitTests.Utils
{
    public class AesGcmEncryptionServiceTests
    {
        [Fact(DisplayName = "Encrypt and Decrypt should return original text")]
        [Trait("Encryption", "Success")]
        public void EncryptDecrypt_ShouldReturnOriginalText()
        {
            // Arrange
            var options = Options.Create(new EncryptionSettings { Key = "test-encryption-key-for-testing" });
            var service = new AesGcmEncryptionService(options);
            var plainText = "my-secret-api-key";

            // Act
            var encrypted = service.Encrypt(plainText);
            var decrypted = service.Decrypt(encrypted);

            // Assert
            Assert.NotEqual(plainText, encrypted); // Encrypted should be different
            Assert.Equal(plainText, decrypted); // Decrypted should match original
        }

        [Fact(DisplayName = "Encrypt should produce different outputs for same input")]
        [Trait("Encryption", "Security")]
        public void Encrypt_ShouldProduceDifferentOutputs_ForSameInput()
        {
            // Arrange
            var options = Options.Create(new EncryptionSettings { Key = "test-encryption-key-for-testing" });
            var service = new AesGcmEncryptionService(options);
            var plainText = "my-secret-api-key";

            // Act
            var encrypted1 = service.Encrypt(plainText);
            var encrypted2 = service.Encrypt(plainText);

            // Assert
            Assert.NotEqual(encrypted1, encrypted2); // Should be different due to random nonce
        }

        [Fact(DisplayName = "Encrypt should throw exception for empty text")]
        [Trait("Encryption", "Validation")]
        public void Encrypt_ShouldThrowException_ForEmptyText()
        {
            // Arrange
            var options = Options.Create(new EncryptionSettings { Key = "test-encryption-key-for-testing" });
            var service = new AesGcmEncryptionService(options);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => service.Encrypt(string.Empty));
            Assert.Throws<ArgumentException>(() => service.Encrypt(null!));
        }

        [Fact(DisplayName = "Decrypt should throw exception for invalid data")]
        [Trait("Encryption", "Validation")]
        public void Decrypt_ShouldThrowException_ForInvalidData()
        {
            // Arrange
            var options = Options.Create(new EncryptionSettings { Key = "test-encryption-key-for-testing" });
            var service = new AesGcmEncryptionService(options);

            // Act & Assert
            Assert.Throws<FormatException>(() => service.Decrypt("invalid-base64"));
        }

        [Fact(DisplayName = "Constructor should throw exception for empty key")]
        [Trait("Encryption", "Validation")]
        public void Constructor_ShouldThrowException_ForEmptyKey()
        {
            // Arrange & Act & Assert
            var options = Options.Create(new EncryptionSettings { Key = string.Empty });
            Assert.Throws<ArgumentException>(() => new AesGcmEncryptionService(options));
        }

        [Fact(DisplayName = "Different keys should produce different encrypted values")]
        [Trait("Encryption", "Security")]
        public void DifferentKeys_ShouldProduceDifferentEncryptedValues()
        {
            // Arrange
            var options1 = Options.Create(new EncryptionSettings { Key = "key-1" });
            var options2 = Options.Create(new EncryptionSettings { Key = "key-2" });
            var service1 = new AesGcmEncryptionService(options1);
            var service2 = new AesGcmEncryptionService(options2);
            var plainText = "my-secret-api-key";

            // Act
            var encrypted1 = service1.Encrypt(plainText);
            var encrypted2 = service2.Encrypt(plainText);

            // Assert
            Assert.NotEqual(encrypted1, encrypted2);
            
            // Verify decryption with wrong key fails with AuthenticationTagMismatchException
            Assert.Throws<System.Security.Cryptography.AuthenticationTagMismatchException>(() => service2.Decrypt(encrypted1));
        }

        [Fact(DisplayName = "IsEncrypted should return true for valid encrypted text")]
        [Trait("IsEncrypted", "Success")]
        public void IsEncrypted_ShouldReturnTrue_ForValidEncryptedText()
        {
            // Arrange
            var options = Options.Create(new EncryptionSettings { Key = "test-encryption-key-for-testing" });
            var service = new AesGcmEncryptionService(options);
            var plainText = "my-secret-api-key";
            var encrypted = service.Encrypt(plainText);

            // Act
            var result = service.IsEncrypted(encrypted);

            // Assert
            Assert.True(result);
        }

        [Fact(DisplayName = "IsEncrypted should return false for plain text")]
        [Trait("IsEncrypted", "Success")]
        public void IsEncrypted_ShouldReturnFalse_ForPlainText()
        {
            // Arrange
            var options = Options.Create(new EncryptionSettings { Key = "test-encryption-key-for-testing" });
            var service = new AesGcmEncryptionService(options);
            var plainText = "my-secret-api-key";

            // Act
            var result = service.IsEncrypted(plainText);

            // Assert
            Assert.False(result);
        }

        [Fact(DisplayName = "IsEncrypted should return false for invalid base64")]
        [Trait("IsEncrypted", "Validation")]
        public void IsEncrypted_ShouldReturnFalse_ForInvalidBase64()
        {
            // Arrange
            var options = Options.Create(new EncryptionSettings { Key = "test-encryption-key-for-testing" });
            var service = new AesGcmEncryptionService(options);
            var invalidBase64 = "invalid-base64-!!!";

            // Act
            var result = service.IsEncrypted(invalidBase64);

            // Assert
            Assert.False(result);
        }

        [Fact(DisplayName = "IsEncrypted should return false for too short base64")]
        [Trait("IsEncrypted", "Validation")]
        public void IsEncrypted_ShouldReturnFalse_ForTooShortBase64()
        {
            // Arrange
            var options = Options.Create(new EncryptionSettings { Key = "test-encryption-key-for-testing" });
            var service = new AesGcmEncryptionService(options);
            // Valid base64 but too short (less than NonceSize + TagSize = 28 bytes)
            var shortBase64 = Convert.ToBase64String(new byte[20]);

            // Act
            var result = service.IsEncrypted(shortBase64);

            // Assert
            Assert.False(result);
        }

        [Fact(DisplayName = "IsEncrypted should return false for empty string")]
        [Trait("IsEncrypted", "Validation")]
        public void IsEncrypted_ShouldReturnFalse_ForEmptyString()
        {
            // Arrange
            var options = Options.Create(new EncryptionSettings { Key = "test-encryption-key-for-testing" });
            var service = new AesGcmEncryptionService(options);

            // Act
            var result = service.IsEncrypted(string.Empty);

            // Assert
            Assert.False(result);
        }

        [Fact(DisplayName = "IsEncrypted should return true for minimum valid length")]
        [Trait("IsEncrypted", "Validation")]
        public void IsEncrypted_ShouldReturnTrue_ForMinimumValidLength()
        {
            // Arrange
            var options = Options.Create(new EncryptionSettings { Key = "test-encryption-key-for-testing" });
            var service = new AesGcmEncryptionService(options);
            // Create a base64 string with exactly NonceSize + TagSize bytes (28 bytes)
            var minValidBytes = new byte[28];
            var minValidBase64 = Convert.ToBase64String(minValidBytes);

            // Act
            var result = service.IsEncrypted(minValidBase64);

            // Assert
            Assert.True(result);
        }

        [Fact(DisplayName = "IsEncrypted should return false for special characters")]
        [Trait("IsEncrypted", "Validation")]
        public void IsEncrypted_ShouldReturnFalse_ForSpecialCharacters()
        {
            // Arrange
            var options = Options.Create(new EncryptionSettings { Key = "test-encryption-key-for-testing" });
            var service = new AesGcmEncryptionService(options);
            var specialChars = "!@#$%^&*()";

            // Act
            var result = service.IsEncrypted(specialChars);

            // Assert
            Assert.False(result);
        }
    }
}
