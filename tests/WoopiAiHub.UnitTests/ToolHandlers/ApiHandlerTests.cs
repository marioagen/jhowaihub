using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;
using System.Text.Json;
using WoopiAiHub.Application.ToolsHandler;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Request.Automation;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Utils;
using WoopiAiHub.Infrastructure.Messaging.Configuration;
using WoopiAiHub.UnitTests.Fixture;
using Xunit;

namespace WoopiAiHub.UnitTests.ToolHandlers
{
    [Collection(nameof(ToolHandlerCollection))]
    public class ApiHandlerTests
    {
        private readonly AutoMocker _mocker;
        private readonly ApiHandler _handler;
        private readonly MessageQueues _messageQueues;
        private readonly Mock<IStepToolRepository> _mockStepToolRepository;
        private readonly Mock<IApiTemplateRepository> _mockApiTemplateRepository;
        private readonly Mock<IEncryptionService> _mockEncryptionService;

        public ApiHandlerTests()
        {
            _mocker = new AutoMocker();
            _messageQueues = new MessageQueues
            {
                ApiRequestQueue = "test-api-request-queue",
                ApiRequestQueueResponse = "test-api-response-queue"
            };
            var options = Options.Create(_messageQueues);
            _mocker.Use<IOptions<MessageQueues>>(options);
            _mockStepToolRepository = _mocker.GetMock<IStepToolRepository>();
            _mockApiTemplateRepository = _mocker.GetMock<IApiTemplateRepository>();
            _mockEncryptionService = _mocker.GetMock<IEncryptionService>();

            _handler = _mocker.CreateInstance<ApiHandler>();
        }

        [Fact(DisplayName = "BuildPayload should return ExecutionMessageDto with correct queue and message")]
        [Trait("BuildPayload", "Success")]
        public async Task BuildPayload_ShouldReturnExecutionMessageDto_WithCorrectQueueAndMessage()
        {
            // Arrange
            var automationServicesDto = AutomationFixture.FindValidAutomationServicesDto();
            var stepToolDto = CreateValidStepToolDto();
            var apiTemplate = CreateValidApiTemplateDto();
            var execution = AutomationFixture.FindValidStepToolExecution();
            var outputs = new List<StepToolOutput>();

            var apiRequest = new ApiRequestDto
            {
                TemplateId = apiTemplate.Id!.Value,
                Url = apiTemplate.Url,
                Method = apiTemplate.Method,
                Body = "{{prompt}}"
            };

            var encryptedData = JsonSerializer.Serialize(apiRequest);

            _mockStepToolRepository
                .Setup(repo => repo.FindById(It.IsAny<int>()))
                .ReturnsAsync(stepToolDto);

            _mockEncryptionService
                .Setup(service => service.Decrypt(It.IsAny<string>()))
                .Returns(encryptedData);

            _mockApiTemplateRepository
                .Setup(repo => repo.FindById(It.IsAny<int>()))
                .ReturnsAsync(apiTemplate);

            // Act
            var result = await _handler.BuildPayload(automationServicesDto, null, outputs, execution);

            // Assert
            Assert.Equal(_messageQueues.ApiRequestQueue, result.Queue);
            var message = result.Message as ApiRequestDto;
            Assert.NotNull(message);
            Assert.Equal(automationServicesDto.Tenant, message.Tenant);
            Assert.Equal(automationServicesDto.Email, message.Email);
            Assert.Equal(automationServicesDto.CardId, message.Data.CardId);
            Assert.Equal(automationServicesDto.StepToolId, message.Data.StepToolId);
            Assert.Equal(_messageQueues.ApiRequestQueueResponse, message.ResponseQueue);
            Assert.Equal(execution.Id, message.ExecutionId);
            Assert.Equal(apiTemplate.Name, message.TemplateName);
        }

