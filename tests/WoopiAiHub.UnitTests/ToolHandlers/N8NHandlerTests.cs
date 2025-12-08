using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;
using Newtonsoft.Json;
using WoopiAiHub.Application.ToolsHandler;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs.Request.Automation;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Utils;
using WoopiAiHub.Infrastructure.Messaging.Configuration;
using WoopiAiHub.UnitTests.Fixture;
using Xunit;

namespace WoopiAiHub.UnitTests.ToolHandlers
{
    [Collection(nameof(ToolHandlerCollection))]
    public class N8NHandlerTests
    {
        private readonly AutoMocker _mocker;
        private readonly N8NHandler _handler;
        private readonly MessageQueues _messageQueues;
        private readonly Mock<IToolRepository> _mockToolRepository;

        public N8NHandlerTests()
        {
            _mocker = new AutoMocker();
            _messageQueues = new MessageQueues { EmbeddingQueue = "test-queue" };
            var options = Options.Create(_messageQueues);
            _mocker.Use<IOptions<MessageQueues>>(options);
            _mockToolRepository = _mocker.GetMock<IToolRepository>();

            _handler = _mocker.CreateInstance<N8NHandler>();
        }

        [Fact(DisplayName = "BuildPayload should throw AppException when Tool not found")]
        [Trait("BuildPayload", "Fail")]
        public async Task BuildPayload_ShouldThrowAppException_WhenToolNotFound()
        {
            // Arrange
            var automationServicesDto = AutomationFixture.FindValidAutomationServicesDto();

            _mockToolRepository
                .Setup(repo => repo.FindModelByStepToolIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Tool?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _handler.BuildPayload(automationServicesDto, null, []));
            Assert.Equal(ErrorCode.NotFound, exception.ErrorCode);
            Assert.Equal("Tool not found", exception.Message);
        }

        [Fact(DisplayName = "BuildPayload should return ExecutionMessageDto with correct queue and message")]
        [Trait("BuildPayload", "Success")]
        public async Task BuildPayload_ShouldReturnExecutionMessageDto_WithCorrectQueueAndMessage()
        {
            // Arrange
            var automationServicesDto = AutomationFixture.FindValidAutomationServicesDto();
            var stepToolExecution = AutomationFixture.FindValidStepToolExecution();
            var input = ToolHandlerFixture.FindValidStepToolParameter();
            var tool = ToolFixture.FindValidToolModel();
            var documentEmbeddingsDataDto = MessagingFixture.FindValidDocumentEmbeddingsDataDto();
            var output = AutomationFixture.FindValidStepToolOutput(JsonConvert.SerializeObject(documentEmbeddingsDataDto));
            output.StepTool = AutomationFixture.FindValidStepTool();
            output.StepTool.Tool = ToolFixture.FindValidToolModel();
            output.StepTool.Tool.ToolType = new ToolType(1, DateTime.Now, HandlersTypes.Ocr, true);

            _mockToolRepository
                .Setup(repo => repo.FindModelByStepToolIdAsync(It.IsAny<int>()))
                .ReturnsAsync(tool);

            // Act
            var result = await _handler.BuildPayload(automationServicesDto, input, [output], stepToolExecution);

            // Assert
            Assert.Equal(_messageQueues.AutomationQueueConsumer, result.Queue);
            var message = result.Message as AutomationInputDto;
            Assert.NotNull(message);
            Assert.Equal(tool.ConnectorUrl, message.Url);
            Assert.Equal(input.WebhookId!.Value.ToString(), message.WebhookId);
            Assert.Equal(input.RequiredFile, message.RequiredFile);
            Assert.Equal(automationServicesDto.Tenant, message.Tenant);
            Assert.Equal(automationServicesDto.Email, message.Email);
            Assert.Equal(_messageQueues.AutomationQueueResponse, message.ResponseQueue);
            Assert.Equal(ConnectorNames.N8N, message.Type);
            Assert.Equal(automationServicesDto.CardId, message.Data.CardId);
            Assert.Equal(automationServicesDto.StepToolId, message.Data.StepToolId);
        }
    }
}
