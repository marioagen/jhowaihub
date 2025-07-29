using Xunit;
using WoopiAiHub.Application.Utils;

namespace WoopiAiHub.UnitTests.Services
{
    public class Argon2PasswordHasherTests
    {
        private readonly Argon2PasswordHasher _passwordHasher;

        public Argon2PasswordHasherTests()
        {
            _passwordHasher = new Argon2PasswordHasher();
        }

        [Fact(DisplayName = "Hash should return byte array when valid password and salt are provided")]
        [Trait("Hash", "Success")]
        public void Hash_ShouldReturnByteArray_WhenValidPasswordAndSaltAreProvided()
        {
            // Arrange
            const string password = "TestPassword123!";
            var salt = _passwordHasher.GenerateSalt();

            // Act
            var result = _passwordHasher.Hash(password, salt);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(32, result.Length);
            Assert.IsType<byte[]>(result);
        }

        [Fact(DisplayName = "Hash should return consistent result when same password and salt are used")]
        [Trait("Hash", "Success")]
        public void Hash_ShouldReturnConsistentResult_WhenSamePasswordAndSaltAreUsed()
        {
            // Arrange
            const string password = "TestPassword123!";
            var salt = _passwordHasher.GenerateSalt();

            // Act
            var hash1 = _passwordHasher.Hash(password, salt);
            var hash2 = _passwordHasher.Hash(password, salt);

            // Assert
            Assert.Equal(hash1, hash2);
        }

        [Fact(DisplayName = "Hash should return different results when different salts are used")]
        [Trait("Hash", "Success")]
        public void Hash_ShouldReturnDifferentResults_WhenDifferentSaltsAreUsed()
        {
            // Arrange
            const string password = "TestPassword123!";
            var salt1 = _passwordHasher.GenerateSalt();
            var salt2 = _passwordHasher.GenerateSalt();

            // Act
            var hash1 = _passwordHasher.Hash(password, salt1);
            var hash2 = _passwordHasher.Hash(password, salt2);

            // Assert
            Assert.NotEqual(hash1, hash2);
        }

        [Fact(DisplayName = "Hash should return different results when different passwords are used")]
        [Trait("Hash", "Success")]
        public void Hash_ShouldReturnDifferentResults_WhenDifferentPasswordsAreUsed()
        {
            // Arrange
            const string password1 = "TestPassword123!";
            const string password2 = "DifferentPassword456@";
            var salt = _passwordHasher.GenerateSalt();

            // Act
            var hash1 = _passwordHasher.Hash(password1, salt);
            var hash2 = _passwordHasher.Hash(password2, salt);

            // Assert
            Assert.NotEqual(hash1, hash2);
        }

        [Fact(DisplayName = "Hash should throw ArgumentException when password is null")]
        [Trait("Hash", "Exception")]
        public void Hash_ShouldThrowArgumentException_WhenPasswordIsNull()
        {
            // Arrange
            string password = null;
            var salt = _passwordHasher.GenerateSalt();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _passwordHasher.Hash(password, salt));
            Assert.Equal("Password cannot be null or empty. (Parameter 'password')", exception.Message);
        }

