using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;
using Newtonsoft.Json;
using WoopiAiHub.Application.ToolsHandler;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.DTOs.Response.OpenAiResponses;
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
        private readonly ResponseOpenAiSettings _responseOpenAiSettings;
        public PromptHandlerTests()
        {
            _mocker = new AutoMocker();
            _messageQueues = new MessageQueues
            {
                OpenAiResponseQueueAiHubResponse = "test-queue",
                OpenAiResponseQueue = "test-queue2",
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
            _responseOpenAiSettings = new ResponseOpenAiSettings
            {
                Temperature = 0,
                Model = "gpt-4",
                ApiVersion = "",
                McpAddress = "",
            };
            _mocker.Use<IOptions<ChatCompletionSettings>>(Options.Create(_chatCompletionSettings));
            _mocker.Use<IOptions<ResponseOpenAiSettings>>(Options.Create(_responseOpenAiSettings));


            _handler = _mocker.CreateInstance<PromptHandler>();
        }

        [Fact(DisplayName = "BuildPayload should return ExecutionMessageDto with correct queue and message")]
        [Trait("BuildPayload", "Success")]
        public async Task BuildPayload_ShouldReturnExecutionMessageDto_WithCorrectQueueAndMessage()
        {
            // Arrange
            var automationServicesDto = AutomationFixture.FindValidAutomationServicesDto();

            var tenantInfo = new TenantInfoDto
            {
                AiGatewayApplicationId = Guid.NewGuid(),
                AiGatewayKey = "key"
            };

            var stepToolParameter = new StepToolParameter(1, DateTime.Now, 2, true, Guid.NewGuid(), "6");
            var documentEmbeddingsDataDto = MessagingFixture.FindValidDocumentEmbeddingsDataDto();
            var output = AutomationFixture.FindValidStepToolOutput(JsonConvert.SerializeObject(documentEmbeddingsDataDto));

            _mockTenantCacheServices
                .Setup(service => service.FindTenantAsync(It.IsAny<string>()))
                .ReturnsAsync(tenantInfo);

            _mockPromptServices.Setup(service => service.FindById(It.IsAny<int>()))
                               .Returns(PromptFixture.FindValidPromptDto());


            _mocker
                .GetMock<IApiTemplateServices>()
                .Setup(s =>
                    s
                    .FindAll(new ApiTemplateFilterDto())
                )
                .ReturnsAsync(new List<ApiTemplateDto> {
                    new ApiTemplateDto {
                        Id = 1,
                        Created = DateTime.Now,
                        Name = "Api 1",
                        Method = "GET",
                        Url = "http://localhost",
                        Description = "",
                        EnableAccessFromMcp = true,
                        BodyTemplate = "{}"
                    },
                    new ApiTemplateDto {
                        Id = 2,
                        Created = DateTime.Now,
                        Name = "Api 2",
                        Method = "POST",
                        Url = "http://localhost",
                        Description = "",
                        EnableAccessFromMcp = true,
                        BodyTemplate = "{}"
                    }
                });

            // Act
            var result = await _handler.BuildPayload(automationServicesDto, stepToolParameter, [output]);

            // Assert
            Assert.Equal(_messageQueues.OpenAiResponseQueue, result.Queue);
            var message = result.Message as OpenAiResponseQueryDto;
            Assert.NotNull(message);
            Assert.Equal(automationServicesDto.Tenant, message.Tenant);
            Assert.Equal(automationServicesDto.Email, message.Email);
            Assert.Equal(automationServicesDto.ReferenceFile, message.ReferenceFile);
            Assert.Equal(automationServicesDto.CardId, message.Data.CardId);
            Assert.Equal(automationServicesDto.StepToolId, message.Data.StepToolId);
        }

        [Fact(DisplayName = "BuildPayload should throw ArgumentException when AiGateway info is null or empty")]
        [Trait("BuildPayload", "Fail")]
        public async Task BuildPayload_ShouldThrowArgumentException_WhenEmbeddingModelNameIsNullOrEmpty()
        {
            // Arrange
            var automationServiceDto = AutomationFixture.FindValidAutomationServicesDto();
            var tenantInfo = new TenantInfoDto { };
            var documentEmbeddingsDataDto = MessagingFixture.FindValidDocumentEmbeddingsDataDto();
            var output = AutomationFixture.FindValidStepToolOutput(JsonConvert.SerializeObject(documentEmbeddingsDataDto));

            _mockTenantCacheServices
                .Setup(service => service.FindTenantAsync(It.IsAny<string>()))
                .ReturnsAsync(tenantInfo);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _handler.BuildPayload(automationServiceDto, It.IsAny<StepToolParameter>(), [output]));
        }

        [Fact(DisplayName = "BuildPayload should use previous Prompt output when dependency is another Prompt")]
        [Trait("BuildPayload", "Success")]
        public async Task BuildPayload_WhenDependencyIsPrompt_UsesPromptOutputAsContext()
        {
            // Arrange: dependency is another Prompt (plain text output)
            var automationServicesDto = AutomationFixture.FindValidAutomationServicesDto();
            var tenantInfo = new TenantInfoDto
            {
                AiGatewayApplicationId = Guid.NewGuid(),
                AiGatewayKey = "key"
            };
            var stepToolParameter = new StepToolParameter(1, DateTime.Now, 2, true, Guid.NewGuid(), "6");
            var previousPromptOutput = "Resumo do documento: este é o texto gerado pelo prompt anterior.";
            var output = AutomationFixture.FindValidStepToolOutput(previousPromptOutput);
            output.StepTool = new StepTool(1, DateTime.UtcNow, 1, 1, 1, 1, 1)
            {
                Tool = new Tool(1, DateTime.UtcNow, "Prompt", true, 1, 1, 1, false, null, null)
                {
                    ToolType = new ToolType(1, DateTime.UtcNow, "Prompt", string.Empty, true)
                }
            };

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
            Assert.Contains(previousPromptOutput, message!.ChatCompletion!.Messages![0].Content);
        }
    }
}