        [Fact(DisplayName = "BuildPayload should throw AppException when StepTool not found")]
        [Trait("BuildPayload", "Fail")]
        public async Task BuildPayload_ShouldThrowAppException_WhenStepToolNotFound()
        {
            // Arrange
            var automationServicesDto = AutomationFixture.FindValidAutomationServicesDto();
            var outputs = new List<StepToolOutput>();
            var execution = AutomationFixture.FindValidStepToolExecution();

            _mockStepToolRepository
                .Setup(repo => repo.FindById(It.IsAny<int>()))
                .ReturnsAsync((StepToolDto?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() =>
                _handler.BuildPayload(automationServicesDto, null, outputs, execution));

            Assert.Equal(ErrorCode.NotFound, exception.ErrorCode);
            Assert.Contains("StepTool not found", exception.Message);
        }

        [Fact(DisplayName = "BuildPayload should throw AppException when Tool type is invalid")]
        [Trait("BuildPayload", "Fail")]
        public async Task BuildPayload_ShouldThrowAppException_WhenToolTypeIsInvalid()
        {
            // Arrange
            var automationServicesDto = AutomationFixture.FindValidAutomationServicesDto();
            var stepToolDto = CreateValidStepToolDto();
            stepToolDto.Tool!.ToolType = "INVALID_TYPE";
            var outputs = new List<StepToolOutput>();
            var execution = AutomationFixture.FindValidStepToolExecution();

            _mockStepToolRepository
                .Setup(repo => repo.FindById(It.IsAny<int>()))
                .ReturnsAsync(stepToolDto);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() =>
                _handler.BuildPayload(automationServicesDto, null, outputs, execution));

            Assert.Equal(ErrorCode.InvalidValue, exception.ErrorCode);
            Assert.Contains("Invalid tool type for API handler", exception.Message);
        }

        [Fact(DisplayName = "BuildPayload should throw AppException when no parameters configured")]
        [Trait("BuildPayload", "Fail")]
        public async Task BuildPayload_ShouldThrowAppException_WhenNoParametersConfigured()
        {
            // Arrange
            var automationServicesDto = AutomationFixture.FindValidAutomationServicesDto();
            var stepToolDto = CreateValidStepToolDto();
            stepToolDto.Parameters = new List<StepToolParameterDto>();
            var outputs = new List<StepToolOutput>();
            var execution = AutomationFixture.FindValidStepToolExecution();

            _mockStepToolRepository
                .Setup(repo => repo.FindById(It.IsAny<int>()))
                .ReturnsAsync(stepToolDto);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() =>
                _handler.BuildPayload(automationServicesDto, null, outputs, execution));

            Assert.Equal(ErrorCode.NotFound, exception.ErrorCode);
            Assert.Contains("No API was found configured for the specified step tool", exception.Message);
        }

        [Fact(DisplayName = "BuildPayload should throw AppException when API template not found")]
        [Trait("BuildPayload", "Fail")]
        public async Task BuildPayload_ShouldThrowAppException_WhenApiTemplateNotFound()
        {
            // Arrange
            var automationServicesDto = AutomationFixture.FindValidAutomationServicesDto();
            var stepToolDto = CreateValidStepToolDto();
            var outputs = new List<StepToolOutput>();
            var execution = AutomationFixture.FindValidStepToolExecution();

            var apiRequest = new ApiRequestDto
            {
                TemplateId = 1,
                Url = "https://api.example.com",
                Method = "POST"
            };

            var encryptedData = JsonSerializer.Serialize(apiRequest);

            _mockStepToolRepository
                .Setup(repo => repo.FindById(It.IsAny<int>()))
                .ReturnsAsync(stepToolDto);

            _mockEncryptionService
                .Setup(service => service.Decrypt(It.IsAny<string>()))
                .Returns(encryptedData);

            _mockApiTemplateRepository
                .Setup(repo => repo.FindById(It.IsAny<int>()))
                .ReturnsAsync((ApiTemplateDto?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() =>
                _handler.BuildPayload(automationServicesDto, null, outputs, execution));

            Assert.Equal(ErrorCode.NotFound, exception.ErrorCode);
            Assert.Contains("API template not found", exception.Message);
        }

        [Fact(DisplayName = "BuildPayload should throw AppException when ExecutionId is null")]
        [Trait("BuildPayload", "Fail")]
        public async Task BuildPayload_ShouldThrowAppException_WhenExecutionIdIsNull()
        {
            // Arrange
            var automationServicesDto = AutomationFixture.FindValidAutomationServicesDto();
            var stepToolDto = CreateValidStepToolDto();
            var apiTemplate = CreateValidApiTemplateDto();
            var outputs = new List<StepToolOutput>();

            var apiRequest = new ApiRequestDto
            {
                TemplateId = apiTemplate.Id!.Value,
                Url = apiTemplate.Url,
                Method = apiTemplate.Method
            };

            var encryptedData = JsonSerializer.Serialize(apiRequest);

            _mockStepToolRepository
                .Setup(repo => repo.FindById(It.IsAny<int>()))
                .ReturnsAsync(stepToolDto);

            _mockEncryptionService
                .Setup(service => service.Decrypt(It.IsAny<string>()))
                .Returns(encryptedData);

            _mockApiTemplateRepository
                .Setup(repo => repo.FindById(It.IsAny<int>()))
                .ReturnsAsync(apiTemplate);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() =>
                _handler.BuildPayload(automationServicesDto, null, outputs, null));

            Assert.Equal(ErrorCode.InvalidValue, exception.ErrorCode);
            Assert.Contains("ExecutionId not defined", exception.Message);
        }

