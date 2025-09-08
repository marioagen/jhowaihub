using Moq;
using Moq.AutoMock;
using WoopiAiHub.Application.Services;
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

        [Fact(DisplayName = "Test to create with valid document normalized")]
        [Trait("Create", "Success")]
        public void Create_Success()
        {
            // Arrange
            var documentNormalized = _fixture.FindValidDocumentNormalized();
            var documentNormalizedRepository = _mocker.GetMock<IDocumentNormalizedRepository>();
            documentNormalizedRepository.Setup(a => a.Create(It.IsAny<DocumentNormalized>())).Returns(true);

            // Act
            var result = _documentNormalizedServices.Create(documentNormalized);

            // Assert
            Assert.True(result);
            documentNormalizedRepository.Verify(a => a.Create(It.IsAny<DocumentNormalized>()), Times.Once);
        }

        [Fact(DisplayName = "Test to create with not valid document normalized")]
        [Trait("Create", "Fail")]
        public void Create_Fail()
        {
            // Arrange
            var documentNormalized = _fixture.FindValidDocumentNormalized();
            var documentNormalizedRepository = _mocker.GetMock<IDocumentNormalizedRepository>();
            documentNormalizedRepository.Setup(a => a.Create(It.IsAny<DocumentNormalized>())).Returns(false);

            // Act
            var result = _documentNormalizedServices.Create(documentNormalized);

            // Assert
            Assert.False(result);
            documentNormalizedRepository.Verify(a => a.Create(It.IsAny<DocumentNormalized>()), Times.Once);
        }

        [Fact(DisplayName = "Test to find by id not valid document normalized")]
        [Trait("FindById", "Success")]
        public void FindById_Success()
        {
            // Arrange
            var document = _fixture.FindValidDocument();
            var documentNormalized = _fixture.FindValidDocumentNormalized();
            var documentNormalizedRepository = _mocker.GetMock<IDocumentNormalizedRepository>();
            documentNormalizedRepository.Setup(a => a.FindById(It.IsAny<int>())).Returns(documentNormalized);

            // Act
            var result = _documentNormalizedServices.FindById(document.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(documentNormalized, result);
            documentNormalizedRepository.Verify(a => a.FindById(It.IsAny<int>()), Times.Once);
        }

        [Fact(DisplayName = "Test to find by id not valid document normalized")]
        [Trait("FindById", "Fail")]
        public void FindById_Fail()
        {
            // Arrange
            var document = _fixture.FindValidDocument();
            DocumentNormalized documentNormalized = null;
            var documentNormalizedRepository = _mocker.GetMock<IDocumentNormalizedRepository>();
            documentNormalizedRepository.Setup(a => a.FindById(It.IsAny<int>())).Returns(documentNormalized);

            // Act
            var result = _documentNormalizedServices.FindById(document.Id);

            // Assert
            Assert.Null(result);
            documentNormalizedRepository.Verify(a => a.FindById(It.IsAny<int>()), Times.Once);
        }

        [Fact(DisplayName = "Count normalized document")]
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

        [Fact(DisplayName = "Update updates sucessfully")]
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

        [Fact(DisplayName = "Update fails when try to update")]
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

        [Fact(DisplayName = "InsertOrUpdate updates when document exists")]
        [Trait("InsertOrUpdate", "Success")]
        public void InsertOrUpdate_ShouldUpdateDocument_WhenDocumentExists()
        {
            // Arrange
            int documentId = 1;
            string normalizedContext = "Test Content";
            var existingDocument = _fixture.FindValidDocumentNormalized();
            var _documentNormalizedRepositoryMock = _mocker.GetMock<IDocumentNormalizedRepository>();
            _documentNormalizedRepositoryMock.Setup(repo => repo.FindById(documentId))
                                             .Returns(existingDocument);

            // Act
            _documentNormalizedServices.InsertOrUpdate(documentId, normalizedContext);

            // Assert
            _documentNormalizedRepositoryMock.Verify(repo => repo.Update(It.IsAny<DocumentNormalized>()), Times.Once);
        }

        [Fact(DisplayName = "InsertOrUpdate creates when document exists")]
        [Trait("InsertOrUpdate", "Success")]
        public void InsertOrUpdate_ShouldCreateDocument_WhenDocumentDoesNotExist()
        {
            // Arrange
            int documentId = 1;
            string normalizedContext = "Test Content";
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
