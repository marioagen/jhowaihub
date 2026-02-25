using WoopiAiHub.Application.Services;
using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Utils;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.UnitTests.Fixture;
using Moq;
using Moq.AutoMock;
using Xunit;

namespace WoopiAiHub.UnitTests.Services
{
    [Collection(nameof(DocumentCollection))]
    public class DocumentMetadataServicesTests
    {
        private readonly DocumentFixture _fixture;
        private readonly AutoMocker _mocker;
        private readonly DocumentMetadataServices _documentMetadataServices;

        public DocumentMetadataServicesTests(DocumentFixture documentFixture)
        {
            _fixture = documentFixture;
            _mocker = new AutoMocker();
            _documentMetadataServices = _mocker.CreateInstance<DocumentMetadataServices>();
        }

        [Fact(DisplayName = "FindByIdAnalyzeSuccess")]
        [Trait("FindByIdAnalyze", "Success")]
        public void FindByIdAnalyze_Success()
        {
            // Arrange
            var document = DocumentFixture.FindValidDocument();
            var documentRepository = _mocker.GetMock<IDocumentRepository>();
            var cardRepository = _mocker.GetMock<ICardRepository>();
            var headers = DocumentFixture.FindValidHeadersDto();

            var card = new Card(1, DateTime.Now, 1, document.Id, "Card Test", 1, null);
            documentRepository.Setup(a => a.FindById(It.IsAny<int>())).Returns(document);
            cardRepository.Setup(a => a.FindByDocumentIdCardListAsync(It.IsAny<int>())).ReturnsAsync(new List<Card> { card });

            // Act
            var result = _documentMetadataServices.FindByIdAnalyze(document.Id, headers);

            // Assert
            Assert.Equal(document.Name, result.Name);
            Assert.NotNull(result.CardId);
            Assert.Equal(card.Id, result.CardId);
            documentRepository.Verify(a => a.FindById(It.IsAny<int>()), Times.Once);
            cardRepository.Verify(a => a.FindByDocumentIdCardListAsync(It.IsAny<int>()), Times.Once);
        }

        [Fact(DisplayName = "FindByIdAnalyzeFail")]
        [Trait("FindByIdAnalyze", "Fail")]
        public void FindByIdAnalyze_Fail()
        {
            // Arrange
            var documentId = DocumentFixture.FindValidDocument().Id;
            var documentRepository = _mocker.GetMock<IDocumentRepository>();
            documentRepository.Setup(a => a.FindById(It.IsAny<int>())).Returns((Document)(object)null!);
            var headers = DocumentFixture.FindValidHeadersDto();

            // Act / Assert
            Assert.Throws<ArgumentException>(() => _documentMetadataServices.FindByIdAnalyze(documentId, headers));
            documentRepository.Verify(a => a.FindById(It.IsAny<int>()), Times.Once);
        }

