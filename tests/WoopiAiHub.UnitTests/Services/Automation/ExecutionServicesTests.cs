using Moq;
using Moq.AutoMock;
using WoopiAiHub.Application.Services.Automation;
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
    public class ExecutionServicesTests
    {
        private readonly AutoMocker _mocker;
        private readonly ExecutionServices _service;

        public ExecutionServicesTests()
        {
            _mocker = new AutoMocker();
            _service = _mocker.CreateInstance<ExecutionServices>();
        }

        [Fact(DisplayName = "HandleExecutionProgress should update execution status and notify when card exists without batch")]
        [Trait("HandleExecutionProgress", "Success")]
        public async Task HandleExecutionProgress_CardWithoutBatch_ShouldUpdateStatusAndNotify()
        {
            // Arrange
            var card = new Card(1, DateTime.Now, 1, 1, "Test Card", 1, Guid.NewGuid(), null);
            var stepTool1 = new StepTool(1, DateTime.Now, 1, 1, 1, 1, 1);
            var stepTool2 = new StepTool(2, DateTime.Now, 1, 1, 1, 1, 1);
            var step = new Step(1, DateTime.Now, 1, "Test Step", 1, 1, 1);
            step.StepTools = new List<StepTool> { stepTool1, stepTool2 };

            var execution = new StepToolExecution(1, DateTime.Now, stepTool1.Id, StatusExecution.Pending, card.Id);
            execution.StepTool = stepTool1;
            execution.Card = card;

            var execution1 = new StepToolExecution(2, DateTime.Now, stepTool1.Id, StatusExecution.Ready, card.Id);
            var execution2 = new StepToolExecution(3, DateTime.Now, stepTool2.Id, StatusExecution.Running, card.Id);

            var email = "test@example.com";
            var tool = ToolFixture.FindValidTool();

            _mocker.GetMock<IStepToolExecutionRepository>()
                .Setup(x => x.UpdateAsync(It.IsAny<StepToolExecution>()))
                .Returns(Task.CompletedTask);

            _mocker.GetMock<ICardRepository>()
                .Setup(x => x.FindById(execution.CardId))
                .ReturnsAsync(card);

            _mocker.GetMock<IStepRepository>()
                .Setup(x => x.FindByIdWithTools(stepTool1.StepId))
                .ReturnsAsync(step);

            _mocker.GetMock<IStepToolExecutionRepository>()
                .Setup(x => x.FindByStepToolIdAndCardIdAsync(stepTool1.Id, card.Id))
                .ReturnsAsync(execution1);

            _mocker.GetMock<IStepToolExecutionRepository>()
                .Setup(x => x.FindByStepToolIdAndCardIdAsync(stepTool2.Id, card.Id))
                .ReturnsAsync(execution2);

            _mocker.GetMock<IWorkflowRepository>()
                .Setup(x => x.FindToolByStepToolId(stepTool2.Id))
                .ReturnsAsync(tool);

            _mocker.GetMock<IHubNotifier>()
                .Setup(x => x.CardProgessAsync(email, card.Id, It.IsAny<double>(), stepTool1.StepId, It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.HandleExecutionProgress(execution, email);

            // Assert
            _mocker.GetMock<IStepToolExecutionRepository>().Verify(x => x.UpdateAsync(It.IsAny<StepToolExecution>()), Times.Once);
            _mocker.GetMock<ICardRepository>().Verify(x => x.FindById(execution.CardId), Times.Once);
            _mocker.GetMock<IStepRepository>().Verify(x => x.FindByIdWithTools(stepTool1.StepId), Times.Once);
            _mocker.GetMock<IHubNotifier>().Verify(x => x.CardProgessAsync(email, card.Id, 50.0, stepTool1.StepId, tool.Name, It.IsAny<bool>()), Times.Once);
        }

        [Fact(DisplayName = "HandleExecutionProgress should notify all batch cards when card has DocumentBatchId")]
        [Trait("HandleExecutionProgress", "Success")]
        public async Task HandleExecutionProgress_CardWithBatch_ShouldNotifyAllBatchCards()
        {
            // Arrange
            var card = new Card(1, DateTime.Now, 1, 1, "Test Card", 1, Guid.NewGuid(), 100);
            var batchCard1 = new Card(1, DateTime.Now, 1, 1, "Batch Card 1", 1, Guid.NewGuid(), 100);
            var batchCard2 = new Card(2, DateTime.Now, 1, 1, "Batch Card 2", 1, Guid.NewGuid(), 100);
            var batchCards = new List<Card> { batchCard1, batchCard2 };

            var stepTool = new StepTool(1, DateTime.Now, 1, 1, 1, 1, 1);
            var step = new Step(1, DateTime.Now, 1, "Test Step", 1, 1, 1);
            step.StepTools = new List<StepTool> { stepTool };

            var execution = new StepToolExecution(1, DateTime.Now, stepTool.Id, StatusExecution.Pending, card.Id);
            execution.StepTool = stepTool;
            execution.Card = card;

            var execution1 = new StepToolExecution(2, DateTime.Now, stepTool.Id, StatusExecution.Ready, batchCard1.Id);
            var execution2 = new StepToolExecution(3, DateTime.Now, stepTool.Id, StatusExecution.Ready, batchCard2.Id);

            var email = "test@example.com";

            _mocker.GetMock<IStepToolExecutionRepository>()
                .Setup(x => x.UpdateAsync(It.IsAny<StepToolExecution>()))
                .Returns(Task.CompletedTask);

            _mocker.GetMock<ICardRepository>()
                .Setup(x => x.FindById(execution.CardId))
                .ReturnsAsync(card);

            _mocker.GetMock<ICardRepository>()
                .Setup(x => x.FindByDocumentBatchId(100))
                .ReturnsAsync(batchCards);

            _mocker.GetMock<IStepRepository>()
                .Setup(x => x.FindByIdWithTools(stepTool.StepId))
                .ReturnsAsync(step);

            _mocker.GetMock<IStepToolExecutionRepository>()
                .Setup(x => x.FindByStepToolIdAndCardIdAsync(stepTool.Id, batchCard1.Id))
                .ReturnsAsync(execution1);

            _mocker.GetMock<IStepToolExecutionRepository>()
                .Setup(x => x.FindByStepToolIdAndCardIdAsync(stepTool.Id, batchCard2.Id))
                .ReturnsAsync(execution2);

            _mocker.GetMock<IHubNotifier>()
                .Setup(x => x.CardProgessAsync(email, It.IsAny<int>(), It.IsAny<double>(), stepTool.StepId, It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.HandleExecutionProgress(execution, email);

            // Assert
            _mocker.GetMock<ICardRepository>().Verify(x => x.FindByDocumentBatchId(100), Times.Once);
            _mocker.GetMock<IHubNotifier>().Verify(x => x.CardProgessAsync(email, batchCard1.Id, 100.0, stepTool.StepId, string.Empty, It.IsAny<bool>()), Times.Once);
            _mocker.GetMock<IHubNotifier>().Verify(x => x.CardProgessAsync(email, batchCard2.Id, 100.0, stepTool.StepId, string.Empty, It.IsAny<bool>()), Times.Once);
        }

        [Fact(DisplayName = "HandleExecutionProgress should return early when card is not found")]
        [Trait("HandleExecutionProgress", "CardNotFound")]
        public async Task HandleExecutionProgress_CardNotFound_ShouldReturnEarly()
        {
            // Arrange
            var stepTool = new StepTool(1, DateTime.Now, 1, 1, 1, 1, 1);
            var execution = new StepToolExecution(1, DateTime.Now, stepTool.Id, StatusExecution.Pending, 1);
            execution.StepTool = stepTool;

            var email = "test@example.com";

            _mocker.GetMock<IStepToolExecutionRepository>()
                .Setup(x => x.UpdateAsync(It.IsAny<StepToolExecution>()))
                .Returns(Task.CompletedTask);

            _mocker.GetMock<ICardRepository>()
                .Setup(x => x.FindById(execution.CardId))
                .ReturnsAsync((Card?)null);

            // Act
            await _service.HandleExecutionProgress(execution, email);

            // Assert
            _mocker.GetMock<IStepToolExecutionRepository>().Verify(x => x.UpdateAsync(It.IsAny<StepToolExecution>()), Times.Once);
            _mocker.GetMock<IStepRepository>().Verify(x => x.FindByIdWithTools(It.IsAny<int>()), Times.Never);
            _mocker.GetMock<IHubNotifier>().Verify(x => x.CardProgessAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact(DisplayName = "HandleExecutionProgress should return early when step is not found")]
        [Trait("HandleExecutionProgress", "StepNotFound")]
        public async Task HandleExecutionProgress_StepNotFound_ShouldReturnEarly()
        {
            // Arrange
            var card = new Card(1, DateTime.Now, 1, 1, "Test Card", 1, Guid.NewGuid(), null);
            var stepTool = new StepTool(1, DateTime.Now, 1, 1, 1, 1, 1);
            var execution = new StepToolExecution(1, DateTime.Now, stepTool.Id, StatusExecution.Pending, card.Id);
            execution.StepTool = stepTool;
            execution.Card = card;

            var email = "test@example.com";

            _mocker.GetMock<IStepToolExecutionRepository>()
                .Setup(x => x.UpdateAsync(It.IsAny<StepToolExecution>()))
                .Returns(Task.CompletedTask);

            _mocker.GetMock<ICardRepository>()
                .Setup(x => x.FindById(execution.CardId))
                .ReturnsAsync(card);

            _mocker.GetMock<IStepRepository>()
                .Setup(x => x.FindByIdWithTools(stepTool.StepId))
                .ReturnsAsync((Step?)null);

            // Act
            await _service.HandleExecutionProgress(execution, email);

            // Assert
            _mocker.GetMock<IStepToolExecutionRepository>().Verify(x => x.UpdateAsync(It.IsAny<StepToolExecution>()), Times.Once);
            _mocker.GetMock<ICardRepository>().Verify(x => x.FindById(execution.CardId), Times.Once);
            _mocker.GetMock<IHubNotifier>().Verify(x => x.CardProgessAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact(DisplayName = "HandleExecutionProgress should notify 100% progress when step has no tools")]
        [Trait("HandleExecutionProgress", "NoStepTools")]
        public async Task HandleExecutionProgress_NoStepTools_ShouldNotify100Percent()
        {
            // Arrange
            var card = new Card(1, DateTime.Now, 1, 1, "Test Card", 1, Guid.NewGuid(), null);
            var stepTool = new StepTool(1, DateTime.Now, 1, 1, 1, 1, 1);
            var step = new Step(1, DateTime.Now, 1, "Test Step", 1, 1, 1);
            step.StepTools = new List<StepTool>();

            var execution = new StepToolExecution(1, DateTime.Now, stepTool.Id, StatusExecution.Pending, card.Id);
            execution.StepTool = stepTool;
            execution.Card = card;

            var email = "test@example.com";

            _mocker.GetMock<IStepToolExecutionRepository>()
                .Setup(x => x.UpdateAsync(It.IsAny<StepToolExecution>()))
                .Returns(Task.CompletedTask);

            _mocker.GetMock<ICardRepository>()
                .Setup(x => x.FindById(execution.CardId))
                .ReturnsAsync(card);

            _mocker.GetMock<IStepRepository>()
                .Setup(x => x.FindByIdWithTools(stepTool.StepId))
                .ReturnsAsync(step);

            _mocker.GetMock<IHubNotifier>()
                .Setup(x => x.CardProgessAsync(email, card.Id, 100.0, stepTool.StepId, string.Empty, It.IsAny<bool>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.HandleExecutionProgress(execution, email);

            // Assert
            _mocker.GetMock<IHubNotifier>().Verify(x => x.CardProgessAsync(email, card.Id, 100.0, stepTool.StepId, string.Empty, It.IsAny<bool>()), Times.Once);
        }

        [Fact(DisplayName = "HandleExecutionProgress should calculate correct progress percentage with multiple tools")]
        [Trait("HandleExecutionProgress", "ProgressCalculation")]
        public async Task HandleExecutionProgress_MultipleTools_ShouldCalculateCorrectProgress()
        {
            // Arrange
            var card = new Card(1, DateTime.Now, 1, 1, "Test Card", 1, Guid.NewGuid(), null);
            var stepTool1 = new StepTool(1, DateTime.Now, 1, 1, 1, 1, 1);
            var stepTool2 = new StepTool(2, DateTime.Now, 1, 1, 1, 1, 1);
            var stepTool3 = new StepTool(3, DateTime.Now, 1, 1, 1, 1, 1);
            var stepTool4 = new StepTool(4, DateTime.Now, 1, 1, 1, 1, 1);
            var step = new Step(1, DateTime.Now, 1, "Test Step", 1, 1, 1);
            step.StepTools = new List<StepTool> { stepTool1, stepTool2, stepTool3, stepTool4 };

            var execution = new StepToolExecution(1, DateTime.Now, stepTool1.Id, StatusExecution.Pending, card.Id);
            execution.StepTool = stepTool1;
            execution.Card = card;

            var execution1 = new StepToolExecution(2, DateTime.Now, stepTool1.Id, StatusExecution.Ready, card.Id);
            var execution2 = new StepToolExecution(3, DateTime.Now, stepTool2.Id, StatusExecution.Ready, card.Id);
            var execution3 = new StepToolExecution(4, DateTime.Now, stepTool3.Id, StatusExecution.Running, card.Id);
            var execution4 = new StepToolExecution(5, DateTime.Now, stepTool4.Id, StatusExecution.Pending, card.Id);

            var email = "test@example.com";
            var tool = ToolFixture.FindValidTool();

            _mocker.GetMock<IStepToolExecutionRepository>()
                .Setup(x => x.UpdateAsync(It.IsAny<StepToolExecution>()))
                .Returns(Task.CompletedTask);

            _mocker.GetMock<ICardRepository>()
                .Setup(x => x.FindById(execution.CardId))
                .ReturnsAsync(card);

            _mocker.GetMock<IStepRepository>()
                .Setup(x => x.FindByIdWithTools(stepTool1.StepId))
                .ReturnsAsync(step);

            _mocker.GetMock<IStepToolExecutionRepository>()
                .Setup(x => x.FindByStepToolIdAndCardIdAsync(stepTool1.Id, card.Id))
                .ReturnsAsync(execution1);

            _mocker.GetMock<IStepToolExecutionRepository>()
                .Setup(x => x.FindByStepToolIdAndCardIdAsync(stepTool2.Id, card.Id))
                .ReturnsAsync(execution2);

            _mocker.GetMock<IStepToolExecutionRepository>()
                .Setup(x => x.FindByStepToolIdAndCardIdAsync(stepTool3.Id, card.Id))
                .ReturnsAsync(execution3);

            _mocker.GetMock<IStepToolExecutionRepository>()
                .Setup(x => x.FindByStepToolIdAndCardIdAsync(stepTool4.Id, card.Id))
                .ReturnsAsync(execution4);

            _mocker.GetMock<IWorkflowRepository>()
                .Setup(x => x.FindToolByStepToolId(stepTool3.Id))
                .ReturnsAsync(tool);

            _mocker.GetMock<IHubNotifier>()
                .Setup(x => x.CardProgessAsync(email, card.Id, It.IsAny<double>(), stepTool1.StepId, It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.HandleExecutionProgress(execution, email);

            // Assert
            // 2 completed out of 4 = 50%
            _mocker.GetMock<IHubNotifier>().Verify(x => x.CardProgessAsync(email, card.Id, 50.0, stepTool1.StepId, tool.Name, It.IsAny<bool>()), Times.Once);
        }

        [Fact(DisplayName = "HandleExecutionProgress should identify running tool name correctly")]
        [Trait("HandleExecutionProgress", "RunningToolName")]
        public async Task HandleExecutionProgress_WithRunningTool_ShouldIdentifyToolName()
        {
            // Arrange
            var card = new Card(1, DateTime.Now, 1, 1, "Test Card", 1, Guid.NewGuid(), null);
            var stepTool1 = new StepTool(1, DateTime.Now, 1, 1, 1, 1, 1);
            var stepTool2 = new StepTool(2, DateTime.Now, 1, 1, 1, 1, 1);
            var step = new Step(1, DateTime.Now, 1, "Test Step", 1, 1, 1);
            step.StepTools = new List<StepTool> { stepTool1, stepTool2 };

            var execution = new StepToolExecution(1, DateTime.Now, stepTool1.Id, StatusExecution.Pending, card.Id);
            execution.StepTool = stepTool1;
            execution.Card = card;

            var execution1 = new StepToolExecution(2, DateTime.Now, stepTool1.Id, StatusExecution.Pending, card.Id);
            var execution2 = new StepToolExecution(3, DateTime.Now, stepTool2.Id, StatusExecution.Running, card.Id);

            var email = "test@example.com";
            var tool = ToolFixture.FindValidTool();
            tool.Name = "Running Tool Name";

            _mocker.GetMock<IStepToolExecutionRepository>()
                .Setup(x => x.UpdateAsync(It.IsAny<StepToolExecution>()))
                .Returns(Task.CompletedTask);

            _mocker.GetMock<ICardRepository>()
                .Setup(x => x.FindById(execution.CardId))
                .ReturnsAsync(card);

            _mocker.GetMock<IStepRepository>()
                .Setup(x => x.FindByIdWithTools(stepTool1.StepId))
                .ReturnsAsync(step);

            _mocker.GetMock<IStepToolExecutionRepository>()
                .Setup(x => x.FindByStepToolIdAndCardIdAsync(stepTool1.Id, card.Id))
                .ReturnsAsync(execution1);

            _mocker.GetMock<IStepToolExecutionRepository>()
                .Setup(x => x.FindByStepToolIdAndCardIdAsync(stepTool2.Id, card.Id))
                .ReturnsAsync(execution2);

            _mocker.GetMock<IWorkflowRepository>()
                .Setup(x => x.FindToolByStepToolId(stepTool2.Id))
                .ReturnsAsync(tool);

            _mocker.GetMock<IHubNotifier>()
                .Setup(x => x.CardProgessAsync(email, card.Id, It.IsAny<double>(), stepTool1.StepId, "Running Tool Name", It.IsAny<bool>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.HandleExecutionProgress(execution, email);

            // Assert
            _mocker.GetMock<IHubNotifier>().Verify(x => x.CardProgessAsync(email, card.Id, 0.0, stepTool1.StepId, "Running Tool Name", It.IsAny<bool>()), Times.Once);
        }

        [Fact(DisplayName = "HandleExecutionProgress should handle null tool name gracefully")]
        [Trait("HandleExecutionProgress", "NullToolName")]
        public async Task HandleExecutionProgress_NullToolName_ShouldUseEmptyString()
        {
            // Arrange
            var card = new Card(1, DateTime.Now, 1, 1, "Test Card", 1, Guid.NewGuid(), null);
            var stepTool = new StepTool(1, DateTime.Now, 1, 1, 1, 1, 1);
            var step = new Step(1, DateTime.Now, 1, "Test Step", 1, 1, 1);
            step.StepTools = new List<StepTool> { stepTool };

            var execution = new StepToolExecution(1, DateTime.Now, stepTool.Id, StatusExecution.Pending, card.Id);
            execution.StepTool = stepTool;
            execution.Card = card;

            var exec = new StepToolExecution(2, DateTime.Now, stepTool.Id, StatusExecution.Running, card.Id);

            var email = "test@example.com";

            _mocker.GetMock<IStepToolExecutionRepository>()
                .Setup(x => x.UpdateAsync(It.IsAny<StepToolExecution>()))
                .Returns(Task.CompletedTask);

            _mocker.GetMock<ICardRepository>()
                .Setup(x => x.FindById(execution.CardId))
                .ReturnsAsync(card);

            _mocker.GetMock<IStepRepository>()
                .Setup(x => x.FindByIdWithTools(stepTool.StepId))
                .ReturnsAsync(step);

            _mocker.GetMock<IStepToolExecutionRepository>()
                .Setup(x => x.FindByStepToolIdAndCardIdAsync(stepTool.Id, card.Id))
                .ReturnsAsync(exec);

            _mocker.GetMock<IWorkflowRepository>()
                .Setup(x => x.FindToolByStepToolId(stepTool.Id))
                .ReturnsAsync(default(ToolDto)!);

            _mocker.GetMock<IHubNotifier>()
                .Setup(x => x.CardProgessAsync(email, card.Id, It.IsAny<double>(), stepTool.StepId, string.Empty, It.IsAny<bool>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.HandleExecutionProgress(execution, email);

            // Assert
            _mocker.GetMock<IHubNotifier>().Verify(x => x.CardProgessAsync(email, card.Id, 0.0, stepTool.StepId, string.Empty, It.IsAny<bool>()), Times.Once);
        }

        [Fact(DisplayName = "HandleExecutionProgress should handle null execution for a card")]
        [Trait("HandleExecutionProgress", "NullExecution")]
        public async Task HandleExecutionProgress_NullExecution_ShouldCountAsIncomplete()
        {
            // Arrange
            var card = new Card(1, DateTime.Now, 1, 1, "Test Card", 1, Guid.NewGuid(), null);
            var stepTool1 = new StepTool(1, DateTime.Now, 1, 1, 1, 1, 1);
            var stepTool2 = new StepTool(2, DateTime.Now, 1, 1, 1, 1, 1);
            var step = new Step(1, DateTime.Now, 1, "Test Step", 1, 1, 1);
            step.StepTools = new List<StepTool> { stepTool1, stepTool2 };

            var execution = new StepToolExecution(1, DateTime.Now, stepTool1.Id, StatusExecution.Pending, card.Id);
            execution.StepTool = stepTool1;
            execution.Card = card;

            var execution1 = new StepToolExecution(2, DateTime.Now, stepTool1.Id, StatusExecution.Ready, card.Id);

            var email = "test@example.com";

            _mocker.GetMock<IStepToolExecutionRepository>()
                .Setup(x => x.UpdateAsync(It.IsAny<StepToolExecution>()))
                .Returns(Task.CompletedTask);

            _mocker.GetMock<ICardRepository>()
                .Setup(x => x.FindById(execution.CardId))
                .ReturnsAsync(card);

            _mocker.GetMock<IStepRepository>()
                .Setup(x => x.FindByIdWithTools(stepTool1.StepId))
                .ReturnsAsync(step);

            _mocker.GetMock<IStepToolExecutionRepository>()
                .Setup(x => x.FindByStepToolIdAndCardIdAsync(stepTool1.Id, card.Id))
                .ReturnsAsync(execution1);

            _mocker.GetMock<IStepToolExecutionRepository>()
                .Setup(x => x.FindByStepToolIdAndCardIdAsync(stepTool2.Id, card.Id))
                .ReturnsAsync((StepToolExecution?)null);

            _mocker.GetMock<IHubNotifier>()
                .Setup(x => x.CardProgessAsync(email, card.Id, It.IsAny<double>(), stepTool1.StepId, It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.HandleExecutionProgress(execution, email);

            // Assert
            // Only 1 complete out of 2 = 50%
            _mocker.GetMock<IHubNotifier>().Verify(x => x.CardProgessAsync(email, card.Id, 50.0, stepTool1.StepId, string.Empty, It.IsAny<bool>()), Times.Once);
        }

        [Fact(DisplayName = "HandleExecutionProgress should notify 100% when all step tools are ready")]
        [Trait("HandleExecutionProgress", "AllComplete")]
        public async Task HandleExecutionProgress_AllStepToolsReady_ShouldNotify100Percent()
        {
            // Arrange
            var card = new Card(1, DateTime.Now, 1, 1, "Test Card", 1, Guid.NewGuid(), null);
            var stepTool1 = new StepTool(1, DateTime.Now, 1, 1, 1, 1, 1);
            var stepTool2 = new StepTool(2, DateTime.Now, 1, 1, 1, 1, 1);
            var step = new Step(1, DateTime.Now, 1, "Test Step", 1, 1, 1);
            step.StepTools = new List<StepTool> { stepTool1, stepTool2 };

            var execution = new StepToolExecution(1, DateTime.Now, stepTool1.Id, StatusExecution.Pending, card.Id);
            execution.StepTool = stepTool1;
            execution.Card = card;

            var execution1 = new StepToolExecution(2, DateTime.Now, stepTool1.Id, StatusExecution.Ready, card.Id);
            var execution2 = new StepToolExecution(3, DateTime.Now, stepTool2.Id, StatusExecution.Ready, card.Id);

            var email = "test@example.com";

            _mocker.GetMock<IStepToolExecutionRepository>()
                .Setup(x => x.UpdateAsync(It.IsAny<StepToolExecution>()))
                .Returns(Task.CompletedTask);

            _mocker.GetMock<ICardRepository>()
                .Setup(x => x.FindById(execution.CardId))
                .ReturnsAsync(card);

            _mocker.GetMock<IStepRepository>()
                .Setup(x => x.FindByIdWithTools(stepTool1.StepId))
                .ReturnsAsync(step);

            _mocker.GetMock<IStepToolExecutionRepository>()
                .Setup(x => x.FindByStepToolIdAndCardIdAsync(stepTool1.Id, card.Id))
                .ReturnsAsync(execution1);

            _mocker.GetMock<IStepToolExecutionRepository>()
                .Setup(x => x.FindByStepToolIdAndCardIdAsync(stepTool2.Id, card.Id))
                .ReturnsAsync(execution2);

            _mocker.GetMock<IHubNotifier>()
                .Setup(x => x.CardProgessAsync(email, card.Id, 100.0, stepTool1.StepId, string.Empty, It.IsAny<bool>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.HandleExecutionProgress(execution, email);

            // Assert
            _mocker.GetMock<IHubNotifier>().Verify(x => x.CardProgessAsync(email, card.Id, 100.0, stepTool1.StepId, string.Empty, It.IsAny<bool>()), Times.Once);
        }
    }
}
