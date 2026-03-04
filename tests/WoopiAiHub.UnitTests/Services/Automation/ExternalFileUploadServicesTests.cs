using Moq;
using Moq.AutoMock;
using WoopiAiHub.Application.Services.Automation;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Services.Automation;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.UnitTests.Fixture;
using Xunit;

namespace WoopiAiHub.UnitTests.Services.Automation
{
    [Collection(nameof(AutomationCollection))]
    public class ExternalFileUploadServicesTests
    {
        private readonly AutoMocker _mocker;
        private readonly WoopiAiHub.Application.Services.Automation.ExternalFileUploadServices _service;

        public ExternalFileUploadServicesTests()
        {
            _mocker = new AutoMocker();
            _service = _mocker.CreateInstance<WoopiAiHub.Application.Services.Automation.ExternalFileUploadServices>();
        }

        [Fact(DisplayName = "ProcessExternalFileUpload should successfully process when workflow exists and has executions")]
        [Trait("ProcessExternalFileUpload", "Success")]
        public async Task ProcessExternalFileUpload_Success_ShouldCreateDocumentAndStartExecution()
        {
            // Arrange
            var externalFileUploadDto = MessagingFixture.FindValidExternalFileUploadDto();
            var workflow = WorkflowFixture.FindValidWorkflow();
            var firstStep = WorkflowFixture.FindValidStep(workflow.Id);
            workflow.Steps.Add(firstStep);
            var workflowServicesMock = _mocker.GetMock<IWorkflowServices>();
            var documentRepositoryMock = _mocker.GetMock<IDocumentRepository>();
            var automationServicesMock = _mocker.GetMock<IAutomationServices>();
            var unitOfWorkMock = _mocker.GetMock<IUnitOfWork>();
            workflowServicesMock.Setup(s => s.FindModelById(It.IsAny<int>())).ReturnsAsync(workflow);
            documentRepositoryMock.Setup(r => r.Create(It.IsAny<Document>()));
            automationServicesMock.Setup(s => s.PrepareExecutionAsync(It.IsAny<List<Workflow>>())).ReturnsAsync(true);
            automationServicesMock.Setup(s => s.StartExecutionByWorkflowsAsync(It.IsAny<AutomationServicesDto>(), It.IsAny<List<Workflow>>())).Returns(Task.CompletedTask);

            // Act
            await _service.ProcessExternalFileUpload(externalFileUploadDto);

            // Assert
            unitOfWorkMock.Verify(u => u.BeginTransaction(), Times.Once);
            workflowServicesMock.Verify(s => s.FindModelById(externalFileUploadDto.WorkflowId), Times.Once);
            documentRepositoryMock.Verify(r => r.Create(It.IsAny<Document>()), Times.Once);
            automationServicesMock.Verify(s => s.PrepareExecutionAsync(It.Is<List<Workflow>>(w => w.First() == workflow)), Times.Once);
            automationServicesMock.Verify(s => s.StartExecutionByWorkflowsAsync(It.IsAny<AutomationServicesDto>(), It.Is<List<Workflow>>(w => w.First() == workflow)), Times.Once);
            unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
            unitOfWorkMock.Verify(u => u.Rollback(), Times.Never);
        }

