using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;
using Newtonsoft.Json;
using WoopiAiHub.Application.ToolsHandler;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Infrastructure.Messaging.Configuration;
using WoopiAiHub.UnitTests.Fixture;
using Xunit;

namespace WoopiAiHub.UnitTests.ToolHandlers
{
    [Collection(nameof(ToolHandlerCollection))]
    public class PromptHandlerTests
    {
        private readonly AutoMocker _mocker;
        private readonly PromptHandler _handler;
        private readonly MessageQueues _messageQueues;
        private readonly Mock<ITenantCacheServices> _mockTenantCacheServices;
        private readonly Mock<IPromptServices> _mockPromptServices;
        private readonly ChatCompletionSettings _chatCompletionSettings;
        public PromptHandlerTests()
        {
            _mocker = new AutoMocker();
            _messageQueues = new MessageQueues { ChatCompletionQueueAiHubResponse = "test-queue",
                                                 ChatCompletionQueue = "test-queue2"
            };
            var options = Options.Create(_messageQueues);
            _mocker.Use<IOptions<MessageQueues>>(options);
            _mockTenantCacheServices = _mocker.GetMock<ITenantCacheServices>();
            _mockPromptServices = _mocker.GetMock<IPromptServices>();
            _chatCompletionSettings = new ChatCompletionSettings
            {
                Model = "gpt-4",
                Temperature = 0.7,
                ApiVersion = "1"
            };
            _mocker.Use<IOptions<ChatCompletionSettings>>(Options.Create(_chatCompletionSettings));


            _handler = _mocker.CreateInstance<PromptHandler>();
        }

        [Fact(DisplayName = "BuildPayload should return ExecutionMessageDto with correct queue and message")]
        [Trait("BuildPayload", "Success")]
        public async Task BuildPayload_ShouldReturnExecutionMessageDto_WithCorrectQueueAndMessage()
        {
            // Arrange
            var automationServicesDto = AutomationFixture.FindValidAutomationServicesDto();

            var tenantInfo = new TenantInfoDto { };

            var stepToolParameter = new StepToolParameter(1,DateTime.Now,2,true,Guid.NewGuid(),"6");
            var documentEmbeddingsDataDto = MessagingFixture.FindValidDocumentEmbeddingsDataDto();
            var output = AutomationFixture.FindValidStepToolOutput(JsonConvert.SerializeObject(documentEmbeddingsDataDto));

            _mockTenantCacheServices
                .Setup(service => service.FindTenantAsync(It.IsAny<string>()))
                .ReturnsAsync(tenantInfo);

            _mockPromptServices.Setup(service => service.FindById(It.IsAny<int>()))
                               .Returns(PromptFixture.FindValidPromptDto());

            // Act
            var result = await _handler.BuildPayload(automationServicesDto, stepToolParameter, [output]);

            // Assert
            Assert.Equal(_messageQueues.ChatCompletionQueue, result.Queue);
            var message = result.Message as ChatCompletionQueryDto;
            Assert.NotNull(message);
            Assert.Equal(automationServicesDto.Tenant, message.Tenant);
            Assert.Equal(automationServicesDto.Email, message.Email);
            Assert.Equal(automationServicesDto.ReferenceFile, message.ReferenceFile);
            Assert.Equal(automationServicesDto.CardId, message.Data.CardId);
            Assert.Equal(automationServicesDto.StepToolId, message.Data.StepToolId);
        }
    }
}
