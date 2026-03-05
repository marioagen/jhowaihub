using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Moq;
using WoopiAiHub.Application.Utils;
using Xunit;

namespace WoopiAiHub.UnitTests.Utils
{
    public class CurrentUserServiceTests
    {
        [Fact(DisplayName = "Constructor should throw ArgumentNullException when httpContextAccessor is null")]
        [Trait("CurrentUserService", "Constructor")]
        public void Constructor_WhenHttpContextAccessorIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new CurrentUserService(null!));
        }

        [Fact(DisplayName = "When HttpContext is null, IsAuthenticated should be false")]
        [Trait("CurrentUserService", "IsAuthenticated")]
        public void WhenHttpContextIsNull_IsAuthenticated_ReturnsFalse()
        {
            var accessorMock = new Mock<IHttpContextAccessor>();
            accessorMock.Setup(a => a.HttpContext).Returns((HttpContext?)null);

            var service = new CurrentUserService(accessorMock.Object);

            Assert.False(service.IsAuthenticated);
        }

        [Fact(DisplayName = "When User is null, IsAuthenticated should be false")]
        [Trait("CurrentUserService", "IsAuthenticated")]
        public void WhenUserIsNull_IsAuthenticated_ReturnsFalse()
        {
            var contextMock = new Mock<HttpContext>();
            contextMock.Setup(c => c.User).Returns(() => null!);
            var accessorMock = new Mock<IHttpContextAccessor>();
            accessorMock.Setup(a => a.HttpContext).Returns(contextMock.Object);

            var service = new CurrentUserService(accessorMock.Object);

            Assert.False(service.IsAuthenticated);
        }

        [Fact(DisplayName = "When User is authenticated, IsAuthenticated should be true")]
        [Trait("CurrentUserService", "IsAuthenticated")]
        public void WhenUserIsAuthenticated_IsAuthenticated_ReturnsTrue()
        {
            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "user@test.com") }, "Bearer");
            var user = new ClaimsPrincipal(identity);
            var contextMock = new Mock<HttpContext>();
            contextMock.Setup(c => c.User).Returns(user);
            var accessorMock = new Mock<IHttpContextAccessor>();
            accessorMock.Setup(a => a.HttpContext).Returns(contextMock.Object);

            var service = new CurrentUserService(accessorMock.Object);

            Assert.True(service.IsAuthenticated);
        }

        [Fact(DisplayName = "Id should return Guid from userId claim when valid")]
        [Trait("CurrentUserService", "Id")]
        public void Id_WhenUserIdClaimIsValidGuid_ReturnsGuid()
        {
            var expectedId = Guid.NewGuid();
            var identity = new ClaimsIdentity(
                new[] { new Claim("userId", expectedId.ToString()) },
                "Bearer");
            var user = new ClaimsPrincipal(identity);
            var contextMock = new Mock<HttpContext>();
            contextMock.Setup(c => c.User).Returns(user);
            var accessorMock = new Mock<IHttpContextAccessor>();
            accessorMock.Setup(a => a.HttpContext).Returns(contextMock.Object);

            var service = new CurrentUserService(accessorMock.Object);

            Assert.Equal(expectedId, service.Id);
        }

        [Fact(DisplayName = "Id should return null when userId claim is missing")]
        [Trait("CurrentUserService", "Id")]
        public void Id_WhenUserIdClaimIsMissing_ReturnsNull()
        {
            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Email, "a@b.com") }, "Bearer");
            var user = new ClaimsPrincipal(identity);
            var contextMock = new Mock<HttpContext>();
            contextMock.Setup(c => c.User).Returns(user);
            var accessorMock = new Mock<IHttpContextAccessor>();
            accessorMock.Setup(a => a.HttpContext).Returns(contextMock.Object);

            var service = new CurrentUserService(accessorMock.Object);

            Assert.Null(service.Id);
        }

        [Fact(DisplayName = "Id should return null when userId claim is not a valid Guid")]
        [Trait("CurrentUserService", "Id")]
        public void Id_WhenUserIdClaimIsInvalidGuid_ReturnsNull()
        {
            var identity = new ClaimsIdentity(new[] { new Claim("userId", "not-a-guid") }, "Bearer");
            var user = new ClaimsPrincipal(identity);
            var contextMock = new Mock<HttpContext>();
            contextMock.Setup(c => c.User).Returns(user);
            var accessorMock = new Mock<IHttpContextAccessor>();
            accessorMock.Setup(a => a.HttpContext).Returns(contextMock.Object);

            var service = new CurrentUserService(accessorMock.Object);

            Assert.Null(service.Id);
        }

        [Fact(DisplayName = "UserId should return NameIdentifier then Name then sub")]
        [Trait("CurrentUserService", "UserId")]
        public void UserId_ReturnsNameIdentifier_WhenPresent()
        {
            var identity = new ClaimsIdentity(
                new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, "name-id-123"),
                    new Claim(ClaimTypes.Name, "name-fallback"),
                    new Claim("sub", "sub-fallback")
                },
                "Bearer");
            var user = new ClaimsPrincipal(identity);
            var contextMock = new Mock<HttpContext>();
            contextMock.Setup(c => c.User).Returns(user);
            var accessorMock = new Mock<IHttpContextAccessor>();
            accessorMock.Setup(a => a.HttpContext).Returns(contextMock.Object);

            var service = new CurrentUserService(accessorMock.Object);

            Assert.Equal("name-id-123", service.UserId);
        }

        [Fact(DisplayName = "UserId should fallback to Name when NameIdentifier missing")]
        [Trait("CurrentUserService", "UserId")]
        public void UserId_FallbackToName_WhenNameIdentifierMissing()
        {
            var identity = new ClaimsIdentity(
                new[]
                {
                    new Claim(ClaimTypes.Name, "name-value"),
                    new Claim("sub", "sub-fallback")
                },
                "Bearer");
            var user = new ClaimsPrincipal(identity);
            var contextMock = new Mock<HttpContext>();
            contextMock.Setup(c => c.User).Returns(user);
            var accessorMock = new Mock<IHttpContextAccessor>();
            accessorMock.Setup(a => a.HttpContext).Returns(contextMock.Object);

            var service = new CurrentUserService(accessorMock.Object);

            Assert.Equal("name-value", service.UserId);
        }

        [Fact(DisplayName = "UserId should fallback to sub when NameIdentifier and Name missing")]
        [Trait("CurrentUserService", "UserId")]
        public void UserId_FallbackToSub_WhenNameIdentifierAndNameMissing()
        {
            var identity = new ClaimsIdentity(
                new[] { new Claim("sub", "sub-value") },
                "Bearer");
            var user = new ClaimsPrincipal(identity);
            var contextMock = new Mock<HttpContext>();
            contextMock.Setup(c => c.User).Returns(user);
            var accessorMock = new Mock<IHttpContextAccessor>();
            accessorMock.Setup(a => a.HttpContext).Returns(contextMock.Object);

            var service = new CurrentUserService(accessorMock.Object);

            Assert.Equal("sub-value", service.UserId);
        }

        [Fact(DisplayName = "Email should return Email claim then email then sub")]
        [Trait("CurrentUserService", "Email")]
        public void Email_ReturnsEmailClaim_WhenPresent()
        {
            var identity = new ClaimsIdentity(
                new[]
                {
                    new Claim(ClaimTypes.Email, "email@test.com"),
                    new Claim("email", "custom-email"),
                    new Claim("sub", "sub-as-email")
                },
                "Bearer");
            var user = new ClaimsPrincipal(identity);
            var contextMock = new Mock<HttpContext>();
            contextMock.Setup(c => c.User).Returns(user);
            var accessorMock = new Mock<IHttpContextAccessor>();
            accessorMock.Setup(a => a.HttpContext).Returns(contextMock.Object);

            var service = new CurrentUserService(accessorMock.Object);

            Assert.Equal("email@test.com", service.Email);
        }

        [Fact(DisplayName = "Email should fallback to sub when Email and email missing")]
        [Trait("CurrentUserService", "Email")]
        public void Email_FallbackToSub_WhenEmailClaimsMissing()
        {
            var identity = new ClaimsIdentity(
                new[] { new Claim("sub", "user@example.com") },
                "Bearer");
            var user = new ClaimsPrincipal(identity);
            var contextMock = new Mock<HttpContext>();
            contextMock.Setup(c => c.User).Returns(user);
            var accessorMock = new Mock<IHttpContextAccessor>();
            accessorMock.Setup(a => a.HttpContext).Returns(contextMock.Object);

            var service = new CurrentUserService(accessorMock.Object);

            Assert.Equal("user@example.com", service.Email);
        }

        [Fact(DisplayName = "IsAdmin should return true when isAdmin claim is true")]
        [Trait("CurrentUserService", "IsAdmin")]
        public void IsAdmin_WhenIsAdminClaimIsTrue_ReturnsTrue()
        {
            var identity = new ClaimsIdentity(
                new[] { new Claim("isAdmin", "true") },
                "Bearer");
            var user = new ClaimsPrincipal(identity);
            var contextMock = new Mock<HttpContext>();
            contextMock.Setup(c => c.User).Returns(user);
            var accessorMock = new Mock<IHttpContextAccessor>();
            accessorMock.Setup(a => a.HttpContext).Returns(contextMock.Object);

            var service = new CurrentUserService(accessorMock.Object);

            Assert.True(service.IsAdmin);
        }

        [Fact(DisplayName = "IsAdmin should return true when isAdmin claim is TRUE (case insensitive)")]
        [Trait("CurrentUserService", "IsAdmin")]
        public void IsAdmin_WhenIsAdminClaimIsTrueUpperCase_ReturnsTrue()
        {
            var identity = new ClaimsIdentity(
                new[] { new Claim("isAdmin", "TRUE") },
                "Bearer");
            var user = new ClaimsPrincipal(identity);
            var contextMock = new Mock<HttpContext>();
            contextMock.Setup(c => c.User).Returns(user);
            var accessorMock = new Mock<IHttpContextAccessor>();
            accessorMock.Setup(a => a.HttpContext).Returns(contextMock.Object);

            var service = new CurrentUserService(accessorMock.Object);

            Assert.True(service.IsAdmin);
        }

        [Fact(DisplayName = "IsAdmin should return false when isAdmin claim is missing")]
        [Trait("CurrentUserService", "IsAdmin")]
        public void IsAdmin_WhenIsAdminClaimIsMissing_ReturnsFalse()
        {
            var identity = new ClaimsIdentity(Array.Empty<Claim>(), "Bearer");
            var user = new ClaimsPrincipal(identity);
            var contextMock = new Mock<HttpContext>();
            contextMock.Setup(c => c.User).Returns(user);
            var accessorMock = new Mock<IHttpContextAccessor>();
            accessorMock.Setup(a => a.HttpContext).Returns(contextMock.Object);

            var service = new CurrentUserService(accessorMock.Object);

            Assert.False(service.IsAdmin);
        }

        [Fact(DisplayName = "IsAdmin should return false when isAdmin claim is not true")]
        [Trait("CurrentUserService", "IsAdmin")]
        public void IsAdmin_WhenIsAdminClaimIsFalse_ReturnsFalse()
        {
            var identity = new ClaimsIdentity(
                new[] { new Claim("isAdmin", "false") },
                "Bearer");
            var user = new ClaimsPrincipal(identity);
            var contextMock = new Mock<HttpContext>();
            contextMock.Setup(c => c.User).Returns(user);
            var accessorMock = new Mock<IHttpContextAccessor>();
            accessorMock.Setup(a => a.HttpContext).Returns(contextMock.Object);

            var service = new CurrentUserService(accessorMock.Object);

            Assert.False(service.IsAdmin);
        }

        [Fact(DisplayName = "FindClaim should return claim value when present")]
        [Trait("CurrentUserService", "FindClaim")]
        public void FindClaim_WhenClaimExists_ReturnsValue()
        {
            var identity = new ClaimsIdentity(
                new[] { new Claim("customClaim", "customValue") },
                "Bearer");
            var user = new ClaimsPrincipal(identity);
            var contextMock = new Mock<HttpContext>();
            contextMock.Setup(c => c.User).Returns(user);
            var accessorMock = new Mock<IHttpContextAccessor>();
            accessorMock.Setup(a => a.HttpContext).Returns(contextMock.Object);

            var service = new CurrentUserService(accessorMock.Object);

            Assert.Equal("customValue", service.FindClaim("customClaim"));
        }

        [Fact(DisplayName = "FindClaim should return null when claim is missing")]
        [Trait("CurrentUserService", "FindClaim")]
        public void FindClaim_WhenClaimMissing_ReturnsNull()
        {
            var identity = new ClaimsIdentity(Array.Empty<Claim>(), "Bearer");
            var user = new ClaimsPrincipal(identity);
            var contextMock = new Mock<HttpContext>();
            contextMock.Setup(c => c.User).Returns(user);
            var accessorMock = new Mock<IHttpContextAccessor>();
            accessorMock.Setup(a => a.HttpContext).Returns(contextMock.Object);

            var service = new CurrentUserService(accessorMock.Object);

            Assert.Null(service.FindClaim("nonexistent"));
        }

        [Fact(DisplayName = "When HttpContext is null, Id UserId Email IsAdmin and FindClaim should return null or false")]
        [Trait("CurrentUserService", "NoContext")]
        public void WhenHttpContextIsNull_AllUserProperties_ReturnNullOrFalse()
        {
            var accessorMock = new Mock<IHttpContextAccessor>();
            accessorMock.Setup(a => a.HttpContext).Returns((HttpContext?)null);

            var service = new CurrentUserService(accessorMock.Object);

            Assert.False(service.IsAuthenticated);
            Assert.Null(service.Id);
            Assert.Null(service.UserId);
            Assert.Null(service.Email);
            Assert.False(service.IsAdmin);
            Assert.Null(service.FindClaim("any"));
        }
    }
}