        [Fact(DisplayName = "ProcessExternalFileUpload should successfully create document but not start execution when no executions available")]
        [Trait("ProcessExternalFileUpload", "Success")]
        public async Task ProcessExternalFileUpload_Success_ShouldCreateDocumentButNotStartExecution()
        {
            // Arrange
            var externalFileUploadDto = MessagingFixture.FindValidExternalFileUploadDto();
            var workflow = WorkflowFixture.FindValidWorkflow();
            var firstStep = WorkflowFixture.FindValidStep(workflow.Id);
            workflow.Steps.Add(firstStep);
            var workflowServicesMock = _mocker.GetMock<IWorkflowServices>();
            var documentRepositoryMock = _mocker.GetMock<IDocumentRepository>();
            var automationServicesMock = _mocker.GetMock<IAutomationServices>();
            var unitOfWorkMock = _mocker.GetMock<IUnitOfWork>();

            workflowServicesMock.Setup(s => s.FindModelById(externalFileUploadDto.WorkflowId)).ReturnsAsync(workflow);
            documentRepositoryMock.Setup(r => r.Create(It.IsAny<Document>()));
            automationServicesMock.Setup(s => s.PrepareExecutionAsync(It.IsAny<List<Workflow>>())).ReturnsAsync(false);

            // Act
            await _service.ProcessExternalFileUpload(externalFileUploadDto);

            // Assert
            unitOfWorkMock.Verify(u => u.BeginTransaction(), Times.Once);
            workflowServicesMock.Verify(s => s.FindModelById(externalFileUploadDto.WorkflowId), Times.Once);
            documentRepositoryMock.Verify(r => r.Create(It.IsAny<Document>()), Times.Once);
            automationServicesMock.Verify(s => s.PrepareExecutionAsync(It.IsAny<List<Workflow>>()), Times.Once);
            automationServicesMock.Verify(s => s.StartExecutionByWorkflowsAsync(It.IsAny<AutomationServicesDto>(), It.IsAny<List<Workflow>>()), Times.Never);
            unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
            unitOfWorkMock.Verify(u => u.Rollback(), Times.Never);
        }

        [Fact(DisplayName = "ProcessExternalFileUpload should do nothing when workflow is not found")]
        [Trait("ProcessExternalFileUpload", "Fail")]
        public async Task ProcessExternalFileUpload_WorkflowNotFound_ShouldNotCreateDocument()
        {
            // Arrange
            var externalFileUploadDto = MessagingFixture.FindValidExternalFileUploadDto();
            var workflowServicesMock = _mocker.GetMock<IWorkflowServices>();
            var documentRepositoryMock = _mocker.GetMock<IDocumentRepository>();
            var automationServicesMock = _mocker.GetMock<IAutomationServices>();
            var unitOfWorkMock = _mocker.GetMock<IUnitOfWork>();

            workflowServicesMock.Setup(s => s.FindModelById(externalFileUploadDto.WorkflowId)).ReturnsAsync((Workflow?)null);

            // Act
            await _service.ProcessExternalFileUpload(externalFileUploadDto);

            // Assert
            unitOfWorkMock.Verify(u => u.BeginTransaction(), Times.Once);
            workflowServicesMock.Verify(s => s.FindModelById(externalFileUploadDto.WorkflowId), Times.Once);
            documentRepositoryMock.Verify(r => r.Create(It.IsAny<Document>()), Times.Never);
            automationServicesMock.Verify(s => s.PrepareExecutionAsync(It.IsAny<List<Workflow>>()), Times.Never);
            automationServicesMock.Verify(s => s.StartExecutionByWorkflowsAsync(It.IsAny<AutomationServicesDto>(), It.IsAny<List<Workflow>>()), Times.Never);
            unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
            unitOfWorkMock.Verify(u => u.Rollback(), Times.Never);
        }

        [Fact(DisplayName = "ProcessExternalFileUpload should rollback and throw AppException on generic exception")]
        [Trait("ProcessExternalFileUpload", "Fail")]
        public async Task ProcessExternalFileUpload_OnException_ShouldRollbackAndThrowAppException()
        {
            // Arrange
            var externalFileUploadDto = MessagingFixture.FindValidExternalFileUploadDto();
            var expectedExceptionMessage = "Database connection error";
            var workflowServicesMock = _mocker.GetMock<IWorkflowServices>();
            var unitOfWorkMock = _mocker.GetMock<IUnitOfWork>();

            workflowServicesMock.Setup(s => s.FindModelById(externalFileUploadDto.WorkflowId)).ThrowsAsync(new Exception(expectedExceptionMessage));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _service.ProcessExternalFileUpload(externalFileUploadDto));
            
            Assert.Equal(ErrorCode.DefaultError, exception.ErrorCode);
            Assert.Equal(expectedExceptionMessage, exception.Message);
            unitOfWorkMock.Verify(u => u.BeginTransaction(), Times.Once);
            unitOfWorkMock.Verify(u => u.Commit(), Times.Never);
            unitOfWorkMock.Verify(u => u.Rollback(), Times.Once);
        }
    }
}