        [Fact(DisplayName = "Hash should throw ArgumentException when password is empty")]
        [Trait("Hash", "Exception")]
        public void Hash_ShouldThrowArgumentException_WhenPasswordIsEmpty()
        {
            // Arrange
            const string password = "";
            var salt = _passwordHasher.GenerateSalt();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _passwordHasher.Hash(password, salt));
            Assert.Equal("Password cannot be null or empty. (Parameter 'password')", exception.Message);
        }

        [Fact(DisplayName = "Hash should throw ArgumentException when salt is null")]
        [Trait("Hash", "Exception")]
        public void Hash_ShouldThrowArgumentException_WhenSaltIsNull()
        {
            // Arrange
            const string password = "TestPassword123!";
            byte[] salt = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _passwordHasher.Hash(password, salt));
            Assert.Equal("Salt cannot be null or empty. (Parameter 'saltBytes')", exception.Message);
        }

        [Fact(DisplayName = "Hash should throw ArgumentException when salt is empty")]
        [Trait("Hash", "Exception")]
        public void Hash_ShouldThrowArgumentException_WhenSaltIsEmpty()
        {
            // Arrange
            const string password = "TestPassword123!";
            var salt = new byte[0];

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _passwordHasher.Hash(password, salt));
            Assert.Equal("Salt cannot be null or empty. (Parameter 'saltBytes')", exception.Message);
        }

        [Fact(DisplayName = "Verify should return true when password matches stored hash")]
        [Trait("Verify", "Success")]
        public void Verify_ShouldReturnTrue_WhenPasswordMatchesStoredHash()
        {
            // Arrange
            const string password = "TestPassword123!";
            var salt = _passwordHasher.GenerateSalt();
            var hash = _passwordHasher.Hash(password, salt);

            // Act
            var result = _passwordHasher.Verify(password, hash, salt);

            // Assert
            Assert.True(result);
        }

        [Fact(DisplayName = "Verify should return false when password does not match stored hash")]
        [Trait("Verify", "Success")]
        public void Verify_ShouldReturnFalse_WhenPasswordDoesNotMatchStoredHash()
        {
            // Arrange
            const string originalPassword = "TestPassword123!";
            const string wrongPassword = "WrongPassword456@";
            var salt = _passwordHasher.GenerateSalt();
            var hash = _passwordHasher.Hash(originalPassword, salt);

            // Act
            var result = _passwordHasher.Verify(wrongPassword, hash, salt);

            // Assert
            Assert.False(result);
        }

        [Fact(DisplayName = "Verify should return false when password is null")]
        [Trait("Verify", "Validation")]
        public void Verify_ShouldReturnFalse_WhenPasswordIsNull()
        {
            // Arrange
            string password = null;
            var salt = _passwordHasher.GenerateSalt();
            var hash = _passwordHasher.Hash("TestPassword123!", salt);

            // Act
            var result = _passwordHasher.Verify(password, hash, salt);

            // Assert
            Assert.False(result);
        }

        [Fact(DisplayName = "Verify should return false when password is empty")]
        [Trait("Verify", "Validation")]
        public void Verify_ShouldReturnFalse_WhenPasswordIsEmpty()
        {
            // Arrange
            const string password = "";
            var salt = _passwordHasher.GenerateSalt();
            var hash = _passwordHasher.Hash("TestPassword123!", salt);

            // Act
            var result = _passwordHasher.Verify(password, hash, salt);

            // Assert
            Assert.False(result);
        }

        [Fact(DisplayName = "Verify should return false when stored hash is null")]
        [Trait("Verify", "Validation")]
        public void Verify_ShouldReturnFalse_WhenStoredHashIsNull()
        {
            // Arrange
            const string password = "TestPassword123!";
            var salt = _passwordHasher.GenerateSalt();
            byte[] hash = null;

            // Act
            var result = _passwordHasher.Verify(password, hash, salt);

            // Assert
            Assert.False(result);
        }

        [Fact(DisplayName = "Verify should return false when stored salt is null")]
        [Trait("Verify", "Validation")]
        public void Verify_ShouldReturnFalse_WhenStoredSaltIsNull()
        {
            // Arrange
            const string password = "TestPassword123!";
            var salt = _passwordHasher.GenerateSalt();
            var hash = _passwordHasher.Hash(password, salt);
            byte[] nullSalt = null;

            // Act
            var result = _passwordHasher.Verify(password, hash, nullSalt);

            // Assert
            Assert.False(result);
        }

        [Fact(DisplayName = "Verify should return false when wrong salt is used")]
        [Trait("Verify", "Success")]
        public void Verify_ShouldReturnFalse_WhenWrongSaltIsUsed()
        {
            // Arrange
            const string password = "TestPassword123!";
            var correctSalt = _passwordHasher.GenerateSalt();
            var wrongSalt = _passwordHasher.GenerateSalt();
            var hash = _passwordHasher.Hash(password, correctSalt);

            // Act
            var result = _passwordHasher.Verify(password, hash, wrongSalt);

            // Assert
            Assert.False(result);
        }

        [Fact(DisplayName = "GenerateSalt should return byte array with default length")]
        [Trait("GenerateSalt", "Success")]
        public void GenerateSalt_ShouldReturnByteArrayWithDefaultLength_WhenNoLengthIsSpecified()
        {
            // Act
            var result = _passwordHasher.GenerateSalt();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(16, result.Length);
            Assert.IsType<byte[]>(result);
        }

        [Fact(DisplayName = "GenerateSalt should return byte array with specified length")]
        [Trait("GenerateSalt", "Success")]
        public void GenerateSalt_ShouldReturnByteArrayWithSpecifiedLength_WhenLengthIsProvided()
        {
            // Arrange
            const int length = 32;

            // Act
            var result = _passwordHasher.GenerateSalt(length);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(length, result.Length);
            Assert.IsType<byte[]>(result);
        }

        [Fact(DisplayName = "GenerateSalt should return different salts on multiple calls")]
        [Trait("GenerateSalt", "Success")]
        public void GenerateSalt_ShouldReturnDifferentSalts_OnMultipleCalls()
        {
            // Act
            var salt1 = _passwordHasher.GenerateSalt();
            var salt2 = _passwordHasher.GenerateSalt();

            // Assert
            Assert.NotEqual(salt1, salt2);
        }

        [Fact(DisplayName = "GenerateSalt should throw ArgumentException when length is zero")]
        [Trait("GenerateSalt", "Exception")]
        public void GenerateSalt_ShouldThrowArgumentException_WhenLengthIsZero()
        {
            // Arrange
            const int length = 0;

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _passwordHasher.GenerateSalt(length));
            Assert.Equal("Salt length must be greater than zero. (Parameter 'length')", exception.Message);
        }

        [Fact(DisplayName = "GenerateSalt should throw ArgumentException when length is negative")]
        [Trait("GenerateSalt", "Exception")]
        public void GenerateSalt_ShouldThrowArgumentException_WhenLengthIsNegative()
        {
            // Arrange
            const int length = -1;

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _passwordHasher.GenerateSalt(length));
            Assert.Equal("Salt length must be greater than zero. (Parameter 'length')", exception.Message);
        }

        [Fact(DisplayName = "GenerateSalt should return non-zero bytes")]
        [Trait("GenerateSalt", "Success")]
        public void GenerateSalt_ShouldReturnNonZeroBytes_WhenCalled()
        {
            // Act
            var result = _passwordHasher.GenerateSalt();

            // Assert
            Assert.NotNull(result);
            Assert.Contains(result, b => b != 0);
        }

        [Fact(DisplayName = "Hash and Verify integration should work correctly for multiple scenarios")]
        [Trait("Integration", "Success")]
        public void HashAndVerifyIntegration_ShouldWorkCorrectly_ForMultipleScenarios()
        {
            // Arrange
            var testCases = new[]
            {
                "SimplePassword",
                "P@ssw0rd123!",
                "VeryLongPasswordWithSpecialCharacters!@#$%^&*()_+{}[]|\\:;\"'<>,.?/~`",
                "短密码", // Chinese characters
                "пароль", // Cyrillic characters
                "🔐🗝️🔑" // Emojis
            };

            foreach (var password in testCases)
            {
                // Act
                var salt = _passwordHasher.GenerateSalt();
                var hash = _passwordHasher.Hash(password, salt);
                var isValid = _passwordHasher.Verify(password, hash, salt);
                var isInvalid = _passwordHasher.Verify(password + "wrong", hash, salt);

                // Assert
                Assert.True(isValid, $"Verification should succeed for password: {password}");
                Assert.False(isInvalid, $"Verification should fail for wrong password: {password}");
                Assert.Equal(32, hash.Length);
                Assert.Equal(16, salt.Length);
            }
        }

        [Fact(DisplayName = "Password hashing should be deterministic with same inputs")]
        [Trait("Integration", "Success")]
        public void PasswordHashing_ShouldBeDeterministic_WithSameInputs()
        {
            // Arrange
            const string password = "TestPassword123!";
            var salt = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };

            // Act
            var hash1 = _passwordHasher.Hash(password, salt);
            var hash2 = _passwordHasher.Hash(password, salt);
            var hash3 = _passwordHasher.Hash(password, salt);

            // Assert
            Assert.Equal(hash1, hash2);
            Assert.Equal(hash2, hash3);
            Assert.Equal(hash1, hash3);
        }

        [Fact(DisplayName = "Password hashing should handle edge case passwords")]
        [Trait("Integration", "EdgeCase")]
        public void PasswordHashing_ShouldHandleEdgeCasePasswords_Correctly()
        {
            // Arrange
            var edgeCasePasswords = new[]
            {
                "a", // Single character
                new string('x', 1000), // Very long password
                " ", // Single space
                "\t\n\r", // Whitespace characters
                "null", // String that looks like null
                "0", // Single digit
                "true", // Boolean-like string
                "false" // Boolean-like string
            };

            foreach (var password in edgeCasePasswords)
            {
                // Act
                var salt = _passwordHasher.GenerateSalt();
                var hash = _passwordHasher.Hash(password, salt);
                var isValid = _passwordHasher.Verify(password, hash, salt);

                // Assert
                Assert.True(isValid, $"Edge case password should hash and verify correctly: '{password}'");
                Assert.Equal(32, hash.Length);
            }
        }
    }
}