        [Fact(DisplayName = "BuildPayload should replace OCR placeholder in body")]
        [Trait("BuildPayload", "Success")]
        public async Task BuildPayload_ShouldReplaceOcrPlaceholder_InBody()
        {
            // Arrange
            var automationServicesDto = AutomationFixture.FindValidAutomationServicesDto();
            var stepToolDto = CreateValidStepToolDto();
            var apiTemplate = CreateValidApiTemplateDto();
            var execution = AutomationFixture.FindValidStepToolExecution();

            var ocrOutput = new
            {
                DocumentEmbeddings = new[]
                {
                    new { Text = "OCR Text 1" },
                    new { Text = "OCR Text 2" }
                }
            };

            var outputValue = JsonSerializer.Serialize(ocrOutput);
            var output = CreateStepToolOutput(HandlersTypes.Ocr, outputValue);
            var outputs = new List<StepToolOutput> { output };

            var apiRequest = new ApiRequestDto
            {
                TemplateId = apiTemplate.Id!.Value,
                Url = apiTemplate.Url,
                Method = apiTemplate.Method,
                Body = "{\"text\": {{ocr}}}"
            };

            var encryptedData = JsonSerializer.Serialize(apiRequest);

            _mockStepToolRepository
                .Setup(repo => repo.FindById(It.IsAny<int>()))
                .ReturnsAsync(stepToolDto);

            _mockEncryptionService
                .Setup(service => service.Decrypt(It.IsAny<string>()))
                .Returns(encryptedData);

            _mockApiTemplateRepository
                .Setup(repo => repo.FindById(It.IsAny<int>()))
                .ReturnsAsync(apiTemplate);

            // Act
            var result = await _handler.BuildPayload(automationServicesDto, null, outputs, execution);

            // Assert
            var message = result.Message as ApiRequestDto;
            Assert.NotNull(message);
            Assert.NotNull(message.Body);
            Assert.Contains("OCR Text 1", message.Body);
            Assert.Contains("OCR Text 2", message.Body);
        }

        [Fact(DisplayName = "BuildPayload should replace Embeddings placeholder in body")]
        [Trait("BuildPayload", "Success")]
        public async Task BuildPayload_ShouldReplaceEmbeddingsPlaceholder_InBody()
        {
            // Arrange
            var automationServicesDto = AutomationFixture.FindValidAutomationServicesDto();
            var stepToolDto = CreateValidStepToolDto();
            var apiTemplate = CreateValidApiTemplateDto();
            var execution = AutomationFixture.FindValidStepToolExecution();

            var embeddingsOutput = new
            {
                DocumentEmbeddings = new[]
                {
                    new { Text = "Embeddings Text 1" },
                    new { Text = "Embeddings Text 2" }
                }
            };

            var outputValue = JsonSerializer.Serialize(embeddingsOutput);
            var output = CreateStepToolOutput(HandlersTypes.Embeddings, outputValue);
            var outputs = new List<StepToolOutput> { output };

            var apiRequest = new ApiRequestDto
            {
                TemplateId = apiTemplate.Id!.Value,
                Url = apiTemplate.Url,
                Method = apiTemplate.Method,
                Body = "{\"text\": {{embeddings}}}"
            };

            var encryptedData = JsonSerializer.Serialize(apiRequest);

            _mockStepToolRepository
                .Setup(repo => repo.FindById(It.IsAny<int>()))
                .ReturnsAsync(stepToolDto);

            _mockEncryptionService
                .Setup(service => service.Decrypt(It.IsAny<string>()))
                .Returns(encryptedData);

            _mockApiTemplateRepository
                .Setup(repo => repo.FindById(It.IsAny<int>()))
                .ReturnsAsync(apiTemplate);

            // Act
            var result = await _handler.BuildPayload(automationServicesDto, null, outputs, execution);

            // Assert
            var message = result.Message as ApiRequestDto;
            Assert.NotNull(message);
            Assert.NotNull(message.Body);
            Assert.Contains("Embeddings Text 1", message.Body);
            Assert.Contains("Embeddings Text 2", message.Body);
        }

