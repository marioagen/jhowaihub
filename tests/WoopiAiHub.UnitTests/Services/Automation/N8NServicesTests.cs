using Moq;
using Moq.AutoMock;
using WoopiAiHub.Application.Services.Automation;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services.Automation;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.UnitTests.Fixture;
using Xunit;

namespace WoopiAiHub.UnitTests.Services.Automation
{
    [Collection(nameof(AutomationCollection))]
    public class N8NServicesTests
    {
        private readonly AutoMocker _mocker;
        private readonly N8NServices _service;

        public N8NServicesTests()
        {
            _mocker = new AutoMocker();
            _service = _mocker.CreateInstance<N8NServices>();
        }

        [Fact(DisplayName = "ProcessMessage should create StepToolOutput and call UpdateExecution")]
        [Trait("ProcessMessage", "Success")]
        public async Task ProcessMessage_ShouldCreateStepToolOutputAndCallUpdateExecution()
        {
            // Arrange
            var automationOutputDto = AutomationFixture.FindValidAutomationOutputDto();
            var stepToolExecution = AutomationFixture.FindValidStepToolExecution();
            stepToolExecution.Card = CardFixture.FindValidCard();
            stepToolExecution.StepTool = AutomationFixture.FindValidStepTool();

            var mockStepToolOutputRepository = _mocker.GetMock<IStepToolOutputRepository>();
            var mockStepToolExecutionRepository = _mocker.GetMock<IStepToolExecutionRepository>();
            var mockExecutionServices = _mocker.GetMock<IExecutionServices>();
            var mockDocumentHistoryRepository = _mocker.GetMock<IDocumentHistoryRepository>();

            mockStepToolExecutionRepository
                .Setup(repo => repo.FindByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(stepToolExecution);
            mockExecutionServices
                .Setup(service => service.HandleExecutionProgress(It.IsAny<StepToolExecution>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            mockDocumentHistoryRepository.Setup(d => d.Create(It.IsAny<DocumentHistory>()))
                .Returns(true);

            // Act
            await _service.ProcessMessage(automationOutputDto);

            // Assert
            mockStepToolOutputRepository.Verify(repo => repo.CreateAsync(It.IsAny<StepToolOutput>()), Times.Once);
            mockExecutionServices.Verify(service => service.HandleExecutionProgress(It.IsAny<StepToolExecution>(), It.IsAny<string>()), Times.Once);
            mockDocumentHistoryRepository.Verify(d => d.Create(It.IsAny<DocumentHistory>()), Times.Once);
        }
    }
}
