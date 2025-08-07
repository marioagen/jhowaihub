using Moq;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Utils.ErrorLabels;
using Xunit;

namespace WoopiAiHub.UnitTests.Services
{
    public class WorkflowServicesTests
    {
        private readonly Mock<IWorkflowRepository> _workflowRepositoryMock;
        private readonly WorkflowServices _workflowService;

        public WorkflowServicesTests()
        {
            _workflowRepositoryMock = new Mock<IWorkflowRepository>();
            _workflowService = new WorkflowServices(_workflowRepositoryMock.Object);
        }

        [Fact(DisplayName = "Test FindById and returns a workflow")]
        [Trait("FindById", "Success")]
        public async Task FindById_WorkflowExists_ReturnsWorkflow()
        {
            // Arrange
            var workflowId = 1;
            var expectedWorkflow = new WorkflowDto { Id = workflowId };
            _workflowRepositoryMock.Setup(repo => repo.FindById(workflowId))
                .ReturnsAsync(expectedWorkflow);

            // Act
            var result = await _workflowService.FindById(workflowId);

            // Assert
            Assert.Equal(expectedWorkflow, result);
        }

        [Fact(DisplayName = "Test FindById and throws an exception")]
        [Trait("FindById", "Fail")]
        public async Task FindById_WorkflowDoesNotExist_ThrowsAppException()
        {
            // Arrange
            var workflowId = 1;
            _workflowRepositoryMock.Setup(repo => repo.FindById(workflowId))
                .ReturnsAsync((WorkflowDto?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _workflowService.FindById(workflowId));
            Assert.Equal(ErrorCode.NotFound, exception.ErrorCode);
            Assert.Equal("Workflow not found", exception.Message);
            Assert.Equal(WorkflowLabel.NotFound, exception.LabelError);
        }

        [Fact(DisplayName = "Test FindByTeamId and returns a workflow")]
        [Trait("FindByTeamId", "Sucess")]
        public async Task FindByTeamId_WorkflowExists_ReturnsWorkflow()
        {
            // Arrange
            var teamId = 1;
            var expectedWorkflow = new WorkflowDto { TeamId = teamId };
            _workflowRepositoryMock.Setup(repo => repo.FindByTeamId(teamId))
                .ReturnsAsync(expectedWorkflow);

            // Act
            var result = await _workflowService.FindByTeamId(teamId);

            // Assert
            Assert.Equal(expectedWorkflow, result);
        }

        [Fact(DisplayName = "Test FindByTeamId and throws an exception")]
        [Trait("FindByTeamId", "Fail")]
        public async Task FindByTeamId_WorkflowDoesNotExist_ThrowsAppException()
        {
            // Arrange
            var teamId = 1;
            _workflowRepositoryMock.Setup(repo => repo.FindByTeamId(teamId))
                .ReturnsAsync((WorkflowDto?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _workflowService.FindByTeamId(teamId));
            Assert.Equal(ErrorCode.NotFound, exception.ErrorCode);
            Assert.Equal("Workflow not found", exception.Message);
            Assert.Equal(WorkflowLabel.NotFound, exception.LabelError);
        }
    }
}