        [Fact(DisplayName = "FindOcrTextByDocumentId - Should return OCR text when available")]
        [Trait("FindOcrTextByDocumentId", "Success")]
        public async Task FindOcrTextByDocumentId_Success()
        {
            // Arrange
            var documentId = 1;
            var cardId = 10;
            var stepToolId = 5;
            var referenceFile = "test-file.pdf";

            var document = new Document("Test Document", "Description", referenceFile,
                DocumentStatus.OCR, "test@email.com", documentId, new List<Workflow>(), DateTime.UtcNow);

            var toolType = new ToolType(1, DateTime.UtcNow, HandlersTypes.Ocr, string.Empty, true);
            var tool = new Tool(1, DateTime.UtcNow, "OCR Tool", true, 1, 1, 1, false, null, null);
            typeof(Tool).GetProperty("ToolType")!.SetValue(tool, toolType);

            var stepTool = new StepTool(stepToolId, DateTime.UtcNow, 1, 1, 1, 0, 0);
            typeof(StepTool).GetProperty("Tool")!.SetValue(stepTool, tool);

            var execution = new StepToolExecution(1, DateTime.UtcNow, stepToolId, StatusExecution.Ready, cardId);
            typeof(StepToolExecution).GetProperty("StepTool")!.SetValue(execution, stepTool);

            var card = new Card(cardId, DateTime.UtcNow, 1, documentId, "Card Name", 1, null);
            typeof(Card).GetProperty("Executions")!.SetValue(card, new List<StepToolExecution> { execution });

            var ocrOutput = new DocumentEmbeddingsDataDto
            {
                ReferenceFile = referenceFile,
                DocumentEmbeddings = new List<DocumentEmbeddingsAddDto>
                {
                    new DocumentEmbeddingsAddDto { Text = "Page 1 text", Metadata = new { PageNumber = 1 } },
                    new DocumentEmbeddingsAddDto { Text = "Page 2 text", Metadata = new { PageNumber = 2 } }
                }
            };
            var outputJson = Newtonsoft.Json.JsonConvert.SerializeObject(ocrOutput);

            var documentRepositoryMock = _mocker.GetMock<IDocumentRepository>();
            documentRepositoryMock.Setup(r => r.FindById(documentId)).Returns(document);

            var cardRepositoryMock = _mocker.GetMock<ICardRepository>();
            cardRepositoryMock.Setup(r => r.FindByDocumentIdCardAsync(documentId)).ReturnsAsync(card);

            var stepToolOutputRepositoryMock = _mocker.GetMock<IStepToolOutputRepository>();
            stepToolOutputRepositoryMock.Setup(r => r.FindByStepToolId(stepToolId, cardId)).ReturnsAsync(outputJson);

            // Act
            var result = await _documentMetadataServices.FindOcrTextByDocumentId(documentId);

            // Assert
            Assert.True(result.HasOcr);
            Assert.Contains("Page 1 text", result.Content);
            Assert.Contains("Page 2 text", result.Content);
            Assert.Equal(referenceFile, result.ReferenceFile);
            documentRepositoryMock.Verify(r => r.FindById(documentId), Times.Once);
            cardRepositoryMock.Verify(r => r.FindByDocumentIdCardAsync(documentId), Times.Once);
            stepToolOutputRepositoryMock.Verify(r => r.FindByStepToolId(stepToolId, cardId), Times.Once);
        }

        [Fact(DisplayName = "FindOcrTextByDocumentId - Should return HasOcr false when document not found")]
        [Trait("FindOcrTextByDocumentId", "DocumentNotFound")]
        public async Task FindOcrTextByDocumentId_DocumentNotFound()
        {
            // Arrange
            var documentId = 1;

            var documentRepositoryMock = _mocker.GetMock<IDocumentRepository>();
            documentRepositoryMock.Setup(r => r.FindById(documentId)).Returns((Document)null!);

            // Act
            var result = await _documentMetadataServices.FindOcrTextByDocumentId(documentId);

            // Assert
            Assert.False(result.HasOcr);
            Assert.Empty(result.Content);
            documentRepositoryMock.Verify(r => r.FindById(documentId), Times.Once);
        }

        [Fact(DisplayName = "FindOcrTextByDocumentId - Should return HasOcr false when no OCR execution found")]
        [Trait("FindOcrTextByDocumentId", "NoOcrExecution")]
        public async Task FindOcrTextByDocumentId_NoOcrExecution()
        {
            // Arrange
            var documentId = 1;
            var cardId = 10;
            var referenceFile = "test-file.pdf";

            var document = new Document("Test Document", "Description", referenceFile,
                DocumentStatus.NotAnalyzed, "test@email.com", documentId, new List<Workflow>(), DateTime.UtcNow);

            var card = new Card(cardId, DateTime.UtcNow, 1, documentId, "Card Name", 1, null);
            typeof(Card).GetProperty("Executions")!.SetValue(card, new List<StepToolExecution>());

            var documentRepositoryMock = _mocker.GetMock<IDocumentRepository>();
            documentRepositoryMock.Setup(r => r.FindById(documentId)).Returns(document);

            var cardRepositoryMock = _mocker.GetMock<ICardRepository>();
            cardRepositoryMock.Setup(r => r.FindByDocumentIdCardAsync(documentId)).ReturnsAsync(card);

            // Act
            var result = await _documentMetadataServices.FindOcrTextByDocumentId(documentId);

            // Assert
            Assert.False(result.HasOcr);
            Assert.Empty(result.Content);
            Assert.Equal(referenceFile, result.ReferenceFile);
            documentRepositoryMock.Verify(r => r.FindById(documentId), Times.Once);
            cardRepositoryMock.Verify(r => r.FindByDocumentIdCardAsync(documentId), Times.Once);
        }
    }
}
