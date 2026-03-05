using Moq;
using Moq.AutoMock;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.UnitTests.Fixture;
using Xunit;

namespace WoopiAiHub.UnitTests.Services
{
    [Collection(nameof(DocumentCollection))]
    public class DocumentNormalizedTests
    {
        private readonly AutoMocker _mocker;
        private readonly DocumentFixture _fixture;
        private readonly DocumentNormalizedServices _documentNormalizedServices;

        public DocumentNormalizedTests(DocumentFixture documentFixture)
        {
            _mocker = new AutoMocker();
            _fixture = documentFixture;
            _documentNormalizedServices = _mocker.CreateInstance<DocumentNormalizedServices>();
        }

        [Fact(DisplayName = "Create - Should create document normalized successfully when valid")]
        [Trait("Create", "Success")]
        public void Create_Success()
        {
            // Arrange
            var documentNormalized = DocumentFixture.FindValidDocumentNormalized();
            var documentNormalizedRepository = _mocker.GetMock<IDocumentNormalizedRepository>();
            documentNormalizedRepository.Setup(a => a.Create(It.IsAny<DocumentNormalized>())).Returns(true);

            // Act
            var result = _documentNormalizedServices.Create(documentNormalized);

            // Assert
            Assert.True(result);
            documentNormalizedRepository.Verify(a => a.Create(It.IsAny<DocumentNormalized>()), Times.Once);
        }

        [Fact(DisplayName = "Create - Should return false when repository create fails")]
        [Trait("Create", "Fail")]
        public void Create_Fail()
        {
            // Arrange
            var documentNormalized = DocumentFixture.FindValidDocumentNormalized();
            var documentNormalizedRepository = _mocker.GetMock<IDocumentNormalizedRepository>();
            documentNormalizedRepository.Setup(a => a.Create(It.IsAny<DocumentNormalized>())).Returns(false);

            // Act
            var result = _documentNormalizedServices.Create(documentNormalized);

            // Assert
            Assert.False(result);
            documentNormalizedRepository.Verify(a => a.Create(It.IsAny<DocumentNormalized>()), Times.Once);
        }

