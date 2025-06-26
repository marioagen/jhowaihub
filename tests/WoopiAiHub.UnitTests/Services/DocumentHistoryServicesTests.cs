using WoopiAiHub.Application.Services;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.UnitTests.Fixture;
using Moq;
using Moq.AutoMock;
using Xunit;

namespace WoopiAiHub.UnitTests.Services
{
    [Collection(nameof(DocumentCollection))]
    public class DocumentHistoryTests
    {
        private readonly AutoMocker _mocker;
        private readonly DocumentFixture _fixture;
        private readonly DocumentHistoryServices _documentHistoryServices;

        public DocumentHistoryTests(DocumentFixture documentFixture)
        {
            _mocker = new AutoMocker();
            _fixture = documentFixture;
            _documentHistoryServices = _mocker.CreateInstance<DocumentHistoryServices>();
        }

        [Fact(DisplayName = "Test creation of a valid document history")]
        [Trait("Create", "Success")]
        public void Create_Success()
        {
            // Arrange
            var documentHistory = _fixture.FindValidDocumentHistory();
            var documentHistoryRepository = _mocker.GetMock<IDocumentHistoryRepository>();
            documentHistoryRepository.Setup(a => a.Create(It.IsAny<DocumentHistory>())).Returns(true);

            // Act
            var result = _documentHistoryServices.Create(documentHistory);

            // Assert
            Assert.True(result);
            documentHistoryRepository.Verify(a => a.Create(It.IsAny<DocumentHistory>()), Times.Once);
        }

        [Fact(DisplayName = "Test creation of a not valid document history")]
        [Trait("Create", "Fail")]
        public void Create_Fail()
        {
            // Arrange
            var documentHistory = _fixture.FindValidDocumentHistory();
            var documentHistoryRepository = _mocker.GetMock<IDocumentHistoryRepository>();
            documentHistoryRepository.Setup(a => a.Create(It.IsAny<DocumentHistory>())).Returns(false);

            // Act
            var result = _documentHistoryServices.Create(documentHistory);

            // Assert
            Assert.False(result);
            documentHistoryRepository.Verify(a => a.Create(It.IsAny<DocumentHistory>()), Times.Once);
        }

        [Fact(DisplayName = "Test to find by id valid Document history")]
        [Trait("FindById", "Success")]
        public void FindById_Success()
        {
            // Arrange
            var document = _fixture.FindValidDocument();
            var documentHistoryList = _fixture.FindValidDocumentHistoryList();
            var documentHistoryRepository = _mocker.GetMock<IDocumentHistoryRepository>();
            documentHistoryRepository.Setup(a => a.FindById(It.IsAny<int>())).Returns(documentHistoryList);

            // Act
            var result = _documentHistoryServices.FindById(document.Id, document.EmailCreator);

            // Assert
            var list = (List<DocumentHistory>?)result.Value;
            Assert.NotNull(list);
            Assert.Equal(documentHistoryList, list);
            documentHistoryRepository.Verify(a => a.FindById(It.IsAny<int>()), Times.Once);
        }

        [Fact(DisplayName = "Test to find by id not valid document history")]
        [Trait("FindById", "Fail")]
        public void FindById_Fail()
        {
            // Arrange
            var document = _fixture.FindValidDocument();
            var documentHistoryRepository = _mocker.GetMock<IDocumentHistoryRepository>();
            documentHistoryRepository.Setup(a => a.FindById(It.IsAny<int>())).Returns(new List<DocumentHistory>());

            // Act
            var result = _documentHistoryServices.FindById(document.Id, document.EmailCreator);

            // Assert
            var list = (List<DocumentHistory>?)result.Value;
            Assert.NotNull(list);
            Assert.Empty(list);
            documentHistoryRepository.Verify(a => a.FindById(It.IsAny<int>()), Times.Once);
        }

        [Fact(DisplayName = "Test to update history  with valid document history")]
        [Trait("UpdateHistory", "Success")]
        public void UpdateHistory_Success()
        {
            // Arrange
            var updateHistoryDto = _fixture.FindValidUpdateHistoryDto();
            var document = _fixture.FindValidDocument();
            var documentHistoryRepository = _mocker.GetMock<IDocumentHistoryRepository>();
            documentHistoryRepository.Setup(a => a.UpdateHistory(It.IsAny<UpdateHistoryDto>())).Returns(true);

            // Act
            var result = _documentHistoryServices.UpdateHistory(updateHistoryDto, document.EmailCreator);

            // Assert
            Assert.True(result);
            documentHistoryRepository.Verify(a => a.UpdateHistory(It.IsAny<UpdateHistoryDto>()), Times.Once);
        }

        [Fact(DisplayName = "Test to update history witg failed result")]
        [Trait("UpdateHistory", "Fail")]
        public void UpdateHistory_Fail()
        {
            // Arrange
            var updateHistoryDto = _fixture.FindValidUpdateHistoryDto();
            var document = _fixture.FindValidDocument();
            var documentHistoryRepository = _mocker.GetMock<IDocumentHistoryRepository>();
            documentHistoryRepository.Setup(a => a.UpdateHistory(It.IsAny<UpdateHistoryDto>())).Returns(false);

            // Act
            var result = _documentHistoryServices.UpdateHistory(updateHistoryDto, document.EmailCreator);

            // Assert
            Assert.False(result);
            documentHistoryRepository.Verify(a => a.UpdateHistory(It.IsAny<UpdateHistoryDto>()), Times.Once);
        }

        [Fact(DisplayName = "Test to delete document history with valid document history")]
        [Trait("Delete", "Success")]
        public void Delete_Success()
        {
            // Arrange
            var document = _fixture.FindValidDocument();
            var documentHistoryRepository = _mocker.GetMock<IDocumentHistoryRepository>();
            documentHistoryRepository.Setup(a => a.Delete(It.IsAny<int>())).Returns(true);

            // Act
            var result = _documentHistoryServices.Delete(document.Id, document.EmailCreator);

            // Assert
            Assert.True(result);
            documentHistoryRepository.Verify(a => a.Delete(It.IsAny<int>()), Times.Once);
        }

        [Fact(DisplayName = "Test to delete document history with failed result")]
        [Trait("Delete", "Fail")]
        public void Delete_Fail()
        {
            // Arrange
            var document = _fixture.FindValidDocument();
            var documentHistoryRepository = _mocker.GetMock<IDocumentHistoryRepository>();
            documentHistoryRepository.Setup(a => a.Delete(It.IsAny<int>())).Returns(false);

            // Act
            var result = _documentHistoryServices.Delete(document.Id, document.EmailCreator);

            // Assert
            Assert.False(result);
            documentHistoryRepository.Verify(a => a.Delete(It.IsAny<int>()), Times.Once);
        }
    }
}