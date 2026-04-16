using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Moq;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Domain.Interfaces.Services;
using Xunit;

namespace WoopiAiHub.UnitTests.Services
{
    public class JwtTokenServicesTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly IJwtTokenServices _jwtTokenServices;
        private const string ValidJwtKey = "this-is-a-valid-jwt-key-that-is-long-enough-for-hmacsha256-algorithm";
        private const string ValidIssuer = "http://localhost";
        private const string ValidAudience = "http://localhost";
        private const string ValidUser = "testuser@example.com";
        private const int DefaultExpirationMinutes = 60;

        public JwtTokenServicesTests()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            
            // Setup for GetValue method (non-generic overload that Moq can handle)
            var mockSection = new Mock<IConfigurationSection>();
            mockSection.Setup(x => x.Value).Returns(DefaultExpirationMinutes.ToString());
            _mockConfiguration.Setup(x => x.GetSection("JWT:AccessTokenExpirationMinutes"))
                .Returns(mockSection.Object);

            _jwtTokenServices = new JwtTokenServices(_mockConfiguration.Object);
        }

        [Fact(DisplayName = "Generate token with valid parameters should return a valid token")]
        [Trait("GenerateToken", "Success")]
        public void GenerateTokenWithParameters_WithValidParameters_ShouldReturnValidToken()
        {
            // Arrange
            var expectedUser = ValidUser;

            // Act
            var token = _jwtTokenServices.GenerateTokenWithParameters(
                ValidJwtKey,
                ValidIssuer,
                ValidAudience,
                expectedUser);

            // Assert
            Assert.NotNull(token);
            Assert.NotEmpty(token);
            Assert.IsType<string>(token);
        }


        [Fact(DisplayName = "Generate token with valid parameters should return a valid token and contain clains")]
        [Trait("GenerateToken", "Success")]
        public void GenerateTokenWithParameters_WithValidToken_ShouldContainCorrectClaims()
        {
            // Arrange
            var expectedUser = ValidUser;

            // Act
            var token = _jwtTokenServices.GenerateTokenWithParameters(
                ValidJwtKey,
                ValidIssuer,
                ValidAudience,
                expectedUser);

            // Assert - Decode and validate token
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

            Assert.NotNull(jwtToken);
            Assert.Equal(ValidIssuer, jwtToken.Issuer);
            Assert.Equal(ValidAudience, jwtToken.Audiences.First());
            
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            Assert.NotNull(userIdClaim);
            Assert.Equal(expectedUser, userIdClaim.Value);

            var jtiClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti);
            Assert.NotNull(jtiClaim);
            Assert.NotEmpty(jtiClaim.Value);
        }


        [Fact(DisplayName = "Generate token with valid parameters  and expire time should expire in determined period")]
        [Trait("GenerateToken", "Success")]
        public void GenerateTokenWithParameters_WithDefaultExpiration_ShouldExpireInConfiguredMinutes()
        {
            // Arrange
            var expectedExpirationMinutes = DefaultExpirationMinutes;
            var beforeTokenGeneration = DateTime.UtcNow;

            // Act
            var token = _jwtTokenServices.GenerateTokenWithParameters(
                ValidJwtKey,
                ValidIssuer,
                ValidAudience,
                ValidUser);

            var afterTokenGeneration = DateTime.UtcNow;

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

            Assert.NotNull(jwtToken);
            Assert.NotNull(jwtToken.ValidTo);

            var expectedExpiration = beforeTokenGeneration.AddMinutes(expectedExpirationMinutes);
            // Allow a 5-second tolerance for execution time
            Assert.True(jwtToken.ValidTo >= expectedExpiration.AddSeconds(-5));
            Assert.True(jwtToken.ValidTo <= afterTokenGeneration.AddMinutes(expectedExpirationMinutes).AddSeconds(5));
        }

        [Fact(DisplayName = "Generate token with valid parameters and custom expire time should expire in determined period")]
        [Trait("GenerateToken", "Success")]
        public void GenerateTokenWithParameters_WithCustomExpiration_ShouldExpireInCustomMinutes()
        {
            // Arrange
            var customExpirationMinutes = 120;
            var beforeTokenGeneration = DateTime.UtcNow;

            // Act
            var token = _jwtTokenServices.GenerateTokenWithParameters(
                ValidJwtKey,
                ValidIssuer,
                ValidAudience,
                ValidUser,
                customExpirationMinutes);

            var afterTokenGeneration = DateTime.UtcNow;

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

            Assert.NotNull(jwtToken);
            Assert.NotNull(jwtToken.ValidTo);

            var expectedExpiration = beforeTokenGeneration.AddMinutes(customExpirationMinutes);
            // Allow a 5-second tolerance for execution time
            Assert.True(jwtToken.ValidTo >= expectedExpiration.AddSeconds(-5));
            Assert.True(jwtToken.ValidTo <= afterTokenGeneration.AddMinutes(customExpirationMinutes).AddSeconds(5));
        }

        [Fact(DisplayName = "Generate token with JWT key null thrown a exception")]
        [Trait("GenerateToken", "Fail")]
        public void GenerateTokenWithParameters_WithNullJwtKey_ShouldThrowArgumentException()
        {
            // Arrange
            string nullKey = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                _jwtTokenServices.GenerateTokenWithParameters(
                    nullKey,
                    ValidIssuer,
                    ValidAudience,
                    ValidUser));

            Assert.Equal("JWT key is not configured.", exception.Message);
        }

        [Fact(DisplayName = "Generate token with different users should create different tokens")]
        [Trait("GenerateToken", "Success")]
        public void GenerateTokenWithParameters_WithDifferentUsers_ShouldGenerateDifferentTokens()
        {
            // Arrange
            var user1 = "user1@example.com";
            var user2 = "user2@example.com";

            // Act
            var token1 = _jwtTokenServices.GenerateTokenWithParameters(
                ValidJwtKey,
                ValidIssuer,
                ValidAudience,
                user1);

            var token2 = _jwtTokenServices.GenerateTokenWithParameters(
                ValidJwtKey,
                ValidIssuer,
                ValidAudience,
                user2);

            // Assert
            Assert.NotEqual(token1, token2);

            var handler = new JwtSecurityTokenHandler();
            var jwtToken1 = handler.ReadToken(token1) as JwtSecurityToken;
            var jwtToken2 = handler.ReadToken(token2) as JwtSecurityToken;

            var userClaim1 = jwtToken1?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var userClaim2 = jwtToken2?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            Assert.Equal(user1, userClaim1);
            Assert.Equal(user2, userClaim2);
        }

        [Fact(DisplayName = "Generate token with the same parameters executed in a row should create different tokens")]
        [Trait("GenerateToken", "Success")]
        public void GenerateTokenWithParameters_ShouldGenerateUniqueJtiForEachToken()
        {
            // Arrange & Act
            var token1 = _jwtTokenServices.GenerateTokenWithParameters(
                ValidJwtKey,
                ValidIssuer,
                ValidAudience,
                ValidUser);

            var token2 = _jwtTokenServices.GenerateTokenWithParameters(
                ValidJwtKey,
                ValidIssuer,
                ValidAudience,
                ValidUser);

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwtToken1 = handler.ReadToken(token1) as JwtSecurityToken;
            var jwtToken2 = handler.ReadToken(token2) as JwtSecurityToken;

            var jti1 = jwtToken1?.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
            var jti2 = jwtToken2?.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;

            Assert.NotNull(jti1);
            Assert.NotNull(jti2);
            Assert.NotEqual(jti1, jti2);
        }
    }
}