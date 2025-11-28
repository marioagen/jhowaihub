using Moq;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;
using Xunit;

namespace WoopiAiHub.UnitTests.Services
{
    public class UsageUnitServicesTests
    {
        private readonly Mock<IUsageUnitRepository> _usageUnitRepositoryMock;
        private readonly UsageUnitServices _usageUnitServices;

        public UsageUnitServicesTests()
        {
            _usageUnitRepositoryMock = new Mock<IUsageUnitRepository>();
            _usageUnitServices = new UsageUnitServices(_usageUnitRepositoryMock.Object);
        }

        [Fact(DisplayName = "Test FindAllAsync should return UsageUnits")]
        [Trait("FindAllAsync", "Success")]
        public async Task FindAllAsync_ShouldReturnUsageUnits_WhenCalled()
        {
            // Arrange
            var usageUnits = new List<UsageUnit>
            {
                new UsageUnit(1, DateTime.Now, "Unit 1", null, null, (decimal)0.006),
                new UsageUnit(2, DateTime.Now, "Unit 2", null, null, (decimal)0.006)
            };

            _usageUnitRepositoryMock.Setup(repo => repo.FindAllAsync())
                .ReturnsAsync(usageUnits);

            // Act
            var result = await _usageUnitServices.FindAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            _usageUnitRepositoryMock.Verify(repo => repo.FindAllAsync(), Times.Once);
        }

        [Fact]
        public async Task FindAllAsync_ShouldReturnEmptyList_WhenNoUsageUnitsExist()
        {
            // Arrange
            _usageUnitRepositoryMock.Setup(repo => repo.FindAllAsync())
                .ReturnsAsync(new List<UsageUnit>());

            // Act
            var result = await _usageUnitServices.FindAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _usageUnitRepositoryMock.Verify(repo => repo.FindAllAsync(), Times.Once);
        }
    }
}
