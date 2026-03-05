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

        [Fact(DisplayName = "Create - Should create document history successfully when valid")]
        [Trait("Create", "Success")]
        public void Create_Success()
        {
            // Arrange
            var documentHistory = DocumentFixture.FindValidDocumentHistory();
            var documentHistoryRepository = _mocker.GetMock<IDocumentHistoryRepository>();
            documentHistoryRepository.Setup(a => a.Create(It.IsAny<DocumentHistory>())).Returns(true);

            // Act
            var result = _documentHistoryServices.Create(documentHistory);

            // Assert
            Assert.True(result);
            documentHistoryRepository.Verify(a => a.Create(It.IsAny<DocumentHistory>()), Times.Once);
        }

        [Fact(DisplayName = "Create - Should return false when repository create fails")]
        [Trait("Create", "Fail")]
        public void Create_Fail()
        {
            // Arrange
            var documentHistory = DocumentFixture.FindValidDocumentHistory();
            var documentHistoryRepository = _mocker.GetMock<IDocumentHistoryRepository>();
            documentHistoryRepository.Setup(a => a.Create(It.IsAny<DocumentHistory>())).Returns(false);

            // Act
            var result = _documentHistoryServices.Create(documentHistory);

            // Assert
            Assert.False(result);
            documentHistoryRepository.Verify(a => a.Create(It.IsAny<DocumentHistory>()), Times.Once);
        }

        [Fact(DisplayName = "FindById - Should return document history list when found")]
        [Trait("FindById", "Success")]
        public void FindById_Success()
        {
            // Arrange
            var document = DocumentFixture.FindValidDocument();
            var documentHistoryList = DocumentFixture.FindValidDocumentHistoryList();
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

        [Fact(DisplayName = "FindById - Should return empty list when no history found")]
        [Trait("FindById", "Fail")]
        public void FindById_Fail()
        {
            // Arrange
            var document = DocumentFixture.FindValidDocument();
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

        [Fact(DisplayName = "UpdateHistory - Should update history successfully when valid")]
        [Trait("UpdateHistory", "Success")]
        public void UpdateHistory_Success()
        {
            // Arrange
            var updateHistoryDto = DocumentFixture.FindValidUpdateHistoryDto();
            var document = DocumentFixture.FindValidDocument();
            var documentHistoryRepository = _mocker.GetMock<IDocumentHistoryRepository>();
            documentHistoryRepository.Setup(a => a.UpdateHistory(It.IsAny<UpdateHistoryDto>())).Returns(true);

            // Act
            var result = _documentHistoryServices.UpdateHistory(updateHistoryDto, document.EmailCreator);

            // Assert
            Assert.True(result);
            documentHistoryRepository.Verify(a => a.UpdateHistory(It.IsAny<UpdateHistoryDto>()), Times.Once);
        }

        [Fact(DisplayName = "UpdateHistory - Should return false when repository update fails")]
        [Trait("UpdateHistory", "Fail")]
        public void UpdateHistory_Fail()
        {
            // Arrange
            var updateHistoryDto = DocumentFixture.FindValidUpdateHistoryDto();
            var document = DocumentFixture.FindValidDocument();
            var documentHistoryRepository = _mocker.GetMock<IDocumentHistoryRepository>();
            documentHistoryRepository.Setup(a => a.UpdateHistory(It.IsAny<UpdateHistoryDto>())).Returns(false);

            // Act
            var result = _documentHistoryServices.UpdateHistory(updateHistoryDto, document.EmailCreator);

            // Assert
            Assert.False(result);
            documentHistoryRepository.Verify(a => a.UpdateHistory(It.IsAny<UpdateHistoryDto>()), Times.Once);
        }

        [Fact(DisplayName = "Delete - Should delete document history successfully")]
        [Trait("Delete", "Success")]
        public void Delete_Success()
        {
            // Arrange
            var document = DocumentFixture.FindValidDocument();
            var documentHistoryRepository = _mocker.GetMock<IDocumentHistoryRepository>();
            documentHistoryRepository.Setup(a => a.Delete(It.IsAny<int>())).Returns(true);

            // Act
            var result = _documentHistoryServices.Delete(document.Id, document.EmailCreator);

            // Assert
            Assert.True(result);
            documentHistoryRepository.Verify(a => a.Delete(It.IsAny<int>()), Times.Once);
        }

        [Fact(DisplayName = "Delete - Should return false when repository delete fails")]
        [Trait("Delete", "Fail")]
        public void Delete_Fail()
        {
            // Arrange
            var document = DocumentFixture.FindValidDocument();
            var documentHistoryRepository = _mocker.GetMock<IDocumentHistoryRepository>();
            documentHistoryRepository.Setup(a => a.Delete(It.IsAny<int>())).Returns(false);

            // Act
            var result = _documentHistoryServices.Delete(document.Id, document.EmailCreator);

            // Assert
            Assert.False(result);
            documentHistoryRepository.Verify(a => a.Delete(It.IsAny<int>()), Times.Once);
        }

        [Fact(DisplayName = "FindByIdWithTake - Should return mapped DTOs when entries exist")]
        [Trait("FindByIdWithTake", "Success")]
        public void FindByIdWithTake_ReturnsMappedDtos_Success()
        {
            // Arrange
            var idDocument = 42;
            var take = 10;
            var created = new DateTime(2025, 1, 15, 10, 0, 0, DateTimeKind.Utc);
            var historyList = new List<DocumentHistory>
            {
                new DocumentHistory(idDocument, "input1", "output1", 1, created, 1, null)
            };
            var documentHistoryRepository = _mocker.GetMock<IDocumentHistoryRepository>();
            documentHistoryRepository
                .Setup(a => a.FindByIdWithTake(idDocument, take, null, null, null, null))
                .Returns(historyList);

            // Act
            var result = _documentHistoryServices.FindByIdWithTake(idDocument, take).ToList();

            // Assert
            Assert.Single(result);
            var dto = result[0];
            Assert.Equal(1, dto.Id);
            Assert.Equal(idDocument, dto.IdDocument);
            Assert.Equal("input1", dto.Input);
            Assert.Equal("output1", dto.Output);
            Assert.False(dto.IsEdited);
            Assert.Equal(1, dto.Type);
            Assert.Null(dto.UserId);
            Assert.Null(dto.UserName);
            Assert.Equal(created, dto.Created);
            documentHistoryRepository.Verify(
                a => a.FindByIdWithTake(idDocument, take, null, null, null, null),
                Times.Once);
        }

        [Fact(DisplayName = "FindByIdWithTake - Should return empty when no entries")]
        [Trait("FindByIdWithTake", "Empty")]
        public void FindByIdWithTake_ReturnsEmpty_WhenNoEntries()
        {
            // Arrange
            var idDocument = 99;
            var take = 20;
            var documentHistoryRepository = _mocker.GetMock<IDocumentHistoryRepository>();
            documentHistoryRepository
                .Setup(a => a.FindByIdWithTake(idDocument, take, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid?>()))
                .Returns(Array.Empty<DocumentHistory>());

            // Act
            var result = _documentHistoryServices.FindByIdWithTake(idDocument, take, "search", "desc", "created", Guid.NewGuid()).ToList();

            // Assert
            Assert.Empty(result);
        }

        [Fact(DisplayName = "FindByIdWithTake - Should pass all parameters to repository")]
        [Trait("FindByIdWithTake", "Parameters")]
        public void FindByIdWithTake_PassesParametersToRepository()
        {
            // Arrange
            var idDocument = 5;
            var take = 15;
            var search = "filter text";
            var order = "asc";
            var orderBy = "created";
            var userId = Guid.NewGuid();
            var documentHistoryRepository = _mocker.GetMock<IDocumentHistoryRepository>();
            documentHistoryRepository
                .Setup(a => a.FindByIdWithTake(idDocument, take, search, order, orderBy, userId))
                .Returns(new List<DocumentHistory>());

            // Act
            _ = _documentHistoryServices.FindByIdWithTake(idDocument, take, search, order, orderBy, userId).ToList();

            // Assert
            documentHistoryRepository.Verify(
                a => a.FindByIdWithTake(idDocument, take, search, order, orderBy, userId),
                Times.Once);
        }

        [Fact(DisplayName = "FindByIdWithTake - Should map UserName when User is included")]
        [Trait("FindByIdWithTake", "UserName")]
        public void FindByIdWithTake_MapsUserName_WhenUserIsIncluded()
        {
            // Arrange
            var idDocument = 7;
            var take = 10;
            var user = new User(Guid.NewGuid(), "Alice Smith", "alice@example.com", true, DateTime.UtcNow);
            var created = DateTime.UtcNow.AddDays(-1);
            var history = new DocumentHistory(idDocument, "q", "a", 100, created, 1, user.Id)
            {
                User = user
            };
            var historyList = new List<DocumentHistory> { history };
            var documentHistoryRepository = _mocker.GetMock<IDocumentHistoryRepository>();
            documentHistoryRepository
                .Setup(a => a.FindByIdWithTake(idDocument, take, null, null, null, null))
                .Returns(historyList);

            // Act
            var result = _documentHistoryServices.FindByIdWithTake(idDocument, take).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal("Alice Smith", result[0].UserName);
            Assert.Equal(user.Id, result[0].UserId);
        }
    }
}
