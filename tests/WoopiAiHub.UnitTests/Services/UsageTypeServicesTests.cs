using Moq;
using Moq.AutoMock;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.UnitTests.Fixture;
using Xunit;

namespace WoopiAiHub.UnitTests.Services
{
    [Collection(nameof(UsageCollection))]
    public class UsageTypeServicesTests
    {
        private readonly AutoMocker _mocker;
        private readonly UsageTypeServices _service;
        private readonly Mock<IUsageTypeRepository> _repositoryMock;
        private readonly UsageFixture _fixture;

        public UsageTypeServicesTests()
        {
            _fixture = new UsageFixture();
            _mocker = new AutoMocker();
            _repositoryMock = _mocker.GetMock<IUsageTypeRepository>();
            _service = _mocker.CreateInstance<UsageTypeServices>();
        }

        [Fact(DisplayName = "FindByNameAsync should return usage type when found")]
        [Trait("FindByNameAsync", "Success")]
        public async Task FindByNameAsync_ShouldReturnUsageType_WhenFound()
        {
            // Arrange
            var usageType = _fixture.CreateValidUsageType();
            _repositoryMock.Setup(r => r.FindByNameAsync(usageType.Name))
                          .ReturnsAsync(usageType);

            // Act
            var result = await _service.FindByNameAsync(usageType.Name);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(usageType.Name, result.Name);
            _repositoryMock.Verify(r => r.FindByNameAsync(usageType.Name), Times.Once);
        }

        [Fact(DisplayName = "FindByNameAsync should return null when not found")]
        [Trait("FindByNameAsync", "Not Found")]
        public async Task FindByNameAsync_ShouldReturnNull_WhenNotFound()
        {
            // Arrange
            var name = "NonExistentType";
            _repositoryMock.Setup(r => r.FindByNameAsync(name))
                          .ReturnsAsync((UsageType?)null);

            // Act
            var result = await _service.FindByNameAsync(name);

            // Assert
            Assert.Null(result);
            _repositoryMock.Verify(r => r.FindByNameAsync(name), Times.Once);
        }
    }
}
