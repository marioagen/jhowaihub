using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs.IntegrationHub;
using WoopiAiHub.Domain.DTOs.Refit;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Refit;
using WoopiAiHub.UnitTests.Fixture;
using Xunit;

namespace WoopiAiHub.UnitTests.Services
{
    [Collection(nameof(RagInvocationRouterCollection))]
    public class RagInvocationRouterTests
    {
        private readonly AutoMocker _mocker;
        private readonly RagInvocationRouter _ragInvocationRouter;
        private readonly Mock<IEmbeddingsApi> _embeddingsApiMock;

        public RagInvocationRouterTests()
        {
            _mocker = new AutoMocker();

            var configMock = new Mock<IConfiguration>();
            configMock.Setup(x => x["ChatCompletionSettings:ApiVersion"]).Returns("2024-02-15-preview");
            configMock.Setup(x => x["KeyAccess"]).Returns("mock-key-access");

            _mocker.Use(configMock.Object);
            _mocker.Use<ILogger<RagInvocationRouter>>(new Mock<ILogger<RagInvocationRouter>>().Object);

            _embeddingsApiMock = _mocker.GetMock<IEmbeddingsApi>();

            _ragInvocationRouter = _mocker.CreateInstance<RagInvocationRouter>();
        }

        [Fact(DisplayName = "ExecuteCustomQueryAsync - Should execute via Indexer when tenant routes to indexer")]
        [Trait("ExecuteCustomQueryAsync", "Indexer")]
        public async Task ExecuteCustomQueryAsync_Indexer_Success()
        {
            // Arrange
            var tenant = RagInvocationRouterFixture.FindValidTenantWithIndexer();
            var referenceFile = "test_file.pdf";
            var indexerApiKey = "api-key-123";
            var emailCreator = "user@example.com";
            var request = RagInvocationRouterFixture.FindValidCustomQueryRequest();
            var responseModel = RagInvocationRouterFixture.FindValidQueryResponseModel();

            var embeddingsApi = _mocker.GetMock<IEmbeddingsApi>();
            var responseContent = JsonConvert.SerializeObject(responseModel);
            var httpResponse = RagInvocationRouterFixture.FindValidHttpResponseMessage(responseContent);

            embeddingsApi
                .Setup(a => a.CustomQuery(tenant.Name, referenceFile, request, indexerApiKey))
                .ReturnsAsync(httpResponse);

            // Act
            var result = await _ragInvocationRouter.ExecuteCustomQueryAsync(tenant, referenceFile, indexerApiKey, emailCreator, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(responseModel.response, result.ResponseText);
            Assert.NotEmpty(result.Usage);
            embeddingsApi.Verify(a => a.CustomQuery(tenant.Name, referenceFile, request, indexerApiKey), Times.Once);
        }

        [Fact(DisplayName = "ExecuteCustomQueryAsync - Should throw FileNotFoundException when file not found in Indexer")]
        [Trait("ExecuteCustomQueryAsync", "Indexer")]
        public async Task ExecuteCustomQueryAsync_Indexer_FileNotFound()
        {
            // Arrange
            var tenant = RagInvocationRouterFixture.FindValidTenantWithIndexer();
            var referenceFile = "nonexistent_file.pdf";
            var indexerApiKey = "api-key-123";
            var emailCreator = "user@example.com";
            var request = RagInvocationRouterFixture.FindValidCustomQueryRequest();

            var embeddingsApi = _mocker.GetMock<IEmbeddingsApi>();
            var httpResponse = RagInvocationRouterFixture.FindNotFoundHttpResponseMessage();

            embeddingsApi
                .Setup(a => a.CustomQuery(tenant.Name, referenceFile, request, indexerApiKey))
                .ReturnsAsync(httpResponse);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<FileNotFoundException>(
                () => _ragInvocationRouter.ExecuteCustomQueryAsync(tenant, referenceFile, indexerApiKey, emailCreator, request));

            Assert.Contains("file was not found", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact(DisplayName = "ExecuteCustomQueryAsync - Should throw AppException when Embeddings API returns error")]
        [Trait("ExecuteCustomQueryAsync", "Indexer")]
        public async Task ExecuteCustomQueryAsync_Indexer_ApiError()
        {
            // Arrange
            var tenant = RagInvocationRouterFixture.FindValidTenantWithIndexer();
            var referenceFile = "test_file.pdf";
            var indexerApiKey = "api-key-123";
            var emailCreator = "user@example.com";
            var request = RagInvocationRouterFixture.FindValidCustomQueryRequest();

            var embeddingsApi = _mocker.GetMock<IEmbeddingsApi>();
            var httpResponse = RagInvocationRouterFixture.FindErrorHttpResponseMessage();

            embeddingsApi
                .Setup(a => a.CustomQuery(tenant.Name, referenceFile, request, indexerApiKey))
                .ReturnsAsync(httpResponse);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(
                () => _ragInvocationRouter.ExecuteCustomQueryAsync(tenant, referenceFile, indexerApiKey, emailCreator, request));

            Assert.Equal(ErrorCode.RefitApiError, exception.ErrorCode);
        }

        [Fact(DisplayName = "ExecuteCustomQueryAsync - Should throw InvalidOperationException when response has null content")]
        [Trait("ExecuteCustomQueryAsync", "Indexer")]
        public async Task ExecuteCustomQueryAsync_Indexer_NullResponse()
        {
            // Arrange
            var tenant = RagInvocationRouterFixture.FindValidTenantWithIndexer();
            var referenceFile = "test_file.pdf";
            var indexerApiKey = "api-key-123";
            var emailCreator = "user@example.com";
            var request = RagInvocationRouterFixture.FindValidCustomQueryRequest();
            var responseModel = new QueryResponseModelRefitDto { response = null! };

            var embeddingsApi = _mocker.GetMock<IEmbeddingsApi>();
            var responseContent = JsonConvert.SerializeObject(responseModel);
            var httpResponse = RagInvocationRouterFixture.FindValidHttpResponseMessage(responseContent);

            embeddingsApi
                .Setup(a => a.CustomQuery(tenant.Name, referenceFile, request, indexerApiKey))
                .ReturnsAsync(httpResponse);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _ragInvocationRouter.ExecuteCustomQueryAsync(tenant, referenceFile, indexerApiKey, emailCreator, request));

            Assert.Contains("response value is null", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact(DisplayName = "ExecuteCustomQueryAsync - Should execute via Integration Hub when tenant routes to Azure AI Search")]
        [Trait("ExecuteCustomQueryAsync", "Integration")]
        public async Task ExecuteCustomQueryAsync_Integration_Success()
        {
            // Arrange
            var tenant = RagInvocationRouterFixture.FindValidTenantWithAzureAiSearch();
            var referenceFile = "test_file.pdf";
            var indexerApiKey = "api-key-123";
            var emailCreator = "user@example.com";
            var request = RagInvocationRouterFixture.FindValidCustomQueryRequest();
            var integrationResponse = RagInvocationRouterFixture.FindValidIntegrationQueryResponse();

            var integrationHubApi = _mocker.GetMock<IAzureAiSearch>();
            var responseContent = JsonConvert.SerializeObject(integrationResponse, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore
            });
            var httpResponse = RagInvocationRouterFixture.FindValidHttpResponseMessage(responseContent);

            integrationHubApi
                .Setup(a => a.CustomQueryAsync(It.IsAny<string>(), It.IsAny<IntegrationHubDocumentEmbeddingsQueryRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(httpResponse);

            // Act
            var result = await _ragInvocationRouter.ExecuteCustomQueryAsync(tenant, referenceFile, indexerApiKey, emailCreator, request);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.ResponseText);
            Assert.NotEmpty(result.Usage);
            integrationHubApi.Verify(a => a.CustomQueryAsync(It.IsAny<string>(), It.IsAny<IntegrationHubDocumentEmbeddingsQueryRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "ExecuteCustomQueryAsync - Should throw FileNotFoundException when Integration API returns NotFound")]
        [Trait("ExecuteCustomQueryAsync", "Integration")]
        public async Task ExecuteCustomQueryAsync_Integration_FileNotFound()
        {
            // Arrange
            var tenant = RagInvocationRouterFixture.FindValidTenantWithAzureAiSearch();
            var referenceFile = "nonexistent_file.pdf";
            var indexerApiKey = "api-key-123";
            var emailCreator = "user@example.com";
            var request = RagInvocationRouterFixture.FindValidCustomQueryRequest();

            var integrationHubApi = _mocker.GetMock<IAzureAiSearch>();
            var httpResponse = RagInvocationRouterFixture.FindNotFoundHttpResponseMessage();

            integrationHubApi
                .Setup(a => a.CustomQueryAsync(It.IsAny<string>(), It.IsAny<IntegrationHubDocumentEmbeddingsQueryRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(httpResponse);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<FileNotFoundException>(
                () => _ragInvocationRouter.ExecuteCustomQueryAsync(tenant, referenceFile, indexerApiKey, emailCreator, request));

            Assert.Contains("file was not found", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact(DisplayName = "ExecuteCustomQueryAsync - Should throw AppException when Integration API returns error")]
        [Trait("ExecuteCustomQueryAsync", "Integration")]
        public async Task ExecuteCustomQueryAsync_Integration_ApiError()
        {
            // Arrange
            var tenant = RagInvocationRouterFixture.FindValidTenantWithAzureAiSearch();
            var referenceFile = "test_file.pdf";
            var indexerApiKey = "api-key-123";
            var emailCreator = "user@example.com";
            var request = RagInvocationRouterFixture.FindValidCustomQueryRequest();

            var integrationHubApi = _mocker.GetMock<IAzureAiSearch>();
            var httpResponse = RagInvocationRouterFixture.FindErrorHttpResponseMessage();

            integrationHubApi
                .Setup(a => a.CustomQueryAsync(It.IsAny<string>(), It.IsAny<IntegrationHubDocumentEmbeddingsQueryRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(httpResponse);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(
                () => _ragInvocationRouter.ExecuteCustomQueryAsync(tenant, referenceFile, indexerApiKey, emailCreator, request));

            Assert.Equal(ErrorCode.RefitApiError, exception.ErrorCode);
        }

        [Fact(DisplayName = "ExecuteCustomQueryAsync - Should throw InvalidOperationException when Integration API returns no answer")]
        [Trait("ExecuteCustomQueryAsync", "Integration")]
        public async Task ExecuteCustomQueryAsync_Integration_NoAnswer()
        {
            // Arrange
            var tenant = RagInvocationRouterFixture.FindValidTenantWithAzureAiSearch();
            var referenceFile = "test_file.pdf";
            var indexerApiKey = "api-key-123";
            var emailCreator = "user@example.com";
            var request = RagInvocationRouterFixture.FindValidCustomQueryRequest();
            var integrationResponse = new IntegrationHubDocumentEmbeddingsQueryResponse { QuestionsAnswers = [] };

            var integrationHubApi = _mocker.GetMock<IAzureAiSearch>();
            var responseContent = JsonConvert.SerializeObject(integrationResponse);
            var httpResponse = RagInvocationRouterFixture.FindValidHttpResponseMessage(responseContent);

            integrationHubApi
                .Setup(a => a.CustomQueryAsync(It.IsAny<string>(), It.IsAny<IntegrationHubDocumentEmbeddingsQueryRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(httpResponse);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _ragInvocationRouter.ExecuteCustomQueryAsync(tenant, referenceFile, indexerApiKey, emailCreator, request));

            Assert.Contains("no answer", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact(DisplayName = "ExecuteCustomQueryAsync - Should throw ArgumentNullException when tenant is null")]
        [Trait("ExecuteCustomQueryAsync", "Validation")]
        public async Task ExecuteCustomQueryAsync_NullTenant()
        {
            // Arrange
            var referenceFile = "test_file.pdf";
            var indexerApiKey = "api-key-123";
            var emailCreator = "user@example.com";
            var request = RagInvocationRouterFixture.FindValidCustomQueryRequest();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _ragInvocationRouter.ExecuteCustomQueryAsync(null!, referenceFile, indexerApiKey, emailCreator, request));
        }

        [Fact(DisplayName = "DeleteEmbeddingsAsync - Should delete via Indexer when tenant routes to indexer")]
        [Trait("DeleteEmbeddingsAsync", "Indexer")]
        public async Task DeleteEmbeddingsAsync_Indexer_Success()
        {
            // Arrange
            var tenant = RagInvocationRouterFixture.FindValidTenantWithIndexer();
            var referenceFile = "test_file.pdf";
            var indexerApiKey = "api-key-123";

            var embeddingsApi = _mocker.GetMock<IEmbeddingsApi>();
            var httpResponse = RagInvocationRouterFixture.FindValidHttpResponseMessage();

            embeddingsApi
                .Setup(a => a.DeleteHash(tenant.Name, referenceFile, tenant.Name, indexerApiKey))
                .ReturnsAsync(httpResponse);

            // Act
            await _ragInvocationRouter.DeleteEmbeddingsAsync(tenant, referenceFile, indexerApiKey);

            // Assert
            embeddingsApi.Verify(a => a.DeleteHash(tenant.Name, referenceFile, tenant.Name, indexerApiKey), Times.Once);
        }

        [Fact(DisplayName = "DeleteEmbeddingsAsync - Should not throw when Indexer returns NotFound")]
        [Trait("DeleteEmbeddingsAsync", "Indexer")]
        public async Task DeleteEmbeddingsAsync_Indexer_NotFound()
        {
            // Arrange
            var tenant = RagInvocationRouterFixture.FindValidTenantWithIndexer();
            var referenceFile = "nonexistent_file.pdf";
            var indexerApiKey = "api-key-123";

            var embeddingsApi = _mocker.GetMock<IEmbeddingsApi>();
            var httpResponse = RagInvocationRouterFixture.FindNotFoundHttpResponseMessage();

            embeddingsApi
                .Setup(a => a.DeleteHash(tenant.Name, referenceFile, tenant.Name, indexerApiKey))
                .ReturnsAsync(httpResponse);

            // Act & Assert - Should not throw
            await _ragInvocationRouter.DeleteEmbeddingsAsync(tenant, referenceFile, indexerApiKey);

            embeddingsApi.Verify(a => a.DeleteHash(tenant.Name, referenceFile, tenant.Name, indexerApiKey), Times.Once);
        }

        [Fact(DisplayName = "DeleteEmbeddingsAsync - Should throw ArgumentException when Indexer returns error")]
        [Trait("DeleteEmbeddingsAsync", "Indexer")]
        public async Task DeleteEmbeddingsAsync_Indexer_ApiError()
        {
            // Arrange
            var tenant = RagInvocationRouterFixture.FindValidTenantWithIndexer();
            var referenceFile = "test_file.pdf";
            var indexerApiKey = "api-key-123";

            var embeddingsApi = _mocker.GetMock<IEmbeddingsApi>();
            var httpResponse = RagInvocationRouterFixture.FindErrorHttpResponseMessage();

            embeddingsApi
                .Setup(a => a.DeleteHash(tenant.Name, referenceFile, tenant.Name, indexerApiKey))
                .ReturnsAsync(httpResponse);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _ragInvocationRouter.DeleteEmbeddingsAsync(tenant, referenceFile, indexerApiKey));
        }

        [Fact(DisplayName = "DeleteEmbeddingsAsync - Should delete via Integration Hub when tenant routes to Azure AI Search")]
        [Trait("DeleteEmbeddingsAsync", "Integration")]
        public async Task DeleteEmbeddingsAsync_Integration_Success()
        {
            // Arrange
            var tenant = RagInvocationRouterFixture.FindValidTenantWithAzureAiSearch();
            var referenceFile = "test_file.pdf";
            var indexerApiKey = "api-key-123";

            var integrationHubApi = _mocker.GetMock<IAzureAiSearch>();
            var httpResponse = RagInvocationRouterFixture.FindValidHttpResponseMessage();
            integrationHubApi
                .Setup(a => a.DeleteEmbeddingsAsync(It.IsAny<string>(), It.IsAny<IntegrationHubDocumentEmbeddingsDeleteRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(httpResponse);

            // Act
            await _ragInvocationRouter.DeleteEmbeddingsAsync(tenant, referenceFile, indexerApiKey);

            // Assert
            integrationHubApi.Verify(a => a.DeleteEmbeddingsAsync(It.IsAny<string>(), It.IsAny<IntegrationHubDocumentEmbeddingsDeleteRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "DeleteEmbeddingsAsync - Should not throw when Integration Hub returns NotFound")]
        [Trait("DeleteEmbeddingsAsync", "Integration")]
        public async Task DeleteEmbeddingsAsync_Integration_NotFound()
        {
            // Arrange
            var tenant = RagInvocationRouterFixture.FindValidTenantWithAzureAiSearch();
            var referenceFile = "nonexistent_file.pdf";
            var indexerApiKey = "api-key-123";

            var integrationHubApi = _mocker.GetMock<IAzureAiSearch>();
            var httpResponse = RagInvocationRouterFixture.FindNotFoundHttpResponseMessage();

            integrationHubApi
                .Setup(a => a.DeleteEmbeddingsAsync(It.IsAny<string>(), It.IsAny<IntegrationHubDocumentEmbeddingsDeleteRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(httpResponse);

            // Act & Assert - Should not throw
            await _ragInvocationRouter.DeleteEmbeddingsAsync(tenant, referenceFile, indexerApiKey);

            integrationHubApi.Verify(a => a.DeleteEmbeddingsAsync(It.IsAny<string>(), It.IsAny<IntegrationHubDocumentEmbeddingsDeleteRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "DeleteEmbeddingsAsync - Should throw ArgumentException when Integration Hub returns error")]
        [Trait("DeleteEmbeddingsAsync", "Integration")]
        public async Task DeleteEmbeddingsAsync_Integration_ApiError()
        {
            // Arrange
            var tenant = RagInvocationRouterFixture.FindValidTenantWithAzureAiSearch();
            var referenceFile = "test_file.pdf";
            var indexerApiKey = "api-key-123";

            var integrationHubApi = _mocker.GetMock<IAzureAiSearch>();
            var httpResponse = RagInvocationRouterFixture.FindErrorHttpResponseMessage();

            integrationHubApi
                .Setup(a => a.DeleteEmbeddingsAsync(It.IsAny<string>(), It.IsAny<IntegrationHubDocumentEmbeddingsDeleteRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(httpResponse);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _ragInvocationRouter.DeleteEmbeddingsAsync(tenant, referenceFile, indexerApiKey));
        }

        [Fact(DisplayName = "DeleteEmbeddingsAsync - Should throw ArgumentNullException when tenant is null")]
        [Trait("DeleteEmbeddingsAsync", "Validation")]
        public async Task DeleteEmbeddingsAsync_NullTenant()
        {
            // Arrange
            var referenceFile = "test_file.pdf";
            var indexerApiKey = "api-key-123";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _ragInvocationRouter.DeleteEmbeddingsAsync(null!, referenceFile, indexerApiKey));
        }

        [Fact(DisplayName = "ExecuteChatCompletionAsync - Should execute chat completion successfully")]
        [Trait("ExecuteChatCompletionAsync", "Success")]
        public async Task ExecuteChatCompletionAsync_Success()
        {
            // Arrange
            var tenant = RagInvocationRouterFixture.FindValidTenantWithIndexer();
            var email = "user@example.com";
            var chatCompletion = RagInvocationRouterFixture.FindValidChatCompletionRequest();
            var model = "gpt-4";
            var apiVersion = "2024-02-15-preview";
            var response = RagInvocationRouterFixture.FindValidChatCompletionResponse();

            var chatCompletionApi = _mocker.GetMock<IChatCompletionApi>();
            chatCompletionApi
                .Setup(a => a.GetChatCompletion(
                    tenant.AiGatewayApplicationId!.Value.ToString(),
                    model,
                    apiVersion,
                    tenant.AiGatewayKey,
                    chatCompletion))
                .ReturnsAsync(response);

            // Act
            var result = await _ragInvocationRouter.ExecuteChatCompletionAsync(tenant, email, chatCompletion, model, apiVersion);

            // Assert
            Assert.NotNull(result);
            chatCompletionApi.Verify(a => a.GetChatCompletion(
                tenant.AiGatewayApplicationId!.Value.ToString(),
                model,
                apiVersion,
                tenant.AiGatewayKey,
                chatCompletion), Times.Once);
        }

        [Fact(DisplayName = "ExecuteChatCompletionAsync - Should throw ArgumentException when AiGateway ApplicationId is null")]
        [Trait("ExecuteChatCompletionAsync", "Validation")]
        public async Task ExecuteChatCompletionAsync_MissingApplicationId()
        {
            // Arrange
            var tenant = RagInvocationRouterFixture.FindTenantMissingAiGatewayInfo();
            var email = "user@example.com";
            var chatCompletion = RagInvocationRouterFixture.FindValidChatCompletionRequest();
            var model = "gpt-4";
            var apiVersion = "2024-02-15-preview";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _ragInvocationRouter.ExecuteChatCompletionAsync(tenant, email, chatCompletion, model, apiVersion));

            Assert.Contains("ApplicationId", exception.Message);
        }

        [Fact(DisplayName = "ExecuteChatCompletionAsync - Should throw ArgumentException when AiGateway key is empty")]
        [Trait("ExecuteChatCompletionAsync", "Validation")]
        public async Task ExecuteChatCompletionAsync_MissingKey()
        {
            // Arrange
            var tenant = RagInvocationRouterFixture.FindTenantMissingAiGatewayInfo();
            var email = "user@example.com";
            var chatCompletion = RagInvocationRouterFixture.FindValidChatCompletionRequest();
            var model = "gpt-4";
            var apiVersion = "2024-02-15-preview";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _ragInvocationRouter.ExecuteChatCompletionAsync(tenant, email, chatCompletion, model, apiVersion));

            Assert.Contains("key", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact(DisplayName = "ExecuteChatCompletionAsync - Should throw InvalidOperationException when tenant routes to Azure AI Search")]
        [Trait("ExecuteChatCompletionAsync", "Integration")]
        public async Task ExecuteChatCompletionAsync_AzureAiSearchNotSupported()
        {
            // Arrange
            var tenant = RagInvocationRouterFixture.FindValidTenantWithAzureAiSearch();
            var email = "user@example.com";
            var chatCompletion = RagInvocationRouterFixture.FindValidChatCompletionRequest();
            var model = "gpt-4";
            var apiVersion = "2024-02-15-preview";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _ragInvocationRouter.ExecuteChatCompletionAsync(tenant, email, chatCompletion, model, apiVersion));

            Assert.Contains("not supported", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact(DisplayName = "ExecuteChatCompletionAsync - Should throw ArgumentNullException when tenant is null")]
        [Trait("ExecuteChatCompletionAsync", "Validation")]
        public async Task ExecuteChatCompletionAsync_NullTenant()
        {
            // Arrange
            var email = "user@example.com";
            var chatCompletion = RagInvocationRouterFixture.FindValidChatCompletionRequest();
            var model = "gpt-4";
            var apiVersion = "2024-02-15-preview";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _ragInvocationRouter.ExecuteChatCompletionAsync(null!, email, chatCompletion, model, apiVersion));
        }
    }
}