        [Fact(DisplayName = "BuildPayload should replace Prompt placeholder in body")]
        [Trait("BuildPayload", "Success")]
        public async Task BuildPayload_ShouldReplacePromptPlaceholder_InBody()
        {
            // Arrange
            var automationServicesDto = AutomationFixture.FindValidAutomationServicesDto();
            var stepToolDto = CreateValidStepToolDto();
            var apiTemplate = CreateValidApiTemplateDto();
            var execution = AutomationFixture.FindValidStepToolExecution();

            var promptText = "This is the prompt response";
            var output = CreateStepToolOutput(HandlersTypes.Prompt, promptText);
            var outputs = new List<StepToolOutput> { output };

            var apiRequest = new ApiRequestDto
            {
                TemplateId = apiTemplate.Id!.Value,
                Url = apiTemplate.Url,
                Method = apiTemplate.Method,
                Body = "{\"text\": {{prompt}}}"
            };

            var encryptedData = JsonSerializer.Serialize(apiRequest);

            _mockStepToolRepository
                .Setup(repo => repo.FindById(It.IsAny<int>()))
                .ReturnsAsync(stepToolDto);

            _mockEncryptionService
                .Setup(service => service.Decrypt(It.IsAny<string>()))
                .Returns(encryptedData);

            _mockApiTemplateRepository
                .Setup(repo => repo.FindById(It.IsAny<int>()))
                .ReturnsAsync(apiTemplate);

            // Act
            var result = await _handler.BuildPayload(automationServicesDto, null, outputs, execution);

            // Assert
            var message = result.Message as ApiRequestDto;
            Assert.NotNull(message);
            Assert.NotNull(message.Body);
            Assert.Contains(promptText, message.Body);
        }

        [Fact(DisplayName = "BuildPayload should handle multiple placeholders in body")]
        [Trait("BuildPayload", "Success")]
        public async Task BuildPayload_ShouldHandleMultiplePlaceholders_InBody()
        {
            // Arrange
            var automationServicesDto = AutomationFixture.FindValidAutomationServicesDto();
            var stepToolDto = CreateValidStepToolDto();
            var apiTemplate = CreateValidApiTemplateDto();
            var execution = AutomationFixture.FindValidStepToolExecution();

            var ocrOutput = new
            {
                DocumentEmbeddings = new[]
                {
                    new { Text = "OCR Text" }
                }
            };

            var promptText = "Prompt response";

            var ocrOutputValue = JsonSerializer.Serialize(ocrOutput);
            var output1 = CreateStepToolOutput(HandlersTypes.Ocr, ocrOutputValue);
            var output2 = CreateStepToolOutput(HandlersTypes.Prompt, promptText);
            var outputs = new List<StepToolOutput> { output1, output2 };

            var apiRequest = new ApiRequestDto
            {
                TemplateId = apiTemplate.Id!.Value,
                Url = apiTemplate.Url,
                Method = apiTemplate.Method,
                Body = "{\"ocr\": {{ocr}}, \"prompt\": {{prompt}}}"
            };

            var encryptedData = JsonSerializer.Serialize(apiRequest);

            _mockStepToolRepository
                .Setup(repo => repo.FindById(It.IsAny<int>()))
                .ReturnsAsync(stepToolDto);

            _mockEncryptionService
                .Setup(service => service.Decrypt(It.IsAny<string>()))
                .Returns(encryptedData);

            _mockApiTemplateRepository
                .Setup(repo => repo.FindById(It.IsAny<int>()))
                .ReturnsAsync(apiTemplate);

            // Act
            var result = await _handler.BuildPayload(automationServicesDto, null, outputs, execution);

            // Assert
            var message = result.Message as ApiRequestDto;
            Assert.NotNull(message);
            Assert.NotNull(message.Body);
            Assert.Contains("OCR Text", message.Body);
            Assert.Contains(promptText, message.Body);
        }

        [Fact(DisplayName = "Type property should return API handler type")]
        [Trait("Type", "Success")]
        public void Type_ShouldReturnApiHandlerType()
        {
            // Act
            var type = _handler.Type;

            // Assert
            Assert.Equal(HandlersTypes.API, type);
        }

        private static StepToolDto CreateValidStepToolDto()
        {
            var stepToolDto = AutomationFixture.FindValidStepToolDto();
            stepToolDto.Tool = new ToolDto
            {
                Id = 1,
                Name = "API Tool",
                ToolType = HandlersTypes.API,
                InputData = "input",
                OutputData = "output"
            };
            stepToolDto.Parameters = new List<StepToolParameterDto>
            {
                new StepToolParameterDto
                {
                    Id = 1,
                    Type = "API",
                    Value = "encrypted-data",
                    RequiredFile = false
                }
            };
            return stepToolDto;
        }

        private static ApiTemplateDto CreateValidApiTemplateDto()
        {
            return new ApiTemplateDto
            {
                Id = 1,
                Name = "Test Template",
                Method = "POST",
                Url = "https://api.example.com/test",
                QueryTemplate = null,
                HeaderTemplate = null,
                BodyTemplate = null,
                Created = DateTime.UtcNow
            };
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
    }
}
