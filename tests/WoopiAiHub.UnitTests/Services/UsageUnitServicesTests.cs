using Moq;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Domain.DTOs.Response.Automation;
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
            var usageUnits = new List<UsageUnitDto>
                {
                    new UsageUnitDto { Id = 1, Name = "Unit 1", Value = (decimal)0.006 },
                    new UsageUnitDto { Id = 2, Name = "Unit 2", Value = (decimal)0.006 }
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
                .ReturnsAsync(Enumerable.Empty<UsageUnitDto>());

            // Act
            var result = await _usageUnitServices.FindAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _usageUnitRepositoryMock.Verify(repo => repo.FindAllAsync(), Times.Once);
        }
    }
}
