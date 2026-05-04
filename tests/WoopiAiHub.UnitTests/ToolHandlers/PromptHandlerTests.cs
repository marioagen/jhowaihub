using System.Reflection;
using System.Text.Json;
using Bogus;
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
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Utils;
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
        private readonly Mock<IApiTemplateServices> _mockApiTemplateServices;
        private readonly Mock<IJwtTokenServices> _mockJwtTokenServices;
        private readonly ChatCompletionSettings _chatCompletionSettings;
        private readonly OpenAiSettings _openAiSettings;
        private readonly McpSettings _mcpSettings;
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
            _mockApiTemplateServices = _mocker.GetMock<IApiTemplateServices>();
            _mockJwtTokenServices = _mocker.GetMock<IJwtTokenServices>();
            var faker = new Faker("pt_BR");
            _chatCompletionSettings = new ChatCompletionSettings
            {
                Model = "gpt-4",
                Temperature = 0.7,
                ApiVersion = "1"
            };

            _mcpSettings = new McpSettings
            {
                McpAddress = "",
                Instructions = "instructions {0} instructions",
                JWTKey = Guid.NewGuid().ToString(),
                JWTIssuer =  faker.Internet.Url(),
                JWTAudience = faker.Internet.Url(),
                JWTUser = faker.Internet.UserName(),
                JWTExpirationTime = 5
            };

            _openAiSettings = new OpenAiSettings
            {
                Temperature = 0,
                Model = "gpt-4",
                ApiVersion = "",

            };
            _mocker.Use<IOptions<ChatCompletionSettings>>(Options.Create(_chatCompletionSettings));
            _mocker.Use<IOptions<McpSettings>>(Options.Create(_mcpSettings));
            _mocker.Use<IOptions<OpenAiSettings>>(Options.Create(_openAiSettings));

            _handler = _mocker.CreateInstance<PromptHandler>();
        }

        [Fact(DisplayName = "Type should return Prompt handler type")]
        [Trait("PromptHandler", "Metadata")]
        public void Type_ShouldReturnPrompt()
        {
            // Arrange
            // Act
            var type = _handler.Type;

            // Assert
            Assert.Equal(HandlersTypes.Prompt, type);
        }

        private static (PromptHandler Handler, AutoMocker Mocker) CreatePromptHandlerWithMcpSettings(McpSettings mcpSettings)
        {
            var mocker = new AutoMocker();
            var messageQueues = new MessageQueues
            {
                OpenAiResponseQueueAiHubResponse = "test-queue",
                OpenAiResponseQueue = "test-queue2",
            };
            mocker.Use<IOptions<MessageQueues>>(Options.Create(messageQueues));
            mocker.Use<IOptions<OpenAiSettings>>(Options.Create(new OpenAiSettings
            {
                Temperature = 0,
                Model = "gpt-4",
                ApiVersion = "",
            }));
            mocker.Use<IOptions<McpSettings>>(Options.Create(mcpSettings));
            return (mocker.CreateInstance<PromptHandler>(), mocker);
        }

        private static StepToolOutput CreateStepToolOutput(string toolType, string value)
        {
            var output = AutomationFixture.FindValidStepToolOutput(value);
            output.StepTool = new StepTool(1, DateTime.UtcNow, 1, 1, 1, 1, 1)
            {
                Tool = new Tool(1, DateTime.UtcNow, "Tool", true, 1, 1, 1, false, null, null)
                {
                    ToolType = new ToolType(1, DateTime.UtcNow, toolType, string.Empty, true)
                }
            };
            return output;
        }

        private static string ExtractMappedApisJsonFromInstructions(string fullInstructions)
        {
            const string prefix = "instructions ";
            const string suffix = " instructions";
            var start = fullInstructions.IndexOf(prefix, StringComparison.Ordinal);
            var end = fullInstructions.IndexOf(suffix, start + prefix.Length, StringComparison.Ordinal);
            if (start < 0 || end < 0)
                throw new InvalidOperationException("Unexpected instructions format: " + fullInstructions);
            return fullInstructions.Substring(start + prefix.Length, end - start - prefix.Length);
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

            _mockApiTemplateServices
                .Setup(s =>s.FindAll(It.IsAny<ApiTemplateFilterDto>()))
                .ReturnsAsync(MessagingFixture.FindValidListApiTemplateDto(string.Empty));

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

        [Fact(DisplayName = "BuildPayload should throw ArgumentException when AiGateway application id or key is missing")]
        [Trait("BuildPayload", "Fail")]
        public async Task BuildPayload_ShouldThrowArgumentException_WhenAiGatewayApplicationIdOrKeyMissing()
        {
            // Arrange
            var automationServiceDto = AutomationFixture.FindValidAutomationServicesDto();
            var stepToolParameter = new StepToolParameter(1, DateTime.Now, 2, true, Guid.NewGuid(), "6");
            var tenantInfo = new TenantInfoDto { };
            var documentEmbeddingsDataDto = MessagingFixture.FindValidDocumentEmbeddingsDataDto();
            var output = AutomationFixture.FindValidStepToolOutput(JsonConvert.SerializeObject(documentEmbeddingsDataDto));

            _mockTenantCacheServices
                .Setup(service => service.FindTenantAsync(It.IsAny<string>()))
                .ReturnsAsync(tenantInfo);

            // Act
            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                _handler.BuildPayload(automationServiceDto, stepToolParameter, [output]));

            // Assert
            Assert.Contains("AiGateway", ex.Message, StringComparison.Ordinal);
        }

        [Fact(DisplayName = "BuildPayload should throw ArgumentException when AiGateway key is empty")]
        [Trait("BuildPayload", "Fail")]
        public async Task BuildPayload_ShouldThrowArgumentException_WhenAiGatewayKeyIsEmpty()
        {
            // Arrange
            var automationServiceDto = AutomationFixture.FindValidAutomationServicesDto();
            var stepToolParameter = new StepToolParameter(1, DateTime.Now, 2, true, Guid.NewGuid(), "6");
            var tenantInfo = new TenantInfoDto
            {
                AiGatewayApplicationId = Guid.NewGuid(),
                AiGatewayKey = ""
            };
            var output = AutomationFixture.FindValidStepToolOutput(
                JsonConvert.SerializeObject(MessagingFixture.FindValidDocumentEmbeddingsDataDto()));

            _mockTenantCacheServices
                .Setup(service => service.FindTenantAsync(It.IsAny<string>()))
                .ReturnsAsync(tenantInfo);

            // Act
            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                _handler.BuildPayload(automationServiceDto, stepToolParameter, [output]));

            // Assert
            Assert.Contains("AiGateway", ex.Message, StringComparison.Ordinal);
        }

        [Fact(DisplayName = "BuildPayload should use previous Prompt output when dependency is another Prompt")]
        [Trait("BuildPayload", "Success")]
        public async Task BuildPayload_WhenDependencyIsPrompt_UsesPromptOutputAsContext()
        {
            // Arrange
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
            _mockApiTemplateServices
                .Setup(s =>s.FindAll(It.IsAny<ApiTemplateFilterDto>()))
                .ReturnsAsync(MessagingFixture.FindValidListApiTemplateDto(string.Empty));

            // Act
            var result = await _handler.BuildPayload(automationServicesDto, stepToolParameter, [output]);

            // Assert
            Assert.Equal(_messageQueues.OpenAiResponseQueue, result.Queue);
            var message = result.Message as OpenAiResponseQueryDto;
            Assert.NotNull(message);
            Assert.Contains(previousPromptOutput, message!.OpenAiResponse!.Input![0].Content[0].Text);
        }

        [Fact(DisplayName = "Verify And Add Or Not Mcp Support When EnableAccessToMcp Is False Should Do Nothing")]
        [Trait("BuildPayload", "Success")]
        public async Task VerifyAndAddOrNotMcpSupport_WhenEnableAccessToMcpIsFalse_ShouldDoNothing()
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
                               .Returns(PromptFixture.FindValidPromptDto(false));

            _mockApiTemplateServices
                .Setup(s =>s.FindAll(It.IsAny<ApiTemplateFilterDto>()))
                .ReturnsAsync(MessagingFixture.FindValidListApiTemplateDto(string.Empty));


            // Act
            var result = await _handler.BuildPayload(automationServicesDto, stepToolParameter, [output]);

            // Assert
            Assert.IsType<OpenAiResponseQueryDto>(result.Message);
            var message = (OpenAiResponseQueryDto)result.Message;

            Assert.NotNull(message.OpenAiResponse);
            Assert.Empty(message.OpenAiResponse.Instructions);
            Assert.Empty(((OpenAiResponseQueryDto)result.Message).OpenAiResponse.Instructions);
            Assert.Equal(0, message.OpenAiResponse.MaxToolCalls);
            Assert.Empty(message.OpenAiResponse.Tools);
        }

        [Fact(DisplayName = "Verify And Add Or Not Mcp Support When EnableAccessToMcp Is True Should Do add instructions")]
        [Trait("BuildPayload", "Success")]
        public async Task VerifyAndAddOrNotMcpSupport_WhenEnabledAndApisExist_ShouldConfigureDto()
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

            var promptDto = PromptFixture.FindValidPromptDto();
            _mockPromptServices.Setup(service => service.FindById(It.IsAny<int>()))
                               .Returns(promptDto);

            var apis = MessagingFixture.FindValidListApiTemplateDto(string.Empty);
            _mockApiTemplateServices
                .Setup(s =>s.FindAll(It.IsAny<ApiTemplateFilterDto>()))
                .ReturnsAsync(apis);


            _mockJwtTokenServices
                .Setup(x =>
                    x.GenerateTokenWithParameters(
                        _mcpSettings.JWTKey,
                        _mcpSettings.JWTIssuer,
                        _mcpSettings.JWTAudience,
                        _mcpSettings.JWTUser,
                        _mcpSettings.JWTExpirationTime
                    )
                )
                .Returns("token-test");

            // Act
            var result = await _handler.BuildPayload(automationServicesDto, stepToolParameter, [output]);

            // Assert
            Assert.IsType<OpenAiResponseQueryDto>(result.Message);
            var message = (OpenAiResponseQueryDto)result.Message;
            
            Assert.NotNull(message.OpenAiResponse);
            Assert.NotEmpty(message.OpenAiResponse.Instructions);

            Assert.Contains("instructions", message.OpenAiResponse.Instructions);
            Assert.Contains(apis[0].Url, message.OpenAiResponse.Instructions);
            Assert.Contains(apis[0].Description ?? "", message.OpenAiResponse.Instructions);
            Assert.Contains(apis[1].Description ?? "", message.OpenAiResponse.Instructions);

            Assert.Equal(_mcpSettings.MaxToolCalls, message.OpenAiResponse.MaxToolCalls);

            Assert.NotNull(message.OpenAiResponse.Tools);
            Assert.Single(message.OpenAiResponse.Tools);

            var tool = message.OpenAiResponse.Tools[0];
            Assert.Equal(OpenAiResponseToolsType.Mcp, tool.Type);
            Assert.Single(tool.AllowedTools);
            Assert.Equal("generalist", tool.AllowedTools[0]);

            Assert.NotNull(tool.Headers);
            Assert.Equal("Bearer token-test", tool.Headers["Authorization"]);
        }

        [Fact(DisplayName = "BuildPayload MCP instructions embed headers object when HeaderTemplate has key/value pairs")]
        [Trait("ExtractHeadersValues", "Integration")]
        public async Task BuildPayload_WhenMcpEnabled_HeaderTemplateWithPairs_EmbedsHeadersInInstructionsJson()
        {
            // Arrange
            var automationServicesDto = AutomationFixture.FindValidAutomationServicesDto();
            var tenantInfo = new TenantInfoDto { AiGatewayApplicationId = Guid.NewGuid(), AiGatewayKey = "key" };
            var stepToolParameter = new StepToolParameter(1, DateTime.Now, 2, true, Guid.NewGuid(), "6");
            var output = AutomationFixture.FindValidStepToolOutput(
                JsonConvert.SerializeObject(MessagingFixture.FindValidDocumentEmbeddingsDataDto()));

            var apis = MessagingFixture.FindValidListApiTemplateDto("""[{"key":"Authorization","value":"Bearer custom-from-template"},{"key":"X-Trace","value":"t1"}]""");

            _mockTenantCacheServices.Setup(s => s.FindTenantAsync(It.IsAny<string>())).ReturnsAsync(tenantInfo);
            _mockPromptServices.Setup(s => s.FindById(It.IsAny<int>())).Returns(PromptFixture.FindValidPromptDto());
            _mockApiTemplateServices.Setup(s => s.FindAll(It.IsAny<ApiTemplateFilterDto>())).ReturnsAsync(apis);
            _mockJwtTokenServices
                .Setup(x => x.GenerateTokenWithParameters(
                    _mcpSettings.JWTKey,
                    _mcpSettings.JWTIssuer,
                    _mcpSettings.JWTAudience,
                    _mcpSettings.JWTUser,
                    _mcpSettings.JWTExpirationTime))
                .Returns("token-test");

            // Act
            var result = await _handler.BuildPayload(automationServicesDto, stepToolParameter, [output]);
            var message = result.Message as OpenAiResponseQueryDto;

            // Assert
            var apisJson = ExtractMappedApisJsonFromInstructions(message?.OpenAiResponse!.Instructions!);
            using var doc = JsonDocument.Parse(apisJson);
            var first = doc.RootElement.EnumerateArray().First();
            var headers = first.GetProperty("headers");
            Assert.Equal("Bearer custom-from-template", headers.GetProperty("Authorization").GetString());
            Assert.Equal("t1", headers.GetProperty("X-Trace").GetString());
        }

        [Fact(DisplayName = "BuildPayload MCP instructions use empty headers when HeaderTemplate is null or empty")]
        [Trait("ExtractHeadersValues", "Integration")]
        public async Task BuildPayload_WhenMcpEnabled_HeaderTemplateNullOrEmpty_InstructionsHaveEmptyHeadersObject()
        {
            // Arrange
            var automationServicesDto = AutomationFixture.FindValidAutomationServicesDto();
            var tenantInfo = new TenantInfoDto { AiGatewayApplicationId = Guid.NewGuid(), AiGatewayKey = "key" };
            var stepToolParameter = new StepToolParameter(1, DateTime.Now, 2, true, Guid.NewGuid(), "6");
            var output = AutomationFixture.FindValidStepToolOutput(
                JsonConvert.SerializeObject(MessagingFixture.FindValidDocumentEmbeddingsDataDto()));
            
            var apis = MessagingFixture.FindValidListApiTemplateDto(string.Empty);

            _mockTenantCacheServices.Setup(s => s.FindTenantAsync(It.IsAny<string>())).ReturnsAsync(tenantInfo);
            _mockPromptServices.Setup(s => s.FindById(It.IsAny<int>())).Returns(PromptFixture.FindValidPromptDto());
            _mockApiTemplateServices.Setup(s => s.FindAll(It.IsAny<ApiTemplateFilterDto>())).ReturnsAsync(apis);
            _mockJwtTokenServices
                .Setup(x => x.GenerateTokenWithParameters(
                    _mcpSettings.JWTKey,
                    _mcpSettings.JWTIssuer,
                    _mcpSettings.JWTAudience,
                    _mcpSettings.JWTUser,
                    _mcpSettings.JWTExpirationTime))
                .Returns("token-test");

            // Act
            var result = await _handler.BuildPayload(automationServicesDto, stepToolParameter, [output]);
            var message = result.Message as OpenAiResponseQueryDto;

            // Assert
            var apisJson = ExtractMappedApisJsonFromInstructions(message?.OpenAiResponse!.Instructions!);
            using var doc = JsonDocument.Parse(apisJson);
            foreach (var el in doc.RootElement.EnumerateArray())
            {
                var headers = el.GetProperty("headers");
                Assert.Equal(JsonValueKind.Object, headers.ValueKind);
                Assert.Empty(headers.EnumerateObject());
            }
        }

        [Fact(DisplayName = "BuildPayload MCP instructions skip header entries with blank key or value")]
        [Trait("ExtractHeadersValues", "Integration")]
        public async Task BuildPayload_WhenMcpEnabled_HeaderTemplateWithBlankPairs_SkipsInvalidEntries()
        {
            // Arrange
            var automationServicesDto = AutomationFixture.FindValidAutomationServicesDto();
            var tenantInfo = new TenantInfoDto { AiGatewayApplicationId = Guid.NewGuid(), AiGatewayKey = "key" };
            var stepToolParameter = new StepToolParameter(1, DateTime.Now, 2, true, Guid.NewGuid(), "6");
            var output = AutomationFixture.FindValidStepToolOutput(
                JsonConvert.SerializeObject(MessagingFixture.FindValidDocumentEmbeddingsDataDto()));

            var apis = MessagingFixture.FindValidListApiTemplateDto("""[{"key":"","value":"v1"},{"key":"K","value":""},{"key":"Good","value":"ok"}]""");

            _mockTenantCacheServices.Setup(s => s.FindTenantAsync(It.IsAny<string>())).ReturnsAsync(tenantInfo);
            _mockPromptServices.Setup(s => s.FindById(It.IsAny<int>())).Returns(PromptFixture.FindValidPromptDto());
            _mockApiTemplateServices.Setup(s => s.FindAll(It.IsAny<ApiTemplateFilterDto>())).ReturnsAsync(apis);
            _mockJwtTokenServices
                .Setup(x => x.GenerateTokenWithParameters(
                    _mcpSettings.JWTKey,
                    _mcpSettings.JWTIssuer,
                    _mcpSettings.JWTAudience,
                    _mcpSettings.JWTUser,
                    _mcpSettings.JWTExpirationTime))
                .Returns("token-test");

            // Act
            var result = await _handler.BuildPayload(automationServicesDto, stepToolParameter, [output]);
            var message = result.Message as OpenAiResponseQueryDto;

            // Assert
            var apisJson = ExtractMappedApisJsonFromInstructions(message?.OpenAiResponse!.Instructions!);
            using var doc = JsonDocument.Parse(apisJson);
            var headers = doc.RootElement[0].GetProperty("headers");
            Assert.Single(headers.EnumerateObject());
            Assert.Equal("ok", headers.GetProperty("Good").GetString());
        }

        [Fact(DisplayName = "BuildPayload should throw AppException when outputs collection is null")]
        [Trait("BuildPayload", "Fail")]
        public async Task BuildPayload_ShouldThrowAppException_WhenOutputsNull()
        {
            // Arrange
            var automationServicesDto = AutomationFixture.FindValidAutomationServicesDto();
            var stepToolParameter = new StepToolParameter(1, DateTime.Now, 2, true, Guid.NewGuid(), "6");
            var tenantInfo = new TenantInfoDto { AiGatewayApplicationId = Guid.NewGuid(), AiGatewayKey = "key" };

            _mockTenantCacheServices.Setup(s => s.FindTenantAsync(It.IsAny<string>())).ReturnsAsync(tenantInfo);

            // Act
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                _handler.BuildPayload(automationServicesDto, stepToolParameter, null!));

            // Assert
            Assert.Equal(ErrorCode.RequiredField, ex.ErrorCode);
        }

        [Fact(DisplayName = "BuildPayload should throw AppException when outputs collection is empty")]
        [Trait("BuildPayload", "Fail")]
        public async Task BuildPayload_ShouldThrowAppException_WhenOutputsEmpty()
        {
            // Arrange
            var automationServicesDto = AutomationFixture.FindValidAutomationServicesDto();
            var stepToolParameter = new StepToolParameter(1, DateTime.Now, 2, true, Guid.NewGuid(), "6");
            var tenantInfo = new TenantInfoDto { AiGatewayApplicationId = Guid.NewGuid(), AiGatewayKey = "key" };

            _mockTenantCacheServices.Setup(s => s.FindTenantAsync(It.IsAny<string>())).ReturnsAsync(tenantInfo);

            // Act
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                _handler.BuildPayload(automationServicesDto, stepToolParameter, Array.Empty<StepToolOutput>()));

            // Assert
            Assert.Equal(ErrorCode.RequiredField, ex.ErrorCode);
        }

        [Fact(DisplayName = "BuildPayload should throw AppException when all dependency values are whitespace")]
        [Trait("BuildPayload", "Fail")]
        public async Task BuildPayload_ShouldThrowAppException_WhenOnlyWhitespaceOutputs()
        {
            // Arrange
            var automationServicesDto = AutomationFixture.FindValidAutomationServicesDto();
            var stepToolParameter = new StepToolParameter(1, DateTime.Now, 2, true, Guid.NewGuid(), "6");
            var tenantInfo = new TenantInfoDto { AiGatewayApplicationId = Guid.NewGuid(), AiGatewayKey = "key" };
            var output = CreateStepToolOutput(HandlersTypes.Prompt, "   \n  ");

            _mockTenantCacheServices.Setup(s => s.FindTenantAsync(It.IsAny<string>())).ReturnsAsync(tenantInfo);

            // Act
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                _handler.BuildPayload(automationServicesDto, stepToolParameter, [output]));

            // Assert
            Assert.Equal(ErrorCode.RequiredField, ex.ErrorCode);
        }

        [Fact(DisplayName = "BuildPayload should throw AppException when OCR output produces no extractable text")]
        [Trait("BuildPayload", "Fail")]
        public async Task BuildPayload_ShouldThrowAppException_WhenOcrOutputHasNoEmbeddings()
        {
            // Arrange
            var automationServicesDto = AutomationFixture.FindValidAutomationServicesDto();
            var stepToolParameter = new StepToolParameter(1, DateTime.Now, 2, true, Guid.NewGuid(), "6");
            var tenantInfo = new TenantInfoDto { AiGatewayApplicationId = Guid.NewGuid(), AiGatewayKey = "key" };
            var output = CreateStepToolOutput(HandlersTypes.Ocr, "{}");

            _mockTenantCacheServices.Setup(s => s.FindTenantAsync(It.IsAny<string>())).ReturnsAsync(tenantInfo);

            // Act
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                _handler.BuildPayload(automationServicesDto, stepToolParameter, [output]));

            // Assert
            Assert.Equal(ErrorCode.RequiredField, ex.ErrorCode);
        }

        [Fact(DisplayName = "BuildPayload should use Quiz dependency output as context")]
        [Trait("BuildPayload", "Success")]
        public async Task BuildPayload_WhenDependencyIsQuiz_UsesQuizOutputAsContext()
        {
            // Arrange
            var automationServicesDto = AutomationFixture.FindValidAutomationServicesDto();
            var tenantInfo = new TenantInfoDto { AiGatewayApplicationId = Guid.NewGuid(), AiGatewayKey = "key" };
            var stepToolParameter = new StepToolParameter(1, DateTime.Now, 2, true, Guid.NewGuid(), "6");
            var quizText = "  quiz-result-value  ";
            var output = CreateStepToolOutput(HandlersTypes.Quiz, quizText);

            _mockTenantCacheServices.Setup(s => s.FindTenantAsync(It.IsAny<string>())).ReturnsAsync(tenantInfo);
            _mockPromptServices.Setup(s => s.FindById(It.IsAny<int>())).Returns(PromptFixture.FindValidPromptDto(false));
            _mockApiTemplateServices.Setup(s => s.FindAll(It.IsAny<ApiTemplateFilterDto>()))
                .ReturnsAsync(MessagingFixture.FindValidListApiTemplateDto(string.Empty));

            // Act
            var result = await _handler.BuildPayload(automationServicesDto, stepToolParameter, [output]);
            var message = result.Message as OpenAiResponseQueryDto;

            // Assert
            Assert.Contains("quiz-result-value", message!.OpenAiResponse!.Input![0].Content[0].Text);
        }

        [Fact(DisplayName = "BuildPayload should flatten JSON object from API dependency into prompt context")]
        [Trait("BuildPayload", "Success")]
        public async Task BuildPayload_WhenDependencyIsApi_WithJsonObject_FlattensIntoPromptContext()
        {
            // Arrange
            var automationServicesDto = AutomationFixture.FindValidAutomationServicesDto();
            var tenantInfo = new TenantInfoDto { AiGatewayApplicationId = Guid.NewGuid(), AiGatewayKey = "key" };
            var stepToolParameter = new StepToolParameter(1, DateTime.Now, 2, true, Guid.NewGuid(), "6");
            var apiJson =
                """{"":"skip-empty-name","a":1,"msg":"line","esc":"a\"b","slash":"x\\y","n":42,"ok":false,"z":null,"flags":[true,false],"arr":[1,2],"obj":{"k":1}}""";
            var output = CreateStepToolOutput(HandlersTypes.API, apiJson);

            _mockTenantCacheServices.Setup(s => s.FindTenantAsync(It.IsAny<string>())).ReturnsAsync(tenantInfo);
            _mockPromptServices.Setup(s => s.FindById(It.IsAny<int>())).Returns(PromptFixture.FindValidPromptDto(false));
            _mockApiTemplateServices.Setup(s => s.FindAll(It.IsAny<ApiTemplateFilterDto>()))
                .ReturnsAsync(MessagingFixture.FindValidListApiTemplateDto(string.Empty));

            // Act
            var result = await _handler.BuildPayload(automationServicesDto, stepToolParameter, [output]);
            var message = result.Message as OpenAiResponseQueryDto;
            var text = message!.OpenAiResponse!.Input![0].Content[0].Text;

            // Assert
            Assert.Contains("A: 1", text);
            Assert.Contains("Msg: \"line\"", text);
            Assert.Contains("Esc: \"a\\\"b\"", text);
            Assert.Contains("Slash: \"x\\\\y\"", text);
            Assert.Contains("N: 42", text);
            Assert.Contains("Ok: false", text);
            Assert.Contains("Z: null", text);
            Assert.Contains("Flags: true, false", text);
            Assert.Contains("Arr: 1, 2", text);
            Assert.Contains("Obj: {\"k\":1}", text);
        }

        [Fact(DisplayName = "BuildPayload should use {} when API dependency JSON object has no properties")]
        [Trait("BuildPayload", "Success")]
        public async Task BuildPayload_WhenDependencyIsApi_WithEmptyObject_ReturnsEmptyObjectToken()
        {
            // Arrange
            var automationServicesDto = AutomationFixture.FindValidAutomationServicesDto();
            var tenantInfo = new TenantInfoDto { AiGatewayApplicationId = Guid.NewGuid(), AiGatewayKey = "key" };
            var stepToolParameter = new StepToolParameter(1, DateTime.Now, 2, true, Guid.NewGuid(), "6");
            var output = CreateStepToolOutput(HandlersTypes.API, "{}");

            _mockTenantCacheServices.Setup(s => s.FindTenantAsync(It.IsAny<string>())).ReturnsAsync(tenantInfo);
            _mockPromptServices.Setup(s => s.FindById(It.IsAny<int>())).Returns(PromptFixture.FindValidPromptDto(false));
            _mockApiTemplateServices.Setup(s => s.FindAll(It.IsAny<ApiTemplateFilterDto>()))
                .ReturnsAsync(MessagingFixture.FindValidListApiTemplateDto(string.Empty));

            // Act
            var result = await _handler.BuildPayload(automationServicesDto, stepToolParameter, [output]);
            var message = result.Message as OpenAiResponseQueryDto;
            var text = message!.OpenAiResponse!.Input![0].Content[0].Text;

            // Assert
            Assert.Contains("Baseado no: \"{}\"", text);
        }

        [Fact(DisplayName = "BuildPayload should throw ArgumentException when API dependency value is invalid JSON")]
        [Trait("BuildPayload", "Fail")]
        public async Task BuildPayload_WhenDependencyIsApi_WithInvalidJson_ThrowsArgumentException()
        {
            // Arrange
            var automationServicesDto = AutomationFixture.FindValidAutomationServicesDto();
            var tenantInfo = new TenantInfoDto { AiGatewayApplicationId = Guid.NewGuid(), AiGatewayKey = "key" };
            var stepToolParameter = new StepToolParameter(1, DateTime.Now, 2, true, Guid.NewGuid(), "6");
            var output = CreateStepToolOutput(HandlersTypes.API, "{not-json");

            _mockTenantCacheServices.Setup(s => s.FindTenantAsync(It.IsAny<string>())).ReturnsAsync(tenantInfo);

            // Act
            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                _handler.BuildPayload(automationServicesDto, stepToolParameter, [output]));

            // Assert
            Assert.Equal("The API response is not a valid JSON object", ex.Message);
        }

        [Fact(DisplayName = "BuildPayload should pass through raw JSON array from API dependency")]
        [Trait("BuildPayload", "Success")]
        public async Task BuildPayload_WhenDependencyIsApi_WithJsonArray_PassesRawValue()
        {
            // Arrange
            var automationServicesDto = AutomationFixture.FindValidAutomationServicesDto();
            var tenantInfo = new TenantInfoDto { AiGatewayApplicationId = Guid.NewGuid(), AiGatewayKey = "key" };
            var stepToolParameter = new StepToolParameter(1, DateTime.Now, 2, true, Guid.NewGuid(), "6");
            var output = CreateStepToolOutput(HandlersTypes.API, "[1,2]");

            _mockTenantCacheServices.Setup(s => s.FindTenantAsync(It.IsAny<string>())).ReturnsAsync(tenantInfo);
            _mockPromptServices.Setup(s => s.FindById(It.IsAny<int>())).Returns(PromptFixture.FindValidPromptDto(false));
            _mockApiTemplateServices.Setup(s => s.FindAll(It.IsAny<ApiTemplateFilterDto>()))
                .ReturnsAsync(MessagingFixture.FindValidListApiTemplateDto(string.Empty));

            // Act
            var result = await _handler.BuildPayload(automationServicesDto, stepToolParameter, [output]);
            var message = result.Message as OpenAiResponseQueryDto;

            // Assert
            Assert.Contains("[1,2]", message!.OpenAiResponse!.Input![0].Content[0].Text);
        }

        [Fact(DisplayName = "BuildPayload should pass through JSON string literal from API dependency")]
        [Trait("BuildPayload", "Success")]
        public async Task BuildPayload_WhenDependencyIsApi_WithJsonStringLiteral_PassesRawValue()
        {
            // Arrange
            var automationServicesDto = AutomationFixture.FindValidAutomationServicesDto();
            var tenantInfo = new TenantInfoDto { AiGatewayApplicationId = Guid.NewGuid(), AiGatewayKey = "key" };
            var stepToolParameter = new StepToolParameter(1, DateTime.Now, 2, true, Guid.NewGuid(), "6");
            var output = CreateStepToolOutput(HandlersTypes.API, "\"hello\"");

            _mockTenantCacheServices.Setup(s => s.FindTenantAsync(It.IsAny<string>())).ReturnsAsync(tenantInfo);
            _mockPromptServices.Setup(s => s.FindById(It.IsAny<int>())).Returns(PromptFixture.FindValidPromptDto(false));
            _mockApiTemplateServices.Setup(s => s.FindAll(It.IsAny<ApiTemplateFilterDto>()))
                .ReturnsAsync(MessagingFixture.FindValidListApiTemplateDto(string.Empty));

            // Act
            var result = await _handler.BuildPayload(automationServicesDto, stepToolParameter, [output]);
            var message = result.Message as OpenAiResponseQueryDto;

            // Assert
            Assert.Contains("\"hello\"", message!.OpenAiResponse!.Input![0].Content[0].Text);
        }

        [Fact(DisplayName = "BuildPayload should trim unknown tool type output as plain context")]
        [Trait("BuildPayload", "Success")]
        public async Task BuildPayload_WhenDependencyIsUnknownTool_UsesTrimmedValueAsContext()
        {
            // Arrange
            var automationServicesDto = AutomationFixture.FindValidAutomationServicesDto();
            var tenantInfo = new TenantInfoDto { AiGatewayApplicationId = Guid.NewGuid(), AiGatewayKey = "key" };
            var stepToolParameter = new StepToolParameter(1, DateTime.Now, 2, true, Guid.NewGuid(), "6");
            var output = CreateStepToolOutput(HandlersTypes.N8N, "  n8n-plain  ");

            _mockTenantCacheServices.Setup(s => s.FindTenantAsync(It.IsAny<string>())).ReturnsAsync(tenantInfo);
            _mockPromptServices.Setup(s => s.FindById(It.IsAny<int>())).Returns(PromptFixture.FindValidPromptDto(false));
            _mockApiTemplateServices.Setup(s => s.FindAll(It.IsAny<ApiTemplateFilterDto>()))
                .ReturnsAsync(MessagingFixture.FindValidListApiTemplateDto(string.Empty));

            // Act
            var result = await _handler.BuildPayload(automationServicesDto, stepToolParameter, [output]);
            var message = result.Message as OpenAiResponseQueryDto;

            // Assert
            Assert.Contains("n8n-plain", message!.OpenAiResponse!.Input![0].Content[0].Text);
        }

        [Fact(DisplayName = "BuildPayload should skip whitespace-only outputs and join multiple parts")]
        [Trait("BuildPayload", "Success")]
        public async Task BuildPayload_ShouldSkipWhitespaceOnlyOutputs_AndJoinMultipleParts()
        {
            // Arrange
            var automationServicesDto = AutomationFixture.FindValidAutomationServicesDto();
            var tenantInfo = new TenantInfoDto { AiGatewayApplicationId = Guid.NewGuid(), AiGatewayKey = "key" };
            var stepToolParameter = new StepToolParameter(1, DateTime.Now, 2, true, Guid.NewGuid(), "6");
            var blank = CreateStepToolOutput(HandlersTypes.Prompt, "  \t  ");
            var first = CreateStepToolOutput(HandlersTypes.Prompt, "first-block");
            var second = CreateStepToolOutput(HandlersTypes.Prompt, "second-block");

            _mockTenantCacheServices.Setup(s => s.FindTenantAsync(It.IsAny<string>())).ReturnsAsync(tenantInfo);
            _mockPromptServices.Setup(s => s.FindById(It.IsAny<int>())).Returns(PromptFixture.FindValidPromptDto(false));
            _mockApiTemplateServices.Setup(s => s.FindAll(It.IsAny<ApiTemplateFilterDto>()))
                .ReturnsAsync(MessagingFixture.FindValidListApiTemplateDto(string.Empty));

            // Act
            var result = await _handler.BuildPayload(automationServicesDto, stepToolParameter, [blank, first, second]);
            var message = result.Message as OpenAiResponseQueryDto;
            var text = message!.OpenAiResponse!.Input![0].Content[0].Text;

            // Assert
            Assert.Contains("first-block", text);
            Assert.Contains("second-block", text);
            Assert.Contains("first-block\n\nsecond-block", text);
        }

        [Fact(DisplayName = "BuildPayload should throw ArgumentException when MCP is enabled but Instructions are empty")]
        [Trait("BuildPayload", "Fail")]
        public async Task BuildPayload_WhenMcpEnabledAndMcpInstructionsEmpty_ThrowsArgumentException()
        {
            // Arrange
            var mcpSettings = new McpSettings
            {
                Instructions = "",
                JWTKey = Guid.NewGuid().ToString(),
                JWTIssuer = "https://issuer",
                JWTAudience = "https://audience",
                JWTUser = "user",
                JWTExpirationTime = 5,
            };
            var (handler, mocker) = CreatePromptHandlerWithMcpSettings(mcpSettings);
            var mockTenant = mocker.GetMock<ITenantCacheServices>();
            var mockPrompt = mocker.GetMock<IPromptServices>();
            var mockApi = mocker.GetMock<IApiTemplateServices>();

            var automationServicesDto = AutomationFixture.FindValidAutomationServicesDto();
            var tenantInfo = new TenantInfoDto { AiGatewayApplicationId = Guid.NewGuid(), AiGatewayKey = "key" };
            var stepToolParameter = new StepToolParameter(1, DateTime.Now, 2, true, Guid.NewGuid(), "6");
            var output = AutomationFixture.FindValidStepToolOutput(
                JsonConvert.SerializeObject(MessagingFixture.FindValidDocumentEmbeddingsDataDto()));

            mockTenant.Setup(s => s.FindTenantAsync(It.IsAny<string>())).ReturnsAsync(tenantInfo);
            mockPrompt.Setup(s => s.FindById(It.IsAny<int>())).Returns(PromptFixture.FindValidPromptDto());
            mockApi.Setup(s => s.FindAll(It.IsAny<ApiTemplateFilterDto>()))
                .ReturnsAsync(MessagingFixture.FindValidListApiTemplateDto(string.Empty));

            // Act
            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                handler.BuildPayload(automationServicesDto, stepToolParameter, [output]));

            // Assert
            Assert.Equal("The agent with a external access enabled need has the instructions filled in the appSettings", ex.Message);
        }

        [Fact(DisplayName = "BuildPayload MCP instructions map POST PUT DELETE protocols and embed body payloads")]
        [Trait("BuildPayload", "Success")]
        public async Task BuildPayload_WhenMcpEnabled_MapsPostPutDeleteMethodsAndBodies()
        {
            // Arrange
            var automationServicesDto = AutomationFixture.FindValidAutomationServicesDto();
            var tenantInfo = new TenantInfoDto { AiGatewayApplicationId = Guid.NewGuid(), AiGatewayKey = "key" };
            var stepToolParameter = new StepToolParameter(1, DateTime.Now, 2, true, Guid.NewGuid(), "6");
            var output = AutomationFixture.FindValidStepToolOutput(
                JsonConvert.SerializeObject(MessagingFixture.FindValidDocumentEmbeddingsDataDto()));

            var apis = new List<ApiTemplateDto>
            {
                new()
                {
                    Id = 101,
                    Created = DateTime.UtcNow,
                    Name = "PostApi",
                    Method = "POST",
                    Url = "https://example.com/post",
                    Description = "post-desc",
                    EnableAccessFromMcp = true,
                    HeaderTemplate = "",
                    BodyTemplate = """{"x":"post-body"}""",
                },
                new()
                {
                    Id = 102,
                    Created = DateTime.UtcNow,
                    Name = "PutApi",
                    Method = "PUT",
                    Url = "https://example.com/put",
                    Description = "put-desc",
                    EnableAccessFromMcp = true,
                    HeaderTemplate = "",
                    BodyTemplate = "",
                },
                new()
                {
                    Id = 103,
                    Created = DateTime.UtcNow,
                    Name = "DeleteApi",
                    Method = "DELETE",
                    Url = "https://example.com/del",
                    Description = "del-desc",
                    EnableAccessFromMcp = true,
                    HeaderTemplate = "",
                    BodyTemplate = null,
                },
            };

            _mockTenantCacheServices.Setup(s => s.FindTenantAsync(It.IsAny<string>())).ReturnsAsync(tenantInfo);
            _mockPromptServices.Setup(s => s.FindById(It.IsAny<int>())).Returns(PromptFixture.FindValidPromptDto());
            _mockApiTemplateServices.Setup(s => s.FindAll(It.IsAny<ApiTemplateFilterDto>())).ReturnsAsync(apis);
            _mockJwtTokenServices
                .Setup(x => x.GenerateTokenWithParameters(
                    _mcpSettings.JWTKey,
                    _mcpSettings.JWTIssuer,
                    _mcpSettings.JWTAudience,
                    _mcpSettings.JWTUser,
                    _mcpSettings.JWTExpirationTime))
                .Returns("token-test");

            // Act
            var result = await _handler.BuildPayload(automationServicesDto, stepToolParameter, [output]);
            var message = result.Message as OpenAiResponseQueryDto;
            var instructions = message!.OpenAiResponse!.Instructions!;

            // Assert
            Assert.Contains("https://example.com/post", instructions);
            Assert.Contains("https://example.com/put", instructions);
            Assert.Contains("https://example.com/del", instructions);
            Assert.Contains("\"protocol\":1", instructions);
            Assert.Contains("\"protocol\":2", instructions);
            Assert.Contains("\"protocol\":3", instructions);
            Assert.Contains("post-body", instructions);
            Assert.Contains("\"id\":101", instructions);
            Assert.Contains("\"id\":102", instructions);
            Assert.Contains("\"id\":103", instructions);
        }

        [Fact(DisplayName = "FormatApiResponseForPromptContext (reflection) returns empty for null or whitespace")]
        [Trait("PromptHandler", "Reflection")]
        public void Reflect_FormatApiResponseForPromptContext_NullOrWhitespace_ReturnsEmpty()
        {
            // Arrange
            var method = typeof(PromptHandler).GetMethod("FormatApiResponseForPromptContext",
                BindingFlags.Static | BindingFlags.NonPublic);

            // Act
            var nullResult = method!.Invoke(null, new object?[] { null });
            var whitespaceResult = method.Invoke(null, new object[] { "  \t  " });

            // Assert
            Assert.NotNull(method);
            Assert.Equal(string.Empty, nullResult);
            Assert.Equal(string.Empty, whitespaceResult);
        }

        [Fact(DisplayName = "FormatJsonValueForApiDisplay (reflection) returns empty for Undefined")]
        [Trait("PromptHandler", "Reflection")]
        public void Reflect_FormatJsonValueForApiDisplay_UndefinedValue_ReturnsEmpty()
        {
            // Arrange
            var method = typeof(PromptHandler).GetMethod("FormatJsonValueForApiDisplay",
                BindingFlags.Static | BindingFlags.NonPublic);
            var el = default(JsonElement);

            // Act
            var result = method!.Invoke(null, new object[] { el });

            // Assert
            Assert.NotNull(method);
            Assert.Equal(string.Empty, result);
        }

    }
}
