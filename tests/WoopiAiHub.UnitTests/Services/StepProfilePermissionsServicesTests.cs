using Moq;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.Interfaces.Repository;
using Xunit;
namespace WoopiAiHub.UnitTests.Services
{
    public class StepProfilePermissionsServicesTests
    {
        private readonly Mock<IStepProfilePermissionsRepository> _mockRepository;
        private readonly StepProfilePermissionsServices _service;

        public StepProfilePermissionsServicesTests()
        {
            _mockRepository = new Mock<IStepProfilePermissionsRepository>();
            _service = new StepProfilePermissionsServices(_mockRepository.Object);
        }

        [Fact(DisplayName = "Create should return false when permissions workflow is empty")]
        [Trait("Create", "Fail")]
        public async Task Create_ShouldReturnFalse_WhenPermissionsWorkflowIsEmpty()
        {
            // Arrange
            int profileId = 1;
            List<WorkflowPermissionDto> permissionsWorkflow = new List<WorkflowPermissionDto>();

            // Act
            var result = await _service.Create(profileId, permissionsWorkflow);

            // Assert
            Assert.False(result);
        }

        [Fact(DisplayName = "Create should return true when permissions workflow is not empty")]
        [Trait("Create", "Success")]
        public async Task Create_ShouldReturnTrue_WhenPermissionsWorkflowIsNotEmpty()
        {
            // Arrange
            int profileId = 1;
            List<WorkflowPermissionDto> permissionsWorkflow = new List<WorkflowPermissionDto>
            {
                new WorkflowPermissionDto()
            };

            _mockRepository.Setup(repo => repo.Create(profileId, permissionsWorkflow)).ReturnsAsync(true);

            // Act
            var result = await _service.Create(profileId, permissionsWorkflow);

            // Assert
            Assert.True(result);
            _mockRepository.Verify(repo => repo.Create(profileId, permissionsWorkflow), Times.Once);
        }

        [Fact(DisplayName = "Delete should return true when repository returns true")]
        [Trait("Delete", "Success")]
        public async Task Delete_ShouldReturnTrue_WhenRepositoryReturnsTrue()
        {
            // Arrange
            int profileId = 1;
            _mockRepository.Setup(repo => repo.DeleteAsync(profileId)).ReturnsAsync(true);

            // Act
            var result = await _service.Delete(profileId);

            // Assert
            Assert.True(result);
            _mockRepository.Verify(repo => repo.DeleteAsync(profileId), Times.Once);
        }

        [Fact(DisplayName = "Delete should return false when repository returns false")]
        [Trait("Delete", "Fail")]
        public async Task Delete_ShouldReturnFalse_WhenRepositoryReturnsFalse()
        {
            // Arrange
            int profileId = 1;
            _mockRepository.Setup(repo => repo.DeleteAsync(profileId)).ReturnsAsync(false);

            // Act
            var result = await _service.Delete(profileId);

            // Assert
            Assert.False(result);
            _mockRepository.Verify(repo => repo.DeleteAsync(profileId), Times.Once);
        }
    }
}

