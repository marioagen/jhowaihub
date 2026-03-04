using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Refit;
using WoopiAiHub.Domain.Interfaces.Refit.Functions;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Infrastructure.Messaging.Configuration;
using WoopiAiHub.UnitTests.Fixture;
using Xunit;

namespace WoopiAiHub.UnitTests.Services
{
    [Collection(nameof(DocumentCollection))]
    public class DocumentServicesTests
    {
        private readonly DocumentFixture _fixture;
        private readonly AutoMocker _mocker;
        private readonly DocumentServices _documentServices;

        public DocumentServicesTests(DocumentFixture documentFixture)
        {
            this._fixture = documentFixture;
            _mocker = new AutoMocker();

            var mockQueues = new Mock<IOptions<MessageQueues>>();
            mockQueues.Setup(x => x.Value).Returns(new MessageQueues
            {
                OcrQueue = "ocrQueue",
                EmbeddingQueueAiHubResponse = "embeddingQueue"
            });

            _mocker.Use(mockQueues);

            var configMock = new Mock<IConfiguration>();
            configMock.Setup(x => x.GetSection("keyAccess").Value).Returns(Guid.NewGuid().ToString());
            configMock.Setup(x => x.GetSection("UseOcrGoogle").Value).Returns(() => "true");
            configMock.Setup(x => x["RefitExternalSettings:FunctionApiKey"]).Returns(Guid.NewGuid().ToString());
            configMock.Setup(x => x["IndexerApiKey"]).Returns(Guid.NewGuid().ToString());

            _mocker.Use(configMock.Object);

            var httpContext = new Mock<HttpContext>();
            httpContext.Setup(x => x.Request.Headers["X-Tenant"]).Returns("blabla");
            _mocker.Use(httpContext.Object);

            var httpContextAccessor = new Mock<IHttpContextAccessor>();
            httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext.Object);

            _mocker.Use(httpContextAccessor.Object);

            _mocker.Use<IMemoryCache>(new MemoryCache(new MemoryCacheOptions()));

            _documentServices = _mocker.CreateInstance<DocumentServices>();
        }

