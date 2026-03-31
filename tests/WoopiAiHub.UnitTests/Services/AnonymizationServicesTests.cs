using Microsoft.Extensions.Configuration;
using Moq;
using Moq.AutoMock;
using Moq.Protected;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Refit;
using WoopiAiHub.Domain.Interfaces.Hubs;
using WoopiAiHub.Domain.Interfaces.Refit;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.UnitTests.Fixtures;
using Xunit;

namespace WoopiAiHub.UnitTests.Services
{
    public class AnonymizationServicesTests
    {
        private readonly AutoMocker _mocker;
        private readonly AnonymizationServices _sut;

        private const string ValidToken = "validToken";
        private const string ValidUserId = "123";
        private const string ValidWebhook = "https://webhook.example.com";
        private const string DocumentNotFoundMessage = "Document file not found.";
        private const string TokenNotConfiguredMessage = "Anonymization API token is not configured.";
        private const string UserIdNotConfiguredMessage = "Anonymization User ID is not configured.";
        private const string WebhookNotProvidedMessage = "Anonymization Webhook not provided";
        private const string DownloadUrlNotProvidedMessage = "Download URL not provided in anonymization response";

        public AnonymizationServicesTests()
        {
            _mocker = new AutoMocker();

            var configBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "RefitExternalSettings:AnonymizationApiToken", ValidToken },
                    { "RefitExternalSettings:AnonymizationUserId", ValidUserId },
                    { "AnonymizationWebhook", ValidWebhook }
                })
                .Build();

            _mocker.Use<IConfiguration>(configBuilder);

            _sut = _mocker.CreateInstance<AnonymizationServices>();
        }

        #region ProcessAnonymization Tests

        [Fact(DisplayName = "ProcessAnonymization - Should successfully process valid anonymization request")]
        [Trait("ProcessAnonymization", "Success")]
        public async Task ProcessAnonymization_WithValidRequest_SuccessfullyProcesses()
        {
            // Arrange
            var documentDto = AnonymizationFixture.FindValidFindDocumentDto();
            var requestDto = AnonymizationFixture.FindValidProcessAnonymizationRequestDto();
            var headersDto = AnonymizationFixture.FindValidHeadersDto();
            var responseDto = AnonymizationFixture.FindValidAnonymizationResponseDto();

            var documentServicesMock = _mocker.GetMock<IDocumentServices>();
            var anonymizationApiMock = _mocker.GetMock<IAnonymizationApi>();
            var httpClientFactoryMock = _mocker.GetMock<IHttpClientFactory>();

            documentServicesMock
                .Setup(x => x.FindDocumentById(requestDto.DocumentId, headersDto.Tenant))
                .ReturnsAsync(documentDto);

            anonymizationApiMock
                .Setup(x => x.InitiateAnonymization(It.IsAny<string>(), It.IsAny<AnonymizationRequestDto>()))
                .ReturnsAsync(responseDto);

            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));

            using (var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("http://localhost") })
            {
                httpClientFactoryMock
                    .Setup(x => x.CreateClient(It.IsAny<string>()))
                    .Returns(httpClient);

                // Act
                await _sut.ProcessAnonymization(requestDto, headersDto);
            }

            // Assert
            documentServicesMock.Verify(x => x.FindDocumentById(requestDto.DocumentId, headersDto.Tenant), Times.Once);
            anonymizationApiMock.Verify(x => x.InitiateAnonymization(It.IsAny<string>(), It.IsAny<AnonymizationRequestDto>()), Times.Once);
        }

        [Fact(DisplayName = "ProcessAnonymization - Should throw InvalidOperationException when document bytes are null")]
        [Trait("ProcessAnonymization", "DocumentNotFound")]
        public async Task ProcessAnonymization_WithNullDocumentBytes_ThrowsInvalidOperationException()
        {
            // Arrange
            var documentDto = AnonymizationFixture.FindValidFindDocumentDto();
            documentDto.BytesDocument = null;

            var requestDto = AnonymizationFixture.FindValidProcessAnonymizationRequestDto();
            var headersDto = AnonymizationFixture.FindValidHeadersDto();

            var documentServicesMock = _mocker.GetMock<IDocumentServices>();
            documentServicesMock
                .Setup(x => x.FindDocumentById(requestDto.DocumentId, headersDto.Tenant))
                .ReturnsAsync(documentDto);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.ProcessAnonymization(requestDto, headersDto));
            Assert.Equal(DocumentNotFoundMessage, exception.Message);
        }

        [Fact(DisplayName = "ProcessAnonymization - Should throw InvalidOperationException when document bytes are empty")]
        [Trait("ProcessAnonymization", "DocumentNotFound")]
        public async Task ProcessAnonymization_WithEmptyDocumentBytes_ThrowsInvalidOperationException()
        {
            // Arrange
            var documentDto = AnonymizationFixture.FindValidFindDocumentDto();
            documentDto.BytesDocument = Array.Empty<byte>();

            var requestDto = AnonymizationFixture.FindValidProcessAnonymizationRequestDto();
            var headersDto = AnonymizationFixture.FindValidHeadersDto();

            var documentServicesMock = _mocker.GetMock<IDocumentServices>();
            documentServicesMock
                .Setup(x => x.FindDocumentById(requestDto.DocumentId, headersDto.Tenant))
                .ReturnsAsync(documentDto);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.ProcessAnonymization(requestDto, headersDto));
            Assert.Equal(DocumentNotFoundMessage, exception.Message);
        }

        [Fact(DisplayName = "ProcessAnonymization - Should throw InvalidOperationException when API token is missing")]
        [Trait("ProcessAnonymization", "Configuration")]
        public async Task ProcessAnonymization_WithMissingApiToken_ThrowsInvalidOperationException()
        {
            // Arrange
            var configBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "RefitExternalSettings:AnonymizationApiToken", string.Empty },
                    { "RefitExternalSettings:AnonymizationUserId", ValidUserId },
                    { "AnonymizationWebhook", ValidWebhook }
                })
                .Build();

            var mocker = new AutoMocker();
            mocker.Use<IConfiguration>(configBuilder);
            var sut = mocker.CreateInstance<AnonymizationServices>();

            var documentDto = AnonymizationFixture.FindValidFindDocumentDto();
            var requestDto = AnonymizationFixture.FindValidProcessAnonymizationRequestDto();
            var headersDto = AnonymizationFixture.FindValidHeadersDto();

            var documentServicesMock = mocker.GetMock<IDocumentServices>();
            documentServicesMock
                .Setup(x => x.FindDocumentById(requestDto.DocumentId, headersDto.Tenant))
                .ReturnsAsync(documentDto);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => sut.ProcessAnonymization(requestDto, headersDto));
            Assert.Equal(TokenNotConfiguredMessage, exception.Message);
        }

        [Fact(DisplayName = "ProcessAnonymization - Should throw InvalidOperationException when User ID is missing")]
        [Trait("ProcessAnonymization", "Configuration")]
        public async Task ProcessAnonymization_WithMissingUserId_ThrowsInvalidOperationException()
        {
            // Arrange
            var configBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "RefitExternalSettings:AnonymizationApiToken", ValidToken },
                    { "AnonymizationWebhook", ValidWebhook }
                })
                .Build();

            var mocker = new AutoMocker();
            mocker.Use<IConfiguration>(configBuilder);
            var sut = mocker.CreateInstance<AnonymizationServices>();

            var documentDto = AnonymizationFixture.FindValidFindDocumentDto();
            var requestDto = AnonymizationFixture.FindValidProcessAnonymizationRequestDto();
            var headersDto = AnonymizationFixture.FindValidHeadersDto();

            var documentServicesMock = mocker.GetMock<IDocumentServices>();
            documentServicesMock
                .Setup(x => x.FindDocumentById(requestDto.DocumentId, headersDto.Tenant))
                .ReturnsAsync(documentDto);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => sut.ProcessAnonymization(requestDto, headersDto));
            Assert.Equal(UserIdNotConfiguredMessage, exception.Message);
        }

        [Fact(DisplayName = "ProcessAnonymization - Should throw InvalidOperationException when webhook is missing")]
        [Trait("ProcessAnonymization", "Configuration")]
        public async Task ProcessAnonymization_WithMissingWebhook_ThrowsInvalidOperationException()
        {
            // Arrange
            var configBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "RefitExternalSettings:AnonymizationApiToken", ValidToken },
                    { "RefitExternalSettings:AnonymizationUserId", ValidUserId }
                })
                .Build();

            var mocker = new AutoMocker();
            mocker.Use<IConfiguration>(configBuilder);
            var sut = mocker.CreateInstance<AnonymizationServices>();

            var documentDto = AnonymizationFixture.FindValidFindDocumentDto();
            var requestDto = AnonymizationFixture.FindValidProcessAnonymizationRequestDto();
            var headersDto = AnonymizationFixture.FindValidHeadersDto();

            var documentServicesMock = mocker.GetMock<IDocumentServices>();
            documentServicesMock
                .Setup(x => x.FindDocumentById(requestDto.DocumentId, headersDto.Tenant))
                .ReturnsAsync(documentDto);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => sut.ProcessAnonymization(requestDto, headersDto));
            Assert.Equal(WebhookNotProvidedMessage, exception.Message);
        }

        [Fact(DisplayName = "ProcessAnonymization - Should throw InvalidOperationException when download URL is missing in response")]
        [Trait("ProcessAnonymization", "Response")]
        public async Task ProcessAnonymization_WithMissingDownloadUrl_ThrowsInvalidOperationException()
        {
            // Arrange
            var documentDto = AnonymizationFixture.FindValidFindDocumentDto();
            var requestDto = AnonymizationFixture.FindValidProcessAnonymizationRequestDto();
            var headersDto = AnonymizationFixture.FindValidHeadersDto();
            var responseDto = AnonymizationFixture.FindAnonymizationResponseDtoWithoutDownloadUrl();

            var documentServicesMock = _mocker.GetMock<IDocumentServices>();
            var anonymizationApiMock = _mocker.GetMock<IAnonymizationApi>();

            documentServicesMock
                .Setup(x => x.FindDocumentById(requestDto.DocumentId, headersDto.Tenant))
                .ReturnsAsync(documentDto);

            anonymizationApiMock
                .Setup(x => x.InitiateAnonymization(It.IsAny<string>(), It.IsAny<AnonymizationRequestDto>()))
                .ReturnsAsync(responseDto);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.ProcessAnonymization(requestDto, headersDto));
            Assert.Equal(DownloadUrlNotProvidedMessage, exception.Message);
        }

        #endregion

        #region ProcessAnonymizationResult Tests

        [Fact(DisplayName = "ProcessAnonymizationResult - Should successfully notify hub with valid result")]
        [Trait("ProcessAnonymizationResult", "Success")]
        public async Task ProcessAnonymizationResult_WithValidResult_NotifiesHubSuccessfully()
        {
            // Arrange
            var result = AnonymizationFixture.FindValidAnonymizationResultDto();
            var hubNotifierMock = _mocker.GetMock<IHubNotifier>();

            hubNotifierMock
                .Setup(x => x.AnonymizationReadyAsync(result.WoopiAiEmail, result.WoopiAiDocumentId, result.DocumentUrl))
                .Returns(Task.CompletedTask);

            // Act
            await _sut.ProcessAnonymizationResult(result);

            // Assert
            hubNotifierMock.Verify(
                x => x.AnonymizationReadyAsync(result.WoopiAiEmail, result.WoopiAiDocumentId, result.DocumentUrl),
                Times.Once);
        }

        [Fact(DisplayName = "ProcessAnonymizationResult - Should throw NullReferenceException when result is null")]
        [Trait("ProcessAnonymizationResult", "NullHandling")]
        public async Task ProcessAnonymizationResult_WithNullResult_ThrowsNullReferenceException()
        {
            // Arrange
            AnonymizationResultDto? result = null;

            // Act & Assert
            await Assert.ThrowsAsync<NullReferenceException>(
                () => _sut.ProcessAnonymizationResult(result!));
        }

        [Fact(DisplayName = "ProcessAnonymizationResult - Should propagate exception from hub notifier")]
        [Trait("ProcessAnonymizationResult", "ExceptionHandling")]
        public async Task ProcessAnonymizationResult_WhenHubNotifierThrows_PropagatesException()
        {
            // Arrange
            var result = AnonymizationFixture.FindValidAnonymizationResultDto();
            var expectedException = new InvalidOperationException("Hub notification failed");

            var hubNotifierMock = _mocker.GetMock<IHubNotifier>();
            hubNotifierMock
                .Setup(x => x.AnonymizationReadyAsync(result.WoopiAiEmail, result.WoopiAiDocumentId, result.DocumentUrl))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.ProcessAnonymizationResult(result));
            Assert.Equal("Hub notification failed", exception.Message);
        }

        #endregion
    }
}
