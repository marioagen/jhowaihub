using Moq;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Repository;
using Xunit;

namespace WoopiAiHub.UnitTests.Services
{
    public class StatusServicesTests
    {
        private readonly Mock<IStatusRepository> _statusRepositoryMock;
        private readonly StatusServices _statusService;

        public StatusServicesTests()
        {
            _statusRepositoryMock = new Mock<IStatusRepository>();
            _statusService = new StatusServices(_statusRepositoryMock.Object);
        }

        [Fact(DisplayName = "Test FindAll and returns status list")]
        [Trait("FindAll", "Success")]
        public async Task FindAll_ShouldReturnAllStatus()
        {
            // Arrange
            var expectedStatus = new List<StatusDto>
            {
                new StatusDto { Id = 1, Name = "Status1" },
                new StatusDto { Id = 2, Name = "Status2" }
            };

            _statusRepositoryMock.Setup(repo => repo.FindAll())
                .ReturnsAsync(expectedStatus);

            // Act
            var result = await _statusService.FindAll();

            // Assert
            Assert.Equal(expectedStatus, result);
            _statusRepositoryMock.Verify(repo => repo.FindAll(), Times.Once);
        }

        [Fact(DisplayName = "Test FindAll and returns status empty list")]
        [Trait("FindAll", "Success")]
        public async Task FindAll_ShouldReturnEmptyList_WhenNoStatusesExist()
        {
            // Arrange
            var expectedStatus = new List<StatusDto>();

            _statusRepositoryMock.Setup(repo => repo.FindAll())
                .ReturnsAsync(expectedStatus);

            // Act
            var result = await _statusService.FindAll();

            // Assert
            Assert.Empty(result);
            _statusRepositoryMock.Verify(repo => repo.FindAll(), Times.Once);
        }
    }
}
