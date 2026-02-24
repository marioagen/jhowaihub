using Moq;
using Moq.AutoMock;
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
        private readonly AutoMocker _mocker;

        public StatusServicesTests()
        {
            _mocker = new AutoMocker();
            _statusRepositoryMock = _mocker.GetMock<IStatusRepository>();
            _statusService = _mocker.CreateInstance<StatusServices>();
        }

        [Fact(DisplayName = "Test FindAll and returns status list")]
        [Trait("FindAll", "Success")]
        public async Task FindAll_ShouldReturnAllStatus()
        {
            // Arrange
            var expectedStatus = new List<StatusDto>
            {
                new StatusDto { Id = 1, Name = "Status1", Color = "#000000" },
                new StatusDto { Id = 2, Name = "Status2", Color = "#000000" }
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

        [Fact(DisplayName = "Test FindStatusForWorkflowSteps and returns workflow status list")]
        [Trait("FindStatusForWorkflowSteps", "Success")]
        public async Task FindStatusForWorkflowSteps_ShouldReturnWorkflowStatusList()
        {
            // Arrange
            var expectedStatus = new List<StatusDto>
            {
                new StatusDto { Id = 1, Name = "Pending", Color = "#FFA500" },
                new StatusDto { Id = 2, Name = "In Progress", Color = "#0000FF" },
                new StatusDto { Id = 3, Name = "Completed", Color = "#008000" }
            };

            _statusRepositoryMock.Setup(repo => repo.FindStatusForWorkflowSteps())
                .ReturnsAsync(expectedStatus);

            // Act
            var result = await _statusService.FindStatusForWorkflowSteps();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedStatus.Count, result.Count);
            Assert.Equal(expectedStatus, result);
            _statusRepositoryMock.Verify(repo => repo.FindStatusForWorkflowSteps(), Times.Once);
        }

        [Fact(DisplayName = "Test FindStatusForWorkflowSteps and returns empty list")]
        [Trait("FindStatusForWorkflowSteps", "Success")]
        public async Task FindStatusForWorkflowSteps_ShouldReturnEmptyList_WhenNoWorkflowStatusesExist()
        {
            // Arrange
            var expectedStatus = new List<StatusDto>();

            _statusRepositoryMock.Setup(repo => repo.FindStatusForWorkflowSteps())
                .ReturnsAsync(expectedStatus);

            // Act
            var result = await _statusService.FindStatusForWorkflowSteps();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _statusRepositoryMock.Verify(repo => repo.FindStatusForWorkflowSteps(), Times.Once);
        }

        [Fact(DisplayName = "Test FindStatusForWorkflowSteps verifies repository is called correctly")]
        [Trait("FindStatusForWorkflowSteps", "Success")]
        public async Task FindStatusForWorkflowSteps_ShouldCallRepositoryOnce()
        {
            // Arrange
            var expectedStatus = new List<StatusDto>
            {
                new StatusDto { Id = 1, Name = "Active", Color = "#00FF00" }
            };

            _statusRepositoryMock.Setup(repo => repo.FindStatusForWorkflowSteps())
                .ReturnsAsync(expectedStatus);

            // Act
            await _statusService.FindStatusForWorkflowSteps();

            // Assert
            _statusRepositoryMock.Verify(repo => repo.FindStatusForWorkflowSteps(), Times.Once);
            _statusRepositoryMock.VerifyNoOtherCalls();
        }
    }
}
