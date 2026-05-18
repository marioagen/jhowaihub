using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Domain.Utils;
using WoopiAiHub.Repository.Middleware;
using Xunit;

namespace WoopiAiHub.UnitTests.Middleware
{
    public class MultiTenantMiddlewareTests
    {
        private const string TemplateConnection = "Server=.;Database=___NEWDB___;";
        private const string TenantName = "tenant-alpha";
        private const string OtherTenantName = "tenant-beta";

        [Fact(DisplayName = "Sets TenantConnection when authenticated header matches tenant claim")]
        [Trait("InvokeAsync", "Success")]
        public async Task InvokeAsync_AuthenticatedMatchingHeaderAndClaim_SetsTenantConnection()
        {
            // Arrange
            var (context, configuration, tenantCache, nextCalled) = CreateContext(
                isAuthenticated: true,
                headerTenant: TenantName,
                claimTenant: TenantName);

            var middleware = new MultiTenant(_ => { nextCalled.Value = true; return Task.CompletedTask; });

            // Act
            await middleware.InvokeAsync(context, configuration, tenantCache.Object);

            // Assert
            Assert.True(nextCalled.Value);
            Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
            Assert.NotNull(context.Items["TenantConnection"]);
            Assert.Contains("TestDB", context.Items["TenantConnection"]!.ToString());
        }

        [Fact(DisplayName = "Returns 403 when authenticated header tenant mismatches claim")]
        [Trait("InvokeAsync", "Fail")]
        public async Task InvokeAsync_AuthenticatedMismatchedTenant_Returns403()
        {
            // Arrange
            var (context, configuration, tenantCache, nextCalled) = CreateContext(
                isAuthenticated: true,
                headerTenant: OtherTenantName,
                claimTenant: TenantName);

            var middleware = new MultiTenant(_ => { nextCalled.Value = true; return Task.CompletedTask; });

            // Act
            await middleware.InvokeAsync(context, configuration, tenantCache.Object);

            // Assert
            Assert.False(nextCalled.Value);
            Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
            Assert.Null(context.Items["TenantConnection"]);
            await AssertResponseErrorAsync(context);
        }

        [Fact(DisplayName = "Returns 403 when authenticated user has tenant claim but no header")]
        [Trait("InvokeAsync", "Fail")]
        public async Task InvokeAsync_AuthenticatedClaimWithoutHeader_Returns403()
        {
            // Arrange
            var (context, configuration, tenantCache, nextCalled) = CreateContext(
                isAuthenticated: true,
                headerTenant: null,
                claimTenant: TenantName);

            var middleware = new MultiTenant(_ => { nextCalled.Value = true; return Task.CompletedTask; });

            // Act
            await middleware.InvokeAsync(context, configuration, tenantCache.Object);

            // Assert
            Assert.False(nextCalled.Value);
            Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
            Assert.Null(context.Items["TenantConnection"]);
        }

        [Fact(DisplayName = "Returns 403 when authenticated user sends header without tenant claim")]
        [Trait("InvokeAsync", "Fail")]
        public async Task InvokeAsync_AuthenticatedHeaderWithoutClaim_Returns403()
        {
            // Arrange
            var (context, configuration, tenantCache, nextCalled) = CreateContext(
                isAuthenticated: true,
                headerTenant: TenantName,
                claimTenant: null);

            var middleware = new MultiTenant(_ => { nextCalled.Value = true; return Task.CompletedTask; });

            // Act
            await middleware.InvokeAsync(context, configuration, tenantCache.Object);

            // Assert
            Assert.False(nextCalled.Value);
            Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
            Assert.Null(context.Items["TenantConnection"]);
        }

        [Fact(DisplayName = "Sets TenantConnection for unauthenticated request with valid header")]
        [Trait("InvokeAsync", "Success")]
        public async Task InvokeAsync_UnauthenticatedWithValidHeader_SetsTenantConnection()
        {
            // Arrange
            var (context, configuration, tenantCache, nextCalled) = CreateContext(
                isAuthenticated: false,
                headerTenant: TenantName,
                claimTenant: null);

            var middleware = new MultiTenant(_ => { nextCalled.Value = true; return Task.CompletedTask; });

            // Act
            await middleware.InvokeAsync(context, configuration, tenantCache.Object);

            // Assert
            Assert.True(nextCalled.Value);
            Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
            Assert.NotNull(context.Items["TenantConnection"]);
        }

        [Fact(DisplayName = "Continues without TenantConnection when unauthenticated and no header")]
        [Trait("InvokeAsync", "Success")]
        public async Task InvokeAsync_UnauthenticatedWithoutHeader_DoesNotSetTenantConnection()
        {
            // Arrange
            var (context, configuration, tenantCache, nextCalled) = CreateContext(
                isAuthenticated: false,
                headerTenant: null,
                claimTenant: null);

            var middleware = new MultiTenant(_ => { nextCalled.Value = true; return Task.CompletedTask; });

            // Act
            await middleware.InvokeAsync(context, configuration, tenantCache.Object);

            // Assert
            Assert.True(nextCalled.Value);
            Assert.Null(context.Items["TenantConnection"]);
        }

        private static (
            HttpContext Context,
            IConfiguration Configuration,
            Mock<ITenantCacheServices> TenantCache,
            StrongBox<bool> NextCalled) CreateContext(
            bool isAuthenticated,
            string? headerTenant,
            string? claimTenant)
        {
            var context = new DefaultHttpContext
            {
                Response = { Body = new MemoryStream() }
            };
            var nextCalled = new StrongBox<bool>(false);

            if (!string.IsNullOrEmpty(headerTenant))
                context.Request.Headers[HeaderNames.XTenant] = headerTenant;

            if (isAuthenticated)
            {
                var claims = new List<Claim>();
                if (!string.IsNullOrEmpty(claimTenant))
                    claims.Add(new Claim(JwtClaimNames.Tenant, claimTenant));

                context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType: "Bearer"));
            }

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:TemplateConnection"] = TemplateConnection
                })
                .Build();

            var tenantCache = new Mock<ITenantCacheServices>();
            tenantCache
                .Setup(t => t.FindTenantAsync(It.IsAny<string>()))
                .ReturnsAsync((string name) => new TenantInfoDto
                {
                    Name = name,
                    DatabaseName = "TestDB"
                });

            return (context, configuration, tenantCache, nextCalled);
        }

        private static async Task AssertResponseErrorAsync(HttpContext context)
        {
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
            var json = JsonDocument.Parse(body);
            Assert.Equal("Tenant mismatch or missing.", json.RootElement.GetProperty("error").GetString());
        }

        private sealed class StrongBox<T>(T value)
        {
            public T Value { get; set; } = value;
        }
    }
}
