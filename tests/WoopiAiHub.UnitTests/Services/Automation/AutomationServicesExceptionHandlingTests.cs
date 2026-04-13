using Moq;
using Moq.AutoMock;
using WoopiAiHub.Application.Services.Automation;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request.Automation;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Handlers;
using WoopiAiHub.Domain.Interfaces.Messaging;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Services.Automation;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Utils;
using WoopiAiHub.UnitTests.Fixture;
using Xunit;

namespace WoopiAiHub.UnitTests.Services.Automation
{
    [Collection(nameof(AutomationCollection))]
    public class AutomationServicesExceptionHandlingTests
    {
        private readonly AutoMocker _mocker;
        private readonly AutomationServices _service;

        public AutomationServicesExceptionHandlingTests()
        {
            _mocker = new AutoMocker();
            _service = _mocker.CreateInstance<AutomationServices>();
        }

        #region StartExecutionByStepAsync - Exception Handling Tests

        [Fact(DisplayName = "StartExecutionByStepAsync should mark cards as failing when an exception occurs during execution")]
        [Trait("StartExecutionByStepAsync", "Exception Handling")]
        public async Task StartExecutionByStepAsync_ShouldMarkCardsAsFailingOnException()
        {
            // Arrange
            var stepTool = AutomationFixture.FindValidStepTool();
            var tool = ToolFixture.FindValidToolModel();
            tool.ToolType = ToolTypeFixture.FindValidToolType();
            stepTool.Tool = tool;

            var card1 = new Card(1, DateTime.UtcNow, 1, 1, "Card 1", 1, Guid.NewGuid());
            var card2 = new Card(2, DateTime.UtcNow, 1, 1, "Card 2", 1, Guid.NewGuid());
            var cards = new List<Card> { card1, card2 };

            var step = WorkflowFixture.FindValidStep();
            step.StepTools = new List<StepTool> { stepTool };
            step.Cards = cards;

            var automationDto = AutomationFixture.FindValidautomationServicesDto();

            var stepToolExecutionRepositoryMock = _mocker.GetMock<IStepToolExecutionRepository>();
            var failingCardServiceMock = _mocker.GetMock<IFailingCardService>();

            stepToolExecutionRepositoryMock
                .Setup(r => r.FindByStepToolIdAndCardIdAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ThrowsAsync(new InvalidOperationException("Database connection error"));

            failingCardServiceMock
                .Setup(s => s.SetFailingCard(It.IsAny<int>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.StartExecutionByStepAsync(step, automationDto);

            // Assert
            failingCardServiceMock.Verify(
                s => s.SetFailingCard(card1.Id, automationDto.Email),
                Times.Once,
                "Card 1 should be marked as failing");

            failingCardServiceMock.Verify(
                s => s.SetFailingCard(card2.Id, automationDto.Email),
                Times.Once,
                "Card 2 should be marked as failing");
        }

        [Fact(DisplayName = "StartExecutionByStepAsync should handle empty card collection gracefully")]
        [Trait("StartExecutionByStepAsync", "Exception Handling")]
        public async Task StartExecutionByStepAsync_ShouldHandleEmptyCardsGracefully()
        {
            // Arrange
            var step = WorkflowFixture.FindValidStep();
            step.StepTools = new List<StepTool>();
            step.Cards = new List<Card>();

            var automationDto = AutomationFixture.FindValidautomationServicesDto();

            var failingCardServiceMock = _mocker.GetMock<IFailingCardService>();

            // Act
            var exception = await Record.ExceptionAsync(() => _service.StartExecutionByStepAsync(step, automationDto));

            // Assert
            Assert.Null(exception);
            failingCardServiceMock.Verify(s => s.SetFailingCard(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        [Fact(DisplayName = "StartExecutionByStepAsync should continue processing if MarkCardAsFailingAsync throws exception")]
        [Trait("StartExecutionByStepAsync", "Exception Handling")]
        public async Task StartExecutionByStepAsync_ShouldContinueProcessingIfMarkCardAsFailingThrows()
        {
            // Arrange
            var stepTool = AutomationFixture.FindValidStepTool();
            var tool = ToolFixture.FindValidToolModel();
            tool.ToolType = ToolTypeFixture.FindValidToolType();
            stepTool.Tool = tool;

            var card1 = new Card(1, DateTime.UtcNow, 1, 1, "Card 1", 1, Guid.NewGuid());
            var card2 = new Card(2, DateTime.UtcNow, 1, 1, "Card 2", 1, Guid.NewGuid());
            var cards = new List<Card> { card1, card2 };

            var step = WorkflowFixture.FindValidStep();
            step.StepTools = new List<StepTool> { stepTool };
            step.Cards = cards;

            var automationDto = AutomationFixture.FindValidautomationServicesDto();

            var stepToolExecutionRepositoryMock = _mocker.GetMock<IStepToolExecutionRepository>();
            var failingCardServiceMock = _mocker.GetMock<IFailingCardService>();

            stepToolExecutionRepositoryMock
                .Setup(r => r.FindByStepToolIdAndCardIdAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            failingCardServiceMock
                .SetupSequence(s => s.SetFailingCard(It.IsAny<int>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Service unavailable"))
                .Returns(Task.CompletedTask)
                .ThrowsAsync(new Exception("Service unavailable"))
                .Returns(Task.CompletedTask);

            // Act
            await _service.StartExecutionByStepAsync(step, automationDto);

            // Assert
            failingCardServiceMock.Verify(
                s => s.SetFailingCard(It.IsAny<int>(), automationDto.Email),
                Times.AtLeast(2));
        }

        #endregion

        #region StartExecutionByCardAsync - Exception Handling Tests

        [Fact(DisplayName = "StartExecutionByCardAsync should mark card as failing when repository throws exception")]
        [Trait("StartExecutionByCardAsync", "Exception Handling")]
        public async Task StartExecutionByCardAsync_ShouldMarkCardAsFailingOnRepositoryException()
        {
            // Arrange
            var automationDto = AutomationFixture.FindValidautomationServicesDto();

            var stepToolRepositoryMock = _mocker.GetMock<IStepToolRepository>();
            var failingCardServiceMock = _mocker.GetMock<IFailingCardService>();

            stepToolRepositoryMock
                .Setup(r => r.FindByStepIdAndOrderAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ThrowsAsync(new TimeoutException("Repository timeout"));

            failingCardServiceMock
                .Setup(s => s.SetFailingCard(automationDto.CardId, automationDto.Email))
                .Returns(Task.CompletedTask);

            // Act
            await _service.StartExecutionByCardAsync(automationDto);

            // Assert
            failingCardServiceMock.Verify(
                s => s.SetFailingCard(automationDto.CardId, automationDto.Email),
                Times.Once,
                "Card should be marked as failing when repository throws");
        }

        [Fact(DisplayName = "StartExecutionByCardAsync should swallow exceptions from FailingCardService")]
        [Trait("StartExecutionByCardAsync", "Exception Handling")]
        public async Task StartExecutionByCardAsync_ShouldSwallowFailingCardServiceException()
        {
            // Arrange
            var automationDto = AutomationFixture.FindValidautomationServicesDto();

            var stepToolRepositoryMock = _mocker.GetMock<IStepToolRepository>();
            var failingCardServiceMock = _mocker.GetMock<IFailingCardService>();

            stepToolRepositoryMock
                .Setup(r => r.FindByStepIdAndOrderAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ThrowsAsync(new Exception("Repository error"));

            failingCardServiceMock
                .Setup(s => s.SetFailingCard(It.IsAny<int>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Failing card service error"));

            // Act
            var exception = await Record.ExceptionAsync(() => _service.StartExecutionByCardAsync(automationDto));

            // Assert
            Assert.Null(exception);
        }

        #endregion

        #region ContinueExecution - Exception Handling Tests

        [Fact(DisplayName = "ContinueExecution should mark card as failing when repository throws exception")]
        [Trait("ContinueExecution", "Exception Handling")]
        public async Task ContinueExecution_ShouldMarkCardAsFailingOnRepositoryException()
        {
            // Arrange
            var automationDto = AutomationFixture.FindValidautomationServicesDto();

            var cardRepositoryMock = _mocker.GetMock<ICardRepository>();
            var failingCardServiceMock = _mocker.GetMock<IFailingCardService>();

            cardRepositoryMock
                .Setup(r => r.FindByIdWithStatus(automationDto.CardId))
                .ThrowsAsync(new InvalidOperationException("Card repository error"));

            failingCardServiceMock
                .Setup(s => s.SetFailingCard(automationDto.CardId, automationDto.Email))
                .Returns(Task.CompletedTask);

            // Act
            await _service.ContinueExecution(automationDto);

            // Assert
            failingCardServiceMock.Verify(
                s => s.SetFailingCard(automationDto.CardId, automationDto.Email),
                Times.Once);
        }

        [Fact(DisplayName = "ContinueExecution should mark card as failing when message publisher throws")]
        [Trait("ContinueExecution", "Exception Handling")]
        public async Task ContinueExecution_ShouldMarkCardAsFailingWhenPublisherThrows()
        {
            // Arrange
            var stepTool = AutomationFixture.FindValidStepTool();
            var dependentStepTool = AutomationFixture.FindValidStepTool(5000);
            var tool = ToolFixture.FindValidToolModel();
            tool.ToolType = ToolTypeFixture.FindValidToolType();
            stepTool.Tool = tool;
            dependentStepTool.Tool = tool;

            var automationDto = AutomationFixture.FindValidautomationServicesDto();
            var stepToolExecution = AutomationFixture.FindValidStepToolExecution();
            var payload = AutomationFixture.FindValidExecutionMessageDto();

            var card = new Card(automationDto.CardId, DateTime.UtcNow, 1, 1, "Test Card", 1, Guid.NewGuid());

            var cardRepositoryMock = _mocker.GetMock<ICardRepository>();
            var stepToolRepositoryMock = _mocker.GetMock<IStepToolRepository>();
            var stepToolExecutionRepositoryMock = _mocker.GetMock<IStepToolExecutionRepository>();
            var toolFactoryHandlerMock = _mocker.GetMock<IToolFactoryHandler>();
            var handlerMock = _mocker.GetMock<IToolHandler>();
            var messagePublisherMock = _mocker.GetMock<IMessagePublisher<object>>();
            var failingCardServiceMock = _mocker.GetMock<IFailingCardService>();

            cardRepositoryMock.Setup(r => r.FindByIdWithStatus(automationDto.CardId)).ReturnsAsync(card);
            stepToolRepositoryMock.Setup(r => r.FindDependentAsync(automationDto.StepToolId)).ReturnsAsync(dependentStepTool);
            stepToolExecutionRepositoryMock.Setup(r => r.FindByStepToolIdAndCardIdAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(stepToolExecution);
            stepToolExecutionRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<StepToolExecution>())).Returns(Task.CompletedTask);
            toolFactoryHandlerMock.Setup(s => s.GetHandler(It.IsAny<string>())).Returns(handlerMock.Object);
            handlerMock.Setup(h => h.BuildPayload(It.IsAny<AutomationServicesDto>(), It.IsAny<StepToolParameter>(), It.IsAny<List<StepToolOutput>>(), It.IsAny<StepToolExecution>())).ReturnsAsync(payload);

            messagePublisherMock
                .Setup(m => m.PublishAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ThrowsAsync(new Exception("Message publisher error"));

            failingCardServiceMock
                .Setup(s => s.SetFailingCard(automationDto.CardId, automationDto.Email))
                .Returns(Task.CompletedTask);

            // Act
            await _service.ContinueExecution(automationDto);

            // Assert
            failingCardServiceMock.Verify(
                s => s.SetFailingCard(automationDto.CardId, automationDto.Email),
                Times.Once);
        }

        #endregion

        #region ReprocessStepTool - Exception Handling Tests

        [Fact(DisplayName = "ReprocessStepTool should throw AppException when StepId is null or zero")]
        [Trait("ReprocessStepTool", "Exception Handling")]
        public async Task ReprocessStepTool_ShouldThrowAppExceptionWhenStepIdIsNull()
        {
            // Arrange
            var automationDto = new AutomationServicesDto(1, 1, "tenant", "email", "ref", null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _service.ReprocessStepTool(automationDto));
            Assert.Equal(ErrorCode.InvalidValue, exception.ErrorCode);
            Assert.Contains("StepId is required", exception.Message);
        }

        [Fact(DisplayName = "ReprocessStepTool should mark card as failing when repository throws exception")]
        [Trait("ReprocessStepTool", "Exception Handling")]
        public async Task ReprocessStepTool_ShouldMarkCardAsFailingOnRepositoryException()
        {
            // Arrange
            var automationDto = AutomationFixture.FindValidautomationServicesDto();

            var stepToolRepositoryMock = _mocker.GetMock<IStepToolRepository>();
            var failingCardServiceMock = _mocker.GetMock<IFailingCardService>();

            stepToolRepositoryMock
                .Setup(r => r.FindNextPending(automationDto.StepId!.Value, automationDto.CardId))
                .ThrowsAsync(new TimeoutException("Database timeout"));

            failingCardServiceMock
                .Setup(s => s.SetFailingCard(automationDto.CardId, automationDto.Email))
                .Returns(Task.CompletedTask);

            // Act
            await _service.ReprocessStepTool(automationDto);

            // Assert
            failingCardServiceMock.Verify(
                s => s.SetFailingCard(automationDto.CardId, automationDto.Email),
                Times.Once);
        }

        [Fact(DisplayName = "ReprocessStepTool should mark card as failing when execution update throws")]
        [Trait("ReprocessStepTool", "Exception Handling")]
        public async Task ReprocessStepTool_ShouldMarkCardAsFailingWhenExecutionUpdateThrows()
        {
            // Arrange
            var automationDto = AutomationFixture.FindValidautomationServicesDto();
            var stepTool = AutomationFixture.FindValidStepTool();
            var stepToolExecution = AutomationFixture.FindValidStepToolExecution();

            var stepToolRepositoryMock = _mocker.GetMock<IStepToolRepository>();
            var stepToolExecutionRepositoryMock = _mocker.GetMock<IStepToolExecutionRepository>();
            var failingCardServiceMock = _mocker.GetMock<IFailingCardService>();

            stepToolRepositoryMock
                .Setup(r => r.FindNextPending(automationDto.StepId!.Value, automationDto.CardId))
                .ReturnsAsync(stepTool);

            stepToolExecutionRepositoryMock
                .Setup(r => r.FindByStepToolIdAndCardIdAsync(stepTool.Id, automationDto.CardId))
                .ReturnsAsync(stepToolExecution);

            stepToolExecutionRepositoryMock
                .Setup(r => r.UpdateAsync(It.IsAny<StepToolExecution>()))
                .ThrowsAsync(new InvalidOperationException("Execution update failed"));

            failingCardServiceMock
                .Setup(s => s.SetFailingCard(automationDto.CardId, automationDto.Email))
                .Returns(Task.CompletedTask);

            // Act
            await _service.ReprocessStepTool(automationDto);

            // Assert
            failingCardServiceMock.Verify(
                s => s.SetFailingCard(automationDto.CardId, automationDto.Email),
                Times.Once);
        }

        #endregion
    }
}
