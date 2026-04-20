using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;
using Moq.Protected;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Refit;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Enum.Audit;
using WoopiAiHub.Domain.Interfaces.Hubs;
using WoopiAiHub.Domain.Interfaces.Refit;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Models;
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
            var auditCardServiceMock = _mocker.GetMock<IAuditCardService>();

            documentServicesMock
                .Setup(x => x.FindDocumentById(requestDto.DocumentId, headersDto.Tenant))
                .ReturnsAsync(documentDto);

            anonymizationApiMock
                .Setup(x => x.InitiateAnonymization(It.IsAny<string>(), It.IsAny<AnonymizationRequestDto>()))
                .ReturnsAsync(responseDto);

            auditCardServiceMock
                .Setup(x => x.CreateAndSaveAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<AuditCardActionType>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

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
            auditCardServiceMock.Verify(
                x => x.CreateAndSaveAsync(requestDto.CardId, requestDto.WorkflowId, requestDto.DocumentId, AuditCardActionType.AnonymizationRequest, headersDto.EmailCreator, It.IsAny<CancellationToken>()),
                Times.Once);
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
            var auditCardServiceMock = _mocker.GetMock<IAuditCardService>();

            documentServicesMock
                .Setup(x => x.FindDocumentById(requestDto.DocumentId, headersDto.Tenant))
                .ReturnsAsync(documentDto);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.ProcessAnonymization(requestDto, headersDto));
            Assert.Equal(DocumentNotFoundMessage, exception.Message);
            auditCardServiceMock.Verify(
                x => x.CreateAndSaveAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<AuditCardActionType>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
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
            var auditCardServiceMock = _mocker.GetMock<IAuditCardService>();

            documentServicesMock
                .Setup(x => x.FindDocumentById(requestDto.DocumentId, headersDto.Tenant))
                .ReturnsAsync(documentDto);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.ProcessAnonymization(requestDto, headersDto));
            Assert.Equal(DocumentNotFoundMessage, exception.Message);
            auditCardServiceMock.Verify(
                x => x.CreateAndSaveAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<AuditCardActionType>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
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
            var auditCardServiceMock = _mocker.GetMock<IAuditCardService>();

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
            auditCardServiceMock.Verify(
                x => x.CreateAndSaveAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<AuditCardActionType>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        #endregion

        #region ProcessAnonymizationResult Tests

        [Fact(DisplayName = "ProcessAnonymizationResult - Should successfully save anonymization and notify hub")]
        [Trait("ProcessAnonymizationResult", "Success")]
        public async Task ProcessAnonymizationResult_WithValidResult_SavesAndNotifiesSuccessfully()
        {
            // Arrange
            var result = AnonymizationFixture.FindValidAnonymizationResultDto();
            var document = new Document(
                "Test Document",
                "Test Description",
                "test-reference",
                DocumentStatus.Analyzed,
                "test@example.com",
                1,
                new List<Workflow>(),
                DateTime.Now,
                false);

            var documentRepositoryMock = _mocker.GetMock<IDocumentRepository>();
            var documentAnonymizationRepositoryMock = _mocker.GetMock<IDocumentAnonymizationRepository>();
            var hubNotifierMock = _mocker.GetMock<IHubNotifier>();

            documentRepositoryMock
                .Setup(x => x.FindById(result.WoopiAiDocumentId))
                .Returns(document);

            documentAnonymizationRepositoryMock
                .Setup(x => x.CreateAsync(It.IsAny<DocumentAnonymization>()))
                .ReturnsAsync(true);

            hubNotifierMock
                .Setup(x => x.AnonymizationReadyAsync(result.WoopiAiEmail, result.WoopiAiDocumentId, result.DocumentUrl))
                .Returns(Task.CompletedTask);

            // Act
            await _sut.ProcessAnonymizationResult(result);

            // Assert
            documentRepositoryMock.Verify(
                x => x.FindById(result.WoopiAiDocumentId),
                Times.Once);

            documentAnonymizationRepositoryMock.Verify(
                x => x.CreateAsync(It.Is<DocumentAnonymization>(
                    d => d.DocumentId == document.Id && d.DocumentUrl == result.DocumentUrl)),
                Times.Once);

            hubNotifierMock.Verify(
                x => x.AnonymizationReadyAsync(result.WoopiAiEmail, result.WoopiAiDocumentId, result.DocumentUrl),
                Times.Once);
        }

        [Fact(DisplayName = "ProcessAnonymizationResult - Should throw AppException when document is not found")]
        [Trait("ProcessAnonymizationResult", "DocumentNotFound")]
        public async Task ProcessAnonymizationResult_WhenDocumentNotFound_ThrowsAppException()
        {
            // Arrange
            var result = AnonymizationFixture.FindValidAnonymizationResultDto();

            var documentRepositoryMock = _mocker.GetMock<IDocumentRepository>();
            var documentAnonymizationRepositoryMock = _mocker.GetMock<IDocumentAnonymizationRepository>();
            var hubNotifierMock = _mocker.GetMock<IHubNotifier>();

            documentRepositoryMock
                .Setup(x => x.FindById(result.WoopiAiDocumentId))
                .Returns((Document?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(
                () => _sut.ProcessAnonymizationResult(result));

            Assert.Equal(ErrorCode.NotFound, exception.ErrorCode);
            Assert.Equal("Document not found", exception.Message);

            documentAnonymizationRepositoryMock.Verify(
                x => x.CreateAsync(It.IsAny<DocumentAnonymization>()),
                Times.Never);

            hubNotifierMock.Verify(
                x => x.AnonymizationReadyAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact(DisplayName = "ProcessAnonymizationResult - Should propagate exception from document anonymization repository")]
        [Trait("ProcessAnonymizationResult", "ExceptionHandling")]
        public async Task ProcessAnonymizationResult_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            var result = AnonymizationFixture.FindValidAnonymizationResultDto();
            var document = new Document(
                "Test Document",
                "Test Description",
                "test-reference",
                DocumentStatus.Analyzed,
                "test@example.com",
                1,
                new List<Workflow>(),
                DateTime.Now,
                false);

            var expectedException = new InvalidOperationException("Database error");
            var documentRepositoryMock = _mocker.GetMock<IDocumentRepository>();
            var documentAnonymizationRepositoryMock = _mocker.GetMock<IDocumentAnonymizationRepository>();
            var hubNotifierMock = _mocker.GetMock<IHubNotifier>();

            documentRepositoryMock
                .Setup(x => x.FindById(result.WoopiAiDocumentId))
                .Returns(document);

            documentAnonymizationRepositoryMock
                .Setup(x => x.CreateAsync(It.IsAny<DocumentAnonymization>()))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.ProcessAnonymizationResult(result));

            Assert.Equal("Database error", exception.Message);

            hubNotifierMock.Verify(
                x => x.AnonymizationReadyAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact(DisplayName = "ProcessAnonymizationResult - Should propagate exception from hub notifier")]
        [Trait("ProcessAnonymizationResult", "ExceptionHandling")]
        public async Task ProcessAnonymizationResult_WhenHubNotifierThrows_PropagatesException()
        {
            // Arrange
            var result = AnonymizationFixture.FindValidAnonymizationResultDto();
            var document = new Document(
                "Test Document",
                "Test Description",
                "test-reference",
                DocumentStatus.Analyzed,
                "test@example.com",
                1,
                new List<Workflow>(),
                DateTime.Now,
                false);

            var expectedException = new InvalidOperationException("Hub notification failed");

            var documentRepositoryMock = _mocker.GetMock<IDocumentRepository>();
            var documentAnonymizationRepositoryMock = _mocker.GetMock<IDocumentAnonymizationRepository>();
            var hubNotifierMock = _mocker.GetMock<IHubNotifier>();

            documentRepositoryMock
                .Setup(x => x.FindById(result.WoopiAiDocumentId))
                .Returns(document);

            documentAnonymizationRepositoryMock
                .Setup(x => x.CreateAsync(It.IsAny<DocumentAnonymization>()))
                .ReturnsAsync(true);

            hubNotifierMock
                .Setup(x => x.AnonymizationReadyAsync(result.WoopiAiEmail, result.WoopiAiDocumentId, result.DocumentUrl))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.ProcessAnonymizationResult(result));

            Assert.Equal("Hub notification failed", exception.Message);
        }

        #endregion

        #region UploadDocumentToUrl Tests

        [Fact(DisplayName = "ProcessAnonymization - Should throw HttpRequestException when document upload fails")]
        [Trait("ProcessAnonymization", "UploadFailure")]
        public async Task ProcessAnonymization_WhenDocumentUploadFails_ThrowsHttpRequestExceptionAndLogsError()
        {
            // Arrange
            var documentDto = AnonymizationFixture.FindValidFindDocumentDto();
            var requestDto = AnonymizationFixture.FindValidProcessAnonymizationRequestDto();
            var headersDto = AnonymizationFixture.FindValidHeadersDto();
            var responseDto = AnonymizationFixture.FindValidAnonymizationResponseDto();

            var documentServicesMock = _mocker.GetMock<IDocumentServices>();
            var anonymizationApiMock = _mocker.GetMock<IAnonymizationApi>();
            var httpClientFactoryMock = _mocker.GetMock<IHttpClientFactory>();
            var auditCardServiceMock = _mocker.GetMock<IAuditCardService>();
            var loggerMock = _mocker.GetMock<ILogger<AnonymizationServices>>();

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
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent("Server error occurred")
                });

            using var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("http://localhost") };
            httpClientFactoryMock
                .Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<HttpRequestException>(
                () => _sut.ProcessAnonymization(requestDto, headersDto));

            Assert.Contains("Failed to upload document", exception.Message);
            Assert.Contains("Status:", exception.Message);
            auditCardServiceMock.Verify(
                x => x.CreateAndSaveAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<AuditCardActionType>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        #endregion

        #region FindAnonymizedDocumentsByDocument Tests

        [Fact(DisplayName = "FindAnonymizedDocumentsByDocument - Should return collection of anonymized documents when documents exist")]
        [Trait("FindAnonymizedDocumentsByDocument", "Success")]
        public async Task FindAnonymizedDocumentsByDocument_WithValidDocumentId_ReturnsAnonymizedDocumentsCollection()
        {
            // Arrange
            var documentId = 1;
            var anonymizedDocuments = AnonymizationFixture.FindValidDocumentAnonymizationDtoCollection(documentId, 2);

            var documentAnonymizationRepositoryMock = _mocker.GetMock<IDocumentAnonymizationRepository>();
            documentAnonymizationRepositoryMock
                .Setup(x => x.FindAnonymizedDocumentsByDocument(documentId))
                .ReturnsAsync(anonymizedDocuments);

            // Act
            var result = await _sut.FindAnonymizedDocumentsByDocument(documentId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, doc => Assert.Equal(documentId, doc.DocumentId));
            documentAnonymizationRepositoryMock.Verify(
                x => x.FindAnonymizedDocumentsByDocument(documentId),
                Times.Once);
        }

        [Fact(DisplayName = "FindAnonymizedDocumentsByDocument - Should return empty collection when no anonymized documents exist")]
        [Trait("FindAnonymizedDocumentsByDocument", "Success")]
        public async Task FindAnonymizedDocumentsByDocument_WithNoExistingDocuments_ReturnsEmptyCollection()
        {
            // Arrange
            var documentId = 999;
            var emptyCollection = new List<DocumentAnonymizationDto>();

            var documentAnonymizationRepositoryMock = _mocker.GetMock<IDocumentAnonymizationRepository>();
            documentAnonymizationRepositoryMock
                .Setup(x => x.FindAnonymizedDocumentsByDocument(documentId))
                .ReturnsAsync(emptyCollection);

            // Act
            var result = await _sut.FindAnonymizedDocumentsByDocument(documentId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            documentAnonymizationRepositoryMock.Verify(
                x => x.FindAnonymizedDocumentsByDocument(documentId),
                Times.Once);
        }

        [Fact(DisplayName = "FindAnonymizedDocumentsByDocument - Should propagate exception from repository")]
        [Trait("FindAnonymizedDocumentsByDocument", "ExceptionHandling")]
        public async Task FindAnonymizedDocumentsByDocument_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            var documentId = 1;
            var expectedException = new InvalidOperationException("Database error");

            var documentAnonymizationRepositoryMock = _mocker.GetMock<IDocumentAnonymizationRepository>();
            documentAnonymizationRepositoryMock
                .Setup(x => x.FindAnonymizedDocumentsByDocument(documentId))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.FindAnonymizedDocumentsByDocument(documentId));

            Assert.Equal("Database error", exception.Message);
            documentAnonymizationRepositoryMock.Verify(
                x => x.FindAnonymizedDocumentsByDocument(documentId),
                Times.Once);
        }

        #endregion
    }
}
