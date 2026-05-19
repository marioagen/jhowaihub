using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Domain.Utils;
using WoopiAiHub.Repository.Middleware;
using Xunit;

namespace WoopiAiHub.UnitTests.Middleware
{
    public class MultiTenantMiddlewareTests
    {
        private const string TemplateConnection = "Server=.;Database=___NEWDB___;";
        private const string TenantName = "tenant-alpha";

        [Fact(DisplayName = "Sets TenantConnection when validator allows request")]
        [Trait("InvokeAsync", "Success")]
        public async Task InvokeAsync_ValidatorAllows_SetsTenantConnection()
        {
            // Arrange
            var (context, configuration, tenantCache, validator, nextCalled) = CreateContext(
                headerTenant: TenantName,
                validatorAllows: true);

            var middleware = new MultiTenant(_ => { nextCalled.Value = true; return Task.CompletedTask; });

            // Act
            await middleware.InvokeAsync(context, configuration, tenantCache.Object, validator.Object);

            // Assert
            Assert.True(nextCalled.Value);
            Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
            Assert.NotNull(context.Items["TenantConnection"]);
            Assert.Contains("TestDB", context.Items["TenantConnection"]!.ToString());
        }

        [Fact(DisplayName = "Returns 403 when validator rejects request")]
        [Trait("InvokeAsync", "Fail")]
        public async Task InvokeAsync_ValidatorRejects_Returns403()
        {
            // Arrange
            var (context, configuration, tenantCache, validator, nextCalled) = CreateContext(
                headerTenant: TenantName,
                validatorAllows: false);

            var middleware = new MultiTenant(_ => { nextCalled.Value = true; return Task.CompletedTask; });

            // Act
            await middleware.InvokeAsync(context, configuration, tenantCache.Object, validator.Object);

            // Assert
            Assert.False(nextCalled.Value);
            Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
            Assert.Null(context.Items["TenantConnection"]);
            await AssertResponseErrorAsync(context);
        }

        [Fact(DisplayName = "Continues without TenantConnection when no header and validator allows")]
        [Trait("InvokeAsync", "Success")]
        public async Task InvokeAsync_NoHeader_DoesNotSetTenantConnection()
        {
            // Arrange
            var (context, configuration, tenantCache, validator, nextCalled) = CreateContext(
                headerTenant: null,
                validatorAllows: true);

            var middleware = new MultiTenant(_ => { nextCalled.Value = true; return Task.CompletedTask; });

            // Act
            await middleware.InvokeAsync(context, configuration, tenantCache.Object, validator.Object);

            // Assert
            Assert.True(nextCalled.Value);
            Assert.Null(context.Items["TenantConnection"]);
        }

        private static (
            HttpContext Context,
            IConfiguration Configuration,
            Mock<ITenantCacheServices> TenantCache,
            Mock<ITenantBindingValidator> Validator,
            StrongBox<bool> NextCalled) CreateContext(
            string? headerTenant,
            bool validatorAllows)
        {
            var context = new DefaultHttpContext
            {
                Response = { Body = new MemoryStream() }
            };
            var nextCalled = new StrongBox<bool>(false);

            if (!string.IsNullOrEmpty(headerTenant))
                context.Request.Headers[HeaderNames.XTenant] = headerTenant;

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

            var validator = new Mock<ITenantBindingValidator>();
            validator
                .Setup(v => v.TryValidateRequestBindingAsync(context, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validatorAllows);

            return (context, configuration, tenantCache, validator, nextCalled);
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
