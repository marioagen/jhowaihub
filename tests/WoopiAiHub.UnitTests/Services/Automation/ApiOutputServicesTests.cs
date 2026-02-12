using Moq;
using Moq.AutoMock;
using WoopiAiHub.Application.Services.Automation;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Hubs;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.UnitTests.Fixture;
using Xunit;

namespace WoopiAiHub.UnitTests.Services.Automation
{
    [Collection(nameof(AutomationCollection))]
    public class ApiOutputServicesTests
    {
        private readonly AutoMocker _mocker;
        private readonly ApiOutputServices _service;
        private readonly Mock<IStepToolOutputRepository> _mockStepToolOutputRepository;
        private readonly Mock<IStepToolExecutionRepository> _mockStepToolExecutionRepository;
        private readonly Mock<IDocumentHistoryRepository> _mockDocumentHistoryRepository;
        private readonly Mock<IWorkflowRepository> _mockWorkflowRepository;
        private readonly Mock<IHubNotifier> _mockHubNotifier;

        public ApiOutputServicesTests()
        {
            _mocker = new AutoMocker();
            _mockStepToolOutputRepository = _mocker.GetMock<IStepToolOutputRepository>();
            _mockStepToolExecutionRepository = _mocker.GetMock<IStepToolExecutionRepository>();
            _mockDocumentHistoryRepository = _mocker.GetMock<IDocumentHistoryRepository>();
            _mockWorkflowRepository = _mocker.GetMock<IWorkflowRepository>();
            _mockHubNotifier = _mocker.GetMock<IHubNotifier>();

            _service = _mocker.CreateInstance<ApiOutputServices>();
        }

        [Fact(DisplayName = "ProcessMessage should create StepToolOutput")]
        [Trait("ProcessMessage", "Success")]
        public async Task ProcessMessage_ShouldCreateStepToolOutputAndDocumentHistory()
        {
            // Arrange
            var apiOutputDto = CreateValidApiOutputDto();
            var stepToolExecution = CreateValidStepToolExecution();

            _mockStepToolExecutionRepository.Setup(repo => repo.FindByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(stepToolExecution);
            _mockStepToolExecutionRepository.Setup(repo => repo.ExecutionsByStepIdCountAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(2);
            _mockStepToolExecutionRepository.Setup(repo => repo.UpdateAsync(It.IsAny<StepToolExecution>()))
                .Returns(Task.CompletedTask);
            _mockStepToolOutputRepository.Setup(repo => repo.CreateAsync(It.IsAny<StepToolOutput>()))
                .ReturnsAsync(true);
            _mockWorkflowRepository.Setup(repo => repo.FindToolByStepToolId(It.IsAny<int>()))
                .ReturnsAsync((ToolDto?)null);
            _mockHubNotifier.Setup(notifier => notifier.CardProgessAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>(), It.IsAny<int>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.ProcessMessage(apiOutputDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(stepToolExecution.StepToolId, result.StepToolId);
            Assert.Equal(stepToolExecution.CardId, result.CardId);
            Assert.Equal(apiOutputDto.Tenant, result.Tenant);
            Assert.Equal(apiOutputDto.Email, result.Email);
            _mockStepToolOutputRepository.Verify(repo => repo.CreateAsync(It.Is<StepToolOutput>(
                output => output.StepToolId == stepToolExecution.StepToolId && output.CardId == stepToolExecution.CardId)), Times.Once);
        }

        [Fact(DisplayName = "ProcessMessage should throw AppException when execution not found")]
        [Trait("ProcessMessage", "Fail")]
        public async Task ProcessMessage_ShouldThrowAppException_WhenExecutionNotFound()
        {
            // Arrange
            var apiOutputDto = CreateValidApiOutputDto();
            _mockStepToolExecutionRepository.Setup(repo => repo.FindByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((StepToolExecution?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _service.ProcessMessage(apiOutputDto));
            Assert.Equal(ErrorCode.NotFound, exception.ErrorCode);
            Assert.Contains("StepToolExecution not found", exception.Message);
        }

        private static ApiOutputDto CreateValidApiOutputDto()
        {
            return new ApiOutputDto
            {
                TemplateName = "Test Template",
                Tenant = "test-tenant",
                Email = "test@example.com",
                ExecutionId = 1,
                StatusCode = 200,
                Content = "{\"result\": \"success\"}"
            };
        }

        private static StepToolExecution CreateValidStepToolExecution()
        {
            var execution = AutomationFixture.FindValidStepToolExecution();
            execution.Card = CardFixture.FindValidCard();
            execution.Card.Document = DocumentFixture.FindValidDocument();
            execution.StepTool = AutomationFixture.FindValidStepTool();
            return execution;
        }
    }
}
