using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using WoopiAiHub.Application.Validation;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Domain.Utils;
using WoopiAiHub.Application.Utils;
using Xunit;

namespace WoopiAiHub.UnitTests.Validation
{
    public class TenantBindingValidatorTests
    {
        private readonly Mock<IUserTenantAccessCacheServices> _userTenantAccessMock;
        private readonly TenantBindingValidator _validator;

        public TenantBindingValidatorTests()
        {
            _userTenantAccessMock = new Mock<IUserTenantAccessCacheServices>();
            _validator = new TenantBindingValidator(
                _userTenantAccessMock.Object,
                Mock.Of<ILogger<TenantBindingValidator>>());
        }

        [Fact(DisplayName = "Returns true for unauthenticated request")]
        [Trait("TryValidateRequestBindingAsync", "Success")]
        public async Task TryValidateRequestBindingAsync_Unauthenticated_ReturnsTrue()
        {
            // Arrange
            var context = CreateHttpContext(isAuthenticated: false, headerTenant: "tenant-a", claimTenant: null);

            // Act
            var result = await _validator.TryValidateRequestBindingAsync(context);

            // Assert
            Assert.True(result);
            _userTenantAccessMock.Verify(
                s => s.IsTenantAllowedForUserAsync(It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact(DisplayName = "Returns false when claim exists without header")]
        [Trait("TryValidateRequestBindingAsync", "Fail")]
        public async Task TryValidateRequestBindingAsync_ClaimWithoutHeader_ReturnsFalse()
        {
            // Arrange
            var context = CreateHttpContext(isAuthenticated: true, headerTenant: null, claimTenant: "tenant-a");

            // Act
            var result = await _validator.TryValidateRequestBindingAsync(context);

            // Assert
            Assert.False(result);
        }

        [Fact(DisplayName = "Returns true for SignalR hub when claim exists without header")]
        [Trait("TryValidateRequestBindingAsync", "Success")]
        public async Task TryValidateRequestBindingAsync_HubClaimWithoutHeader_ReturnsTrue()
        {
            // Arrange
            var context = CreateHttpContext(
                isAuthenticated: true,
                headerTenant: null,
                claimTenant: "tenant-a",
                path: HubRoutePaths.NotificationsHub);

            // Act
            var result = await _validator.TryValidateRequestBindingAsync(context);

            // Assert
            Assert.True(result);
        }

        [Fact(DisplayName = "Returns true when header matches claim without marketplace call")]
        [Trait("TryValidateRequestBindingAsync", "Success")]
        public async Task TryValidateRequestBindingAsync_HeaderMatchesClaim_ReturnsTrue()
        {
            // Arrange
            var context = CreateHttpContext(
                isAuthenticated: true,
                headerTenant: "tenant-a",
                claimTenant: "tenant-a",
                email: "user@test.com");

            // Act
            var result = await _validator.TryValidateRequestBindingAsync(context);

            // Assert
            Assert.True(result);
            _userTenantAccessMock.Verify(
                s => s.IsTenantAllowedForUserAsync(It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact(DisplayName = "Returns true when header differs from claim but tenant is allowed")]
        [Trait("TryValidateRequestBindingAsync", "Success")]
        public async Task TryValidateRequestBindingAsync_HeaderInAllowedList_ReturnsTrue()
        {
            // Arrange
            var context = CreateHttpContext(
                isAuthenticated: true,
                headerTenant: "tenant-b",
                claimTenant: "tenant-a",
                email: "user@test.com");

            _userTenantAccessMock
                .Setup(s => s.IsTenantAllowedForUserAsync("user@test.com", "tenant-b"))
                .ReturnsAsync(true);

            // Act
            var result = await _validator.TryValidateRequestBindingAsync(context);

            // Assert
            Assert.True(result);
        }

        [Fact(DisplayName = "Returns false when header differs from claim and tenant is not allowed")]
        [Trait("TryValidateRequestBindingAsync", "Fail")]
        public async Task TryValidateRequestBindingAsync_HeaderNotAllowed_ReturnsFalse()
        {
            // Arrange
            var context = CreateHttpContext(
                isAuthenticated: true,
                headerTenant: "tenant-b",
                claimTenant: "tenant-a",
                email: "user@test.com");

            _userTenantAccessMock
                .Setup(s => s.IsTenantAllowedForUserAsync("user@test.com", "tenant-b"))
                .ReturnsAsync(false);

            // Act
            var result = await _validator.TryValidateRequestBindingAsync(context);

            // Assert
            Assert.False(result);
        }

        [Fact(DisplayName = "Returns false when marketplace lookup fails")]
        [Trait("TryValidateRequestBindingAsync", "Fail")]
        public async Task TryValidateRequestBindingAsync_MarketplaceFailure_ReturnsFalse()
        {
            // Arrange
            var context = CreateHttpContext(
                isAuthenticated: true,
                headerTenant: "tenant-b",
                claimTenant: "tenant-a",
                email: "user@test.com");

            _userTenantAccessMock
                .Setup(s => s.IsTenantAllowedForUserAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("marketplace down"));

            // Act
            var result = await _validator.TryValidateRequestBindingAsync(context);

            // Assert
            Assert.False(result);
        }

        [Fact(DisplayName = "FindAndValidateTenant returns name when tenant is ready")]
        [Trait("FindAndValidateTenant", "Success")]
        public void FindAndValidateTenant_ValidTenant_ReturnsName()
        {
            // Arrange
            var tenants = new List<TenantAccessDto> { new("Tenant1", true) };

            // Act
            var result = _validator.FindAndValidateTenant("Tenant1", tenants);

            // Assert
            Assert.Equal("Tenant1", result);
        }

        [Fact(DisplayName = "FindAndValidateTenant throws when tenant is missing")]
        [Trait("FindAndValidateTenant", "Fail")]
        public void FindAndValidateTenant_TenantNotFound_ThrowsAppException()
        {
            // Arrange
            var tenants = new List<TenantAccessDto> { new("Tenant1", true) };

            // Act / Assert
            var ex = Assert.Throws<AppException>(() =>
                _validator.FindAndValidateTenant("Unknown", tenants));

            Assert.Equal("Tenant not found", ex.Message);
        }

        [Fact(DisplayName = "FindAndValidateTenant throws when database is not ready")]
        [Trait("FindAndValidateTenant", "Fail")]
        public void FindAndValidateTenant_DatabaseNotReady_ThrowsAppException()
        {
            // Arrange
            var tenants = new List<TenantAccessDto> { new("Tenant1", false) };

            // Act / Assert
            var ex = Assert.Throws<AppException>(() =>
                _validator.FindAndValidateTenant("Tenant1", tenants));

            Assert.Equal(ErrorCode.BusinessWarningOutput, ex.ErrorCode);
        }

        private static DefaultHttpContext CreateHttpContext(
            bool isAuthenticated,
            string? headerTenant,
            string? claimTenant,
            string? email = null,
            string path = "/api/test")
        {
            var context = new DefaultHttpContext();
            context.Request.Path = path;

            if (!string.IsNullOrEmpty(headerTenant))
                context.Request.Headers[HeaderNames.XTenant] = headerTenant;

            if (isAuthenticated)
            {
                var claims = new List<Claim>();
                if (!string.IsNullOrEmpty(claimTenant))
                    claims.Add(new Claim(JwtClaimNames.Tenant, claimTenant));
                if (!string.IsNullOrEmpty(email))
                    claims.Add(new Claim(ClaimTypes.Email, email));

                context.User = new ClaimsPrincipal(
                    new ClaimsIdentity(claims, authenticationType: "Bearer"));
            }

            return context;
        }
    }
}
