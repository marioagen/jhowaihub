using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;
using Newtonsoft.Json;
using WoopiAiHub.Application.ToolsHandler;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Refit;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Infrastructure.Messaging.Configuration;
using WoopiAiHub.UnitTests.Fixture;
using Xunit;

namespace WoopiAiHub.UnitTests.ToolHandlers
{
    [Collection(nameof(ToolHandlerCollection))]
    public class EmbeddingsHandlerTests
    {
        private readonly AutoMocker _mocker;
        private readonly EmbeddingsHandler _handler;
        private readonly Mock<ITenantCacheServices> _mockTenantCacheServices;
        private readonly Mock<IKeyGeneratorApi> _mockKeyGeneratorApi;
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly MessageQueues _messageQueues;

        public EmbeddingsHandlerTests()
        {
            _mocker = new AutoMocker();
            _messageQueues = new MessageQueues { EmbeddingQueue = "test-queue" };
            var options = Options.Create(_messageQueues);
            _mocker.Use<IOptions<MessageQueues>>(options);
            _mockTenantCacheServices = _mocker.GetMock<ITenantCacheServices>();
            _mockKeyGeneratorApi = _mocker.GetMock<IKeyGeneratorApi>();
            _mockConfig = _mocker.GetMock<IConfiguration>();
            _handler = _mocker.CreateInstance<EmbeddingsHandler>();
        }

        [Fact(DisplayName = "BuildPayload should throw ArgumentException when EmbeddingModelName is null or empty")]
        [Trait("BuildPayload", "Fail")]
        public async Task BuildPayload_ShouldThrowArgumentException_WhenEmbeddingModelNameIsNullOrEmpty()
        {
            // Arrange
            var automationServiceDto = AutomationFixture.FindValidAutomationServicesDto();
            var tenantInfo = new TenantInfoDto { EmbeddingModelName = string.Empty };
            var documentEmbeddingsDataDto = MessagingFixture.FindValidDocumentEmbeddingsDataDto();
            var output = AutomationFixture.FindValidStepToolOutput(JsonConvert.SerializeObject(documentEmbeddingsDataDto));

            _mockTenantCacheServices
                .Setup(service => service.FindTenantAsync(It.IsAny<string>(), It.IsAny<ColTypeModule>()))
                .ReturnsAsync(tenantInfo);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _handler.BuildPayload(automationServiceDto, It.IsAny<StepToolParameter>(), [output]));
        }

        [Fact(DisplayName = "BuildPayload should return ExecutionMessageDto with correct queue and message")]
        [Trait("BuildPayload", "Success")]
        public async Task BuildPayload_ShouldReturnExecutionMessageDto_WithCorrectQueueAndMessage()
        {
            // Arrange
            var automationServicesDto = AutomationFixture.FindValidAutomationServicesDto();
            var tenantInfo = new TenantInfoDto { EmbeddingModelName = "test-model" };
            var keyAccess = "test-key-access";

            var documentEmbeddingsDataDto = MessagingFixture.FindValidDocumentEmbeddingsDataDto();
            var output = AutomationFixture.FindValidStepToolOutput(JsonConvert.SerializeObject(documentEmbeddingsDataDto));

            _mockTenantCacheServices
                .Setup(service => service.FindTenantAsync(It.IsAny<string>(), It.IsAny<ColTypeModule>()))
                .ReturnsAsync(tenantInfo);
            _mockConfig
                .Setup(config => config[It.IsAny<string>()])
                .Returns(keyAccess);

            // Act
            var result = await _handler.BuildPayload(automationServicesDto, null, [output]);

            // Assert
            _mockTenantCacheServices.Verify(repo => repo.FindTenantAsync(It.IsAny<string>(), It.IsAny<ColTypeModule>()), Times.Once);
            Assert.Equal(_messageQueues.EmbeddingQueue, result.Queue);
        }
    }
}