        [Fact(DisplayName = "CheckerExceededPages - Should return true when pages exceeded")]
        [Trait("CheckerExceededPages", "Success")]
        public async Task CheckerExceededPages_Success()
        {
            // Arrange
            var marketPlaceApi = _mocker.GetMock<IMarketPlaceApi>();
            marketPlaceApi.Setup(a => a.CheckExceededPages(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            // Act
            var result = await _documentServices.CheckerExceededPages(It.IsAny<string>());

            // Assert
            Assert.True(result);
            marketPlaceApi.Verify(a => a.CheckExceededPages(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact(DisplayName = "CheckerExceededPages - Should return false when pages not exceeded")]
        [Trait("CheckerExceededPages", "Fail")]
        public async Task CheckerExceededPages_Fail()
        {
            // Arrange
            var marketPlaceApi = _mocker.GetMock<IMarketPlaceApi>();
            marketPlaceApi.Setup(a => a.CheckExceededPages(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);

            // Act
            var result = await _documentServices.CheckerExceededPages(It.IsAny<string>());

            // Assert
            Assert.False(result);
            marketPlaceApi.Verify(a => a.CheckExceededPages(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact(DisplayName = "FindAllPaged - Should return paged documents when valid paged data")]
        [Trait("FindAllPaged", "Success")]
        public void FindAllPaged_Success()
        {
            // Arrange
            var documentRepository = _mocker.GetMock<IDocumentRepository>();
            var pagedData = _fixture.FindValidDocumentPagedDataDto();
            var iqueryable = new List<DocumentListItemDto>().AsQueryable();
            documentRepository.Setup(a => a.FindAllOrdered(pagedData, "email")).Returns(iqueryable);

            // Act
            var result = _documentServices.FindAllPaged(pagedData, "email");

            // Assert
            Assert.NotNull(result);
            documentRepository.Verify(a => a.FindAllOrdered(pagedData, "email"), Times.Once);
        }

        [Fact(DisplayName = "FindAllPaged - Should throw ArgumentException when paged data invalid")]
        [Trait("FindAllPaged", "Fail")]
        public void FindAllPaged_Fail()
        {
            // Arrange
            var documentRepository = _mocker.GetMock<IDocumentRepository>();
            var pagedData = _fixture.FindInvalidDocumentPagedDataDto();

            // Act / Assert
            Assert.Throws<ArgumentException>(() => _documentServices.FindAllPaged(pagedData, "email"));
        }

        [Fact(DisplayName = "FindDocumentById - Should return document when found")]
        [Trait("FindDocument", "Success")]
        public async Task FindDocument_Success()
        {
            // Arrange
            var document = DocumentFixture.FindValidDocument();
            var documentRepository = _mocker.GetMock<IDocumentRepository>();
            var functionFileRetriever = _mocker.GetMock<IFunctionFileRetriever>();
            documentRepository.Setup(a => a.FindById(It.IsAny<int>())).Returns(document);
            functionFileRetriever.Setup(a => a.Get(It.IsAny<string>(),
                                                   It.IsAny<string>(),
                                                   It.IsAny<string>())).ReturnsAsync(_fixture.FindHttpResponseMessage());

            // Act
            var result = await _documentServices.FindDocumentById(It.IsAny<int>(),
                                                                  It.IsAny<string>());

            // Assert
            Assert.NotNull(result);
            functionFileRetriever.Verify(a => a.Get(It.IsAny<string>(),
                                                    It.IsAny<string>(),
                                                    It.IsAny<string>()), Times.Once);
        }

        [Fact(DisplayName = "FindDocumentById - Should throw ArgumentNullException when API key empty")]
        [Trait("FindDocument", "Fail")]
        public async Task FindDocument_Fail()
        {
            // Arrange
            var id = 1;
            var tenant = "Tenant";
            var configMockError = new Mock<IConfiguration>();
            configMockError.Setup(x => x["RefitExternalSettings:FunctionApiKey"]).Returns(string.Empty);
            _mocker.Use(configMockError.Object);
            var documentServices = _mocker.CreateInstance<DocumentServices>();

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => documentServices.FindDocumentById(id, tenant));
        }

        [Fact(DisplayName = "FindStatusAndName - Should return status and name when document found")]
        [Trait("FindStatusAndName", "Success")]
        public void FindStatusAndName_Success()
        {
            // Arrange
            var document = DocumentFixture.FindValidDocument();
            var documentRepository = _mocker.GetMock<IDocumentRepository>();
            documentRepository.Setup(a => a.FindById(It.IsAny<int>())).Returns(document);

            // Act
            var result = _documentServices.FindStatusAndName(document.Id, document.EmailCreator);

            // Assert
            Assert.NotNull(result);
            documentRepository.Verify(a => a.FindById(It.IsAny<int>()), Times.Once);
        }

        [Fact(DisplayName = "ChangeStatus - Should return true when status updated successfully")]
        [Trait("ChangeStatus", "Success")]
        public async Task ChangeStatus_Success()
        {
            // Arrange
            var document = DocumentFixture.FindValidDocument();
            var documentRepository = _mocker.GetMock<IDocumentRepository>();
            documentRepository.Setup(a => a.ChangeStatus(It.IsAny<int>(), It.IsAny<DocumentStatus>())).Returns(true);

            // Act
            var result = await _documentServices.ChangeStatus(document.Id, DocumentStatus.Analyzed, document.EmailCreator);

            // Assert
            Assert.True(result);
            documentRepository.Verify(a => a.ChangeStatus(It.IsAny<int>(), It.IsAny<DocumentStatus>()), Times.Once);
        }

        [Fact(DisplayName = "ChangeStatus - Should return false when repository update fails")]
        [Trait("ChangeStatus", "Fail")]
        public async Task ChangeStatus_Fail()
        {
            // Arrange
            var document = DocumentFixture.FindValidDocument();
            var documentRepository = _mocker.GetMock<IDocumentRepository>();
            documentRepository.Setup(a => a.ChangeStatus(It.IsAny<int>(), It.IsAny<DocumentStatus>())).Returns(false);

            // Act
            var result = await _documentServices.ChangeStatus(document.Id, DocumentStatus.Analyzed, document.EmailCreator);

            // Assert
            Assert.False(result);
            documentRepository.Verify(r => r.ChangeStatus(It.IsAny<int>(), It.IsAny<DocumentStatus>()), Times.Once);
        }

        [Fact(DisplayName = "FindDocumentCount - Should return document count when documents exist")]
        [Trait("FindDocumentCount", "Success")]
        public void FindDocumentCount_Success()
        {
            //Arrange
            var documentRepository = _mocker.GetMock<IDocumentRepository>();
            documentRepository.Setup(a => a.FindDocumentCount()).Returns(1);

            //Act
            var result = _documentServices.FindDocumentCount();

            //Assert
            Assert.Equal(1, result);
            documentRepository.Verify(a => a.FindDocumentCount(), Times.Once);

        }

        [Fact(DisplayName = "FindDocumentCount - Should return zero when no documents")]
        [Trait("FindDocumentCount", "Fail")]
        public void FindDocumentCount_Fail()
        {
            //Arrange
            var documentRepository = _mocker.GetMock<IDocumentRepository>();
            documentRepository.Setup(a => a.FindDocumentCount()).Returns(0);

            //Act
            var result = _documentServices.FindDocumentCount();

            //Assert
            Assert.Equal(0, result);
            documentRepository.Verify(a => a.FindDocumentCount(), Times.Once);

        }

        [Fact(DisplayName = "ChangeStatusByReferenceFile - Should return true when status updated successfully")]
        [Trait("ChangeStatusByReferenceFile", "Success")]
        public async Task ChangeStatusByReferenceFile_Success()
        {
            // Arrange
            string referenceFile = string.Empty;
            string emailCreator = string.Empty;
            DocumentStatus status = DocumentStatus.Analyzed;
            int documentId = 1;
            var documentRepository = _mocker.GetMock<IDocumentRepository>();
            documentRepository.Setup(a => a.FindDocumentIdByReferenceFile(referenceFile)).Returns(documentId);
            documentRepository.Setup(a => a.ChangeStatus(It.IsAny<int>(), It.IsAny<DocumentStatus>())).Returns(true);

            //Act
            var result =  await _documentServices.ChangeStatusByReferenceFile(referenceFile, emailCreator, status);

            Assert.True(result);
            documentRepository.Verify(a => a.FindDocumentIdByReferenceFile(referenceFile), Times.Once);
            documentRepository.Verify(a => a.ChangeStatus(It.IsAny<int>(), It.IsAny<DocumentStatus>()), Times.Once);
        }
    }
}
