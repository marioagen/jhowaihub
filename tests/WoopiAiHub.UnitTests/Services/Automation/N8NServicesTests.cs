using Moq;
using Moq.AutoMock;
using WoopiAiHub.Application.Services.Automation;
using WoopiAiHub.Domain.DTOs.Response.Automation;
using WoopiAiHub.Domain.Interfaces.Hubs;
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
        public async Task ProcessMessage_ShouldCreateStepToolOutputAndCallUpdateExecution()
        {
            // Arrange
            var automationOutputDto = AutomationFixture.FindValidAutomationOutputDto();
            var stepToolExecution = AutomationFixture.FindValidStepToolExecution();
            stepToolExecution.StepTool = AutomationFixture.FindValidStepTool();

            var mockStepToolOutputRepository = _mocker.GetMock<IStepToolOutputRepository>();
            var mockStepToolExecutionRepository = _mocker.GetMock<IStepToolExecutionRepository>();
            var mockHubNotifier = _mocker.GetMock<IHubNotifier>();

            mockStepToolExecutionRepository
                .Setup(repo => repo.FindByStepToolIdAndCardIdAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(stepToolExecution);
            mockStepToolExecutionRepository
                .Setup(repo => repo.ExecutionsByStepIdCountAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(2);
            mockStepToolExecutionRepository.Setup(repo => repo.UpdateAsync(It.IsAny<StepToolExecution>()))
                .Returns(Task.CompletedTask);
            mockHubNotifier.Setup(h => h.CardProgessAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>(), It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.ProcessMessage(automationOutputDto);

            // Assert
            mockStepToolOutputRepository.Verify(repo => repo.CreateAsync(It.IsAny<StepToolOutput>()), Times.Once);
            mockStepToolExecutionRepository.Verify(repo => repo.FindByStepToolIdAndCardIdAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            mockStepToolExecutionRepository.Verify(repo => repo.ExecutionsByStepIdCountAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            mockStepToolExecutionRepository.Verify(repo => repo.UpdateAsync(It.IsAny<StepToolExecution>()), Times.Once);
            mockHubNotifier.Verify(notifier => notifier.CardProgessAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>(), It.IsAny<int>()), Times.Once);
        }
    }
}