        [Fact(DisplayName = "FindById - Should return document normalized when found")]
        [Trait("FindById", "Success")]
        public void FindById_Success()
        {
            // Arrange
            var document = DocumentFixture.FindValidDocument();
            var documentNormalized = DocumentFixture.FindValidDocumentNormalized();
            var documentNormalizedRepository = _mocker.GetMock<IDocumentNormalizedRepository>();
            documentNormalizedRepository.Setup(a => a.FindById(It.IsAny<int>())).Returns(documentNormalized);

            // Act
            var result = _documentNormalizedServices.FindById(document.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(documentNormalized, result);
            documentNormalizedRepository.Verify(a => a.FindById(It.IsAny<int>()), Times.Once);
        }

        [Fact(DisplayName = "FindById - Should return null when document normalized not found")]
        [Trait("FindById", "Fail")]
        public void FindById_Fail()
        {
            // Arrange
            var document = DocumentFixture.FindValidDocument();
            DocumentNormalized documentNormalized = null;
            var documentNormalizedRepository = _mocker.GetMock<IDocumentNormalizedRepository>();
            documentNormalizedRepository.Setup(a => a.FindById(It.IsAny<int>())).Returns(documentNormalized);

            // Act
            var result = _documentNormalizedServices.FindById(document.Id);

            // Assert
            Assert.Null(result);
            documentNormalizedRepository.Verify(a => a.FindById(It.IsAny<int>()), Times.Once);
        }

        [Fact(DisplayName = "FindById - Should log error and throw AppException when repository throws")]
        [Trait("FindById", "Exception")]
        public void FindById_WhenRepositoryThrows_LogsErrorAndThrowsAppException()
        {
            // Arrange
            var documentId = 1;
            var expectedMessage = "Database connection failed";
            var repositoryException = new InvalidOperationException(expectedMessage);

            var documentNormalizedRepository = _mocker.GetMock<IDocumentNormalizedRepository>();
            documentNormalizedRepository.Setup(a => a.FindById(documentId)).Throws(repositoryException);

            var loggerMock = _mocker.GetMock<Microsoft.Extensions.Logging.ILogger<DocumentNormalizedServices>>();

            // Act
            var exception = Assert.Throws<AppException>(() => _documentNormalizedServices.FindById(documentId));

            // Assert
            Assert.Equal(ErrorCode.DefaultError, exception.ErrorCode);
            Assert.Equal(expectedMessage, exception.Message);
            documentNormalizedRepository.Verify(a => a.FindById(documentId), Times.Once);

            loggerMock.Verify(
                x => x.Log(
                    Microsoft.Extensions.Logging.LogLevel.Error,
                    It.IsAny<Microsoft.Extensions.Logging.EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.Is<Exception>(ex => ex == repositoryException),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact(DisplayName = "FindDocumentNormalizedCount - Should return count of normalized documents")]
        [Trait("FindDocumentNormalizedCount", "Success")]
        public void FindDocumentNormalizedCount_Success()
        {
            // Arrange
            var documentNormalizedRepository = _mocker.GetMock<IDocumentNormalizedRepository>();
            documentNormalizedRepository.Setup(a => a.FindDocumentNormalizedCount()).Returns(1);
            
            // Act
            var result = _documentNormalizedServices.FindDocumentNormalizedCount();

            Assert.Equal(1, result);
            documentNormalizedRepository.Verify(a => a.FindDocumentNormalizedCount(), Times.Once);
        }

        [Fact(DisplayName = "Update - Should update document normalized successfully")]
        [Trait("Update", "Success")]
        public void Update_Success()
        {
            // Arrange
            var documentNormalizedRepository = _mocker.GetMock<IDocumentNormalizedRepository>();
            documentNormalizedRepository.Setup(a => a.Update(It.IsAny<DocumentNormalized>())).Returns(true);

            // Act
            var result = _documentNormalizedServices.Update(It.IsAny<DocumentNormalized>());

            Assert.True(result);
            documentNormalizedRepository.Verify(a => a.Update(It.IsAny<DocumentNormalized>()),Times.Once);
        }

        [Fact(DisplayName = "Update - Should return false when repository update fails")]
        [Trait("Update", "Fail")]
        public void Update_Fail()
        {
            // Arrange
            DocumentNormalized documentNormalized = null;
            var documentNormalizedRepository = _mocker.GetMock<IDocumentNormalizedRepository>();
            documentNormalizedRepository.Setup(a => a.Update(documentNormalized)).Returns(false);

            // Act
            var result = _documentNormalizedServices.Update(It.IsAny<DocumentNormalized>());

            Assert.False(result);
        }

        [Fact(DisplayName = "InsertOrUpdate - Should update when document exists")]
        [Trait("InsertOrUpdate", "Success")]
        public void InsertOrUpdate_ShouldUpdateDocument_WhenDocumentExists()
        {
            // Arrange
            int documentId = 1;
            string normalizedContext = "Test Content";
            var document = DocumentFixture.FindValidDocument();
            var existingDocument = _fixture.FindValidDocumentNormalized();

            var documentRepositoryMock = _mocker.GetMock<IDocumentRepository>();
            documentRepositoryMock.Setup(repo => repo.FindById(It.IsAny<int>())).Returns(document);

            var documentNormalizedRepositoryMock = _mocker.GetMock<IDocumentNormalizedRepository>();
            documentNormalizedRepositoryMock.Setup(repo => repo.FindById(documentId)).Returns(existingDocument);
            documentNormalizedRepositoryMock.Setup(repo => repo.Update(It.IsAny<DocumentNormalized>())).Returns(true);

            // Act
            _documentNormalizedServices.InsertOrUpdate(documentId, normalizedContext);

            // Assert
            documentNormalizedRepositoryMock.Verify(repo => repo.Update(It.IsAny<DocumentNormalized>()), Times.Once);
        }

        [Fact(DisplayName = "InsertOrUpdate - Should return without calling normalized repository when document not found")]
        [Trait("InsertOrUpdate", "EarlyReturn")]
        public void InsertOrUpdate_WhenDocumentNotFound_ReturnsWithoutCallingNormalizedRepository()
        {
            // Arrange
            int documentId = 1;
            string normalizedContext = "Test Content";

            var documentRepositoryMock = _mocker.GetMock<IDocumentRepository>();
            documentRepositoryMock.Setup(repo => repo.FindById(documentId)).Returns((Document)null!);

            var documentNormalizedRepositoryMock = _mocker.GetMock<IDocumentNormalizedRepository>();

            // Act
            _documentNormalizedServices.InsertOrUpdate(documentId, normalizedContext);

            // Assert
            documentRepositoryMock.Verify(repo => repo.FindById(documentId), Times.Once);
            documentNormalizedRepositoryMock.Verify(repo => repo.FindById(It.IsAny<int>()), Times.Never);
            documentNormalizedRepositoryMock.Verify(repo => repo.Create(It.IsAny<DocumentNormalized>()), Times.Never);
            documentNormalizedRepositoryMock.Verify(repo => repo.Update(It.IsAny<DocumentNormalized>()), Times.Never);
        }

        [Fact(DisplayName = "InsertOrUpdate - Should create when document does not exist")]
        [Trait("InsertOrUpdate", "Success")]
        public void InsertOrUpdate_ShouldCreateDocument_WhenDocumentDoesNotExist()
        {
            // Arrange
            int documentId = 1;
            string normalizedContext = "Test Content";
            var document = DocumentFixture.FindValidDocument();

            var documentRepositoryMock = _mocker.GetMock<IDocumentRepository>();
            documentRepositoryMock.Setup(repo => repo.FindById(documentId)).Returns(document);

            var _documentNormalizedRepositoryMock = _mocker.GetMock<IDocumentNormalizedRepository>();
            _documentNormalizedRepositoryMock.Setup(repo => repo.FindById(documentId))
                                             .Returns((DocumentNormalized)null);

            // Act
            _documentNormalizedServices.InsertOrUpdate(documentId, normalizedContext);

            // Assert
            _documentNormalizedRepositoryMock.Verify(repo => repo.Create(It.IsAny<DocumentNormalized>()), Times.Once);
        }

    }
}
