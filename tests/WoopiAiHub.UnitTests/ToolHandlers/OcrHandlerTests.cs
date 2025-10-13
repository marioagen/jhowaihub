using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;
using WoopiAiHub.Application.ToolsHandler;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Infrastructure.Messaging.Configuration;
using WoopiAiHub.UnitTests.Fixture;
using Xunit;

namespace WoopiAiHub.UnitTests.ToolHandlers
{
    [Collection(nameof(ToolHandlerCollection))]
    public class OcrHandlerTests
    {
        private readonly AutoMocker _mocker;
        private readonly OcrHandler _handler;
        private readonly MessageQueues _messageQueues;
        private readonly Mock<ITenantCacheServices> _mockTenantCacheServices;

        public OcrHandlerTests()
        {
            _mocker = new AutoMocker();
            _messageQueues = new MessageQueues { EmbeddingQueue = "test-queue" };
            var options = Options.Create(_messageQueues);
            _mocker.Use<IOptions<MessageQueues>>(options);
            _mockTenantCacheServices = _mocker.GetMock<ITenantCacheServices>();

            _handler = _mocker.CreateInstance<OcrHandler>();
        }

        [Fact(DisplayName = "BuildPayload should throw ArgumentException when Ocr model is null or empty")]
        [Trait("BuildPayload", "Fail")]
        public async Task BuildPayload_ShouldThrowArgumentException_WhenEmbeddingModelNameIsNullOrEmpty()
        {
            // Arrange
            var automationServiceDto = AutomationFixture.FindValidAutomationServicesDto();
            var tenantInfo = new TenantInfoDto { OcrModel = string.Empty };

            _mockTenantCacheServices
                .Setup(service => service.FindTenantAsync(It.IsAny<string>(), It.IsAny<ColTypeModule>()))
                .ReturnsAsync(tenantInfo);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _handler.BuildPayload(automationServiceDto, It.IsAny<StepToolParameter>(), It.IsAny<string>()));
        }

        [Fact(DisplayName = "BuildPayload should return ExecutionMessageDto with correct queue and message")]
        [Trait("BuildPayload", "Success")]
        public async Task BuildPayload_ShouldReturnExecutionMessageDto_WithCorrectQueueAndMessage()
        {
            // Arrange
            var automationServicesDto = AutomationFixture.FindValidAutomationServicesDto();

            var tenantInfo = new TenantInfoDto { OcrModel = "test-ocr-model" };

            _mockTenantCacheServices
                .Setup(service => service.FindTenantAsync(It.IsAny<string>(), It.IsAny<ColTypeModule>()))
                .ReturnsAsync(tenantInfo);

            // Act
            var result = await _handler.BuildPayload(automationServicesDto, null, "");

            // Assert
            Assert.Equal(_messageQueues.OcrQueue, result.Queue);
            var message = result.Message as ProcessOcrDto;
            Assert.NotNull(message);
            Assert.Equal(automationServicesDto.Tenant, message.Tenant);
            Assert.Equal(automationServicesDto.Email, message.Email);
            Assert.Equal(automationServicesDto.ReferenceFile, message.ReferenceFile);
            Assert.Equal(tenantInfo.OcrModel, message.Model);
            Assert.Equal(_messageQueues.OcrQueueAiHubResponse, message.ResponseQueue);
            Assert.Equal(automationServicesDto.CardId, message.Data.CardId);
            Assert.Equal(automationServicesDto.StepToolId, message.Data.StepToolId);
        }
    }
}
