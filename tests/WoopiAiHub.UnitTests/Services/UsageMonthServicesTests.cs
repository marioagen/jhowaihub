using Moq;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Repository;
using Xunit;

namespace WoopiAiHub.UnitTests.Services
{
    public class UsageMonthServicesTests
    {
        private readonly Mock<IUsageMonthRepository> _usageMonthRepositoryMock;
        private readonly UsageMonthServices _usageMonthServices;

        public UsageMonthServicesTests()
        {
            _usageMonthRepositoryMock = new Mock<IUsageMonthRepository>();
            _usageMonthServices = new UsageMonthServices(_usageMonthRepositoryMock.Object);
        }

        [Fact(DisplayName = "Test FindDataByUsageType and returns a list of DashboardUsageDto")]
        [Trait("FindDataByUsageType", "Success")]
        public async Task FindDataByUsageType_ShouldReturnData()
        {
            // Arrange
            var usageType = ColTypeUsage.Ocr;
            var usageFilterDto = new UsageTypeFilterDto
            {
                UsageType = usageType.ToString(),
                Start = null,
                End = null
            };
            var expectedData = new List<DashboardUsageDto>
            {
                new DashboardUsageDto("2023-10-01", 10),
                new DashboardUsageDto("2023-10-02", 20)
            };

            _usageMonthRepositoryMock.Setup(x => x.FindDataByUsageType(It.IsAny<string>(), It.IsAny<DateTime?>(),It.IsAny<DateTime?>()))
                .ReturnsAsync(expectedData);

            // Act
            var result = await _usageMonthServices.FindDataByUsageType(usageFilterDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedData.Count, result.Count);
            Assert.Equal(expectedData, result);
            _usageMonthRepositoryMock.Verify(x => x.FindDataByUsageType(It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()), Times.Once);
        }

        [Fact(DisplayName = "Test FindDataByModelEmbedding and returns a list of DashboardUsageDto")]
        [Trait("FindDataByModelEmbedding", "Success")]
        public async Task FindDataByModelEmbedding_ShouldReturnData()
        {
            // Arrange
            var modelEmbeddingId = 1;
            var modelEmbeddingFilterDto = new ModelEmbeddingFilterDto
            {
                Id = modelEmbeddingId,
                Start = null,
                End = null
            };
            var expectedData = new List<DashboardUsageDto>
            {
                new DashboardUsageDto("2023-10-01", 5),
                new DashboardUsageDto("2023-10-02", 15)
            };

            _usageMonthRepositoryMock.Setup(x => x.FindDataByModelEmbedding(It.IsAny<int>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
                .ReturnsAsync(expectedData);

            // Act
            var result = await _usageMonthServices.FindDataByModelEmbedding(modelEmbeddingFilterDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedData.Count, result.Count);
            Assert.Equal(expectedData, result);
            _usageMonthRepositoryMock.Verify(x => x.FindDataByModelEmbedding(It.IsAny<int>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()), Times.Once);
        }

        [Fact(DisplayName = "Test FindUsedModelEmbeddings and returns a list of used ModelEmbeddingDto")]
        [Trait("FindUsedModelEmbeddings", "Success")]
        public async Task FindUsedModelEmbeddings_ShouldReturnData()
        {
            // Arrange
            var expectedData = new List<ModelEmbeddingDto>
            {
                new ModelEmbeddingDto{Id = 1, Name = "Model A" },
                new ModelEmbeddingDto{Id = 2, Name = "Model B" }
            };

            _usageMonthRepositoryMock.Setup(x => x.FindUsedModelEmbeddings())
                .ReturnsAsync(expectedData);

            // Act
            var result = await _usageMonthServices.FindUsedModelEmbeddings();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedData.Count, result.Count);
            Assert.Equal(expectedData, result);
            _usageMonthRepositoryMock.Verify(x => x.FindUsedModelEmbeddings(), Times.Once);
        }
    }
}
