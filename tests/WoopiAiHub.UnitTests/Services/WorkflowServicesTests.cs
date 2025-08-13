using Moq;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;
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
            _workflowRepositoryMock.Verify(repo => repo.FindById(workflowId), Times.Once);
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

            // Act
            var exception = await Assert.ThrowsAsync<AppException>(() => _workflowService.FindById(workflowId));

            // Assert
            _workflowRepositoryMock.Verify(repo => repo.FindById(workflowId), Times.Once);
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
            _workflowRepositoryMock.Verify(repo => repo.FindByTeamId(teamId), Times.Once);
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

            // Act
            var exception = await Assert.ThrowsAsync<AppException>(() => _workflowService.FindByTeamId(teamId));

            // Assert
            _workflowRepositoryMock.Verify(repo => repo.FindByTeamId(teamId), Times.Once);
            Assert.Equal(ErrorCode.NotFound, exception.ErrorCode);
            Assert.Equal("Workflow not found", exception.Message);
            Assert.Equal(WorkflowLabel.NotFound, exception.LabelError);
        }

        [Fact(DisplayName = "Test FindAllByUser returns workflows for valid user")]
        [Trait("FindAllByUser", "Success")]
        public void FindAllByUser_ValidUser_ReturnsWorkflows()
        {
            // Arrange
            var email = "user@email.com";
            var expectedWorkflows = new List<WorkflowDto>
            {
                new WorkflowDto { Id = 1, Name = "Workflow 1" },
                new WorkflowDto { Id = 2, Name = "Workflow 2" }
            };
            _workflowRepositoryMock.Setup(repo => repo.FindAllByUser(email))
                .Returns(expectedWorkflows);

            // Act
            var result = _workflowService.FindAllByUser(email);

            // Assert
            _workflowRepositoryMock.Verify(repo => repo.FindAllByUser(email), Times.Once);
            Assert.Equal(expectedWorkflows, result);
        }

        [Fact(DisplayName = "Test FindAllByUser returns empty when user has no workflows")]
        [Trait("FindAllByUser", "Fail")]
        public void FindAllByUser_UserHasNoWorkflows_ReturnsEmptyList()
        {
            // Arrange
            var email = "empty@email.com";
            var expectedWorkflows = new List<WorkflowDto>();
            _workflowRepositoryMock.Setup(repo => repo.FindAllByUser(email))
                .Returns(expectedWorkflows);

            // Act
            var result = _workflowService.FindAllByUser(email);

            // Assert
            _workflowRepositoryMock.Verify(repo => repo.FindAllByUser(email), Times.Once);
            Assert.Empty(result);
        }

    }
}
