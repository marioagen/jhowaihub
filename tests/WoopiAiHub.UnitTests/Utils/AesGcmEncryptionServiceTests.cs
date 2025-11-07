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
    }
}
