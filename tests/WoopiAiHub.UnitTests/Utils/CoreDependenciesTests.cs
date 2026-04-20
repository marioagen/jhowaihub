using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.Interfaces.Utils;
using Xunit;

namespace WoopiAiHub.UnitTests.Utils
{
    public class CoreDependenciesTests
    {
        [Fact(DisplayName = "Constructor should assign and expose CurrentUserService")]
        [Trait("CoreDependencies", "CurrentUserService")]
        public void Constructor_ShouldExposeInjectedCurrentUserService()
        {
            // Arrange
            var configurationMock = new Mock<IConfiguration>();
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            var currentUserServiceMock = new Mock<ICurrentUserService>();

            // Act
            var coreDependencies = new CoreDependencies(
                configurationMock.Object,
                httpContextAccessorMock.Object,
                currentUserServiceMock.Object);

            // Assert
            Assert.Same(currentUserServiceMock.Object, coreDependencies.CurrentUserService);
        }

        [Fact(DisplayName = "Constructor should assign and expose all dependencies")]
        [Trait("CoreDependencies", "Constructor")]
        public void Constructor_ShouldExposeAllInjectedDependencies()
        {
            // Arrange
            var configurationMock = new Mock<IConfiguration>();
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            var currentUserServiceMock = new Mock<ICurrentUserService>();

            // Act
            var coreDependencies = new CoreDependencies(
                configurationMock.Object,
                httpContextAccessorMock.Object,
                currentUserServiceMock.Object);

            // Assert
            Assert.Same(configurationMock.Object, coreDependencies.Configuration);
            Assert.Same(httpContextAccessorMock.Object, coreDependencies.HttpContextAccessor);
            Assert.Same(currentUserServiceMock.Object, coreDependencies.CurrentUserService);
        }
    }
}
