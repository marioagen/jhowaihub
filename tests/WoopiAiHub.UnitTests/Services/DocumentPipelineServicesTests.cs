using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Infrastructure.Messaging.Configuration;
using WoopiAiHub.UnitTests.Fixture;
using Xunit;

namespace WoopiAiHub.UnitTests.Services
{
    [Collection(nameof(DocumentCollection))]
    public class DocumentPipelineServicesTests
    {
        private readonly DocumentFixture _fixture;
        private readonly AutoMocker _mocker;
        private readonly DocumentPipelineServices _documentPipelineServices;

        public DocumentPipelineServicesTests(DocumentFixture documentFixture)
        {
            _fixture = documentFixture;
            _mocker = new AutoMocker();

            var mockQueues = new Mock<IOptions<MessageQueues>>();
            mockQueues.Setup(x => x.Value).Returns(new MessageQueues
            {
                OcrQueue = "ocrQueue",
                EmbeddingQueueAiHubResponse = "embeddingQueue"
            });
            _mocker.Use(mockQueues);

            var configMock = new Mock<IConfiguration>();
            configMock.Setup(x => x["IndexerApiKey"]).Returns(Guid.NewGuid().ToString());
            _mocker.Use(configMock.Object);

            _documentPipelineServices = _mocker.CreateInstance<DocumentPipelineServices>();
        }

        [Fact(DisplayName = "ProcessOcrResult should successfully process OCR result and list of DocumentEmbeddingsAddDto")]
        [Trait("ProcessOcrResult", "Success")]
        public async Task ProcessOcrResult_Success()
        {
            // Arrange
            var processOcrResultDto = DocumentFixture.FindValidProcessOcrResultDto();
            var ProcessOcrDataAutomationDto = DocumentFixture.FindValidProcessOcrDataAutomationDto();
            var idDocument = 1;
            var tenant = _fixture.FindValidTenantInfoDto();
            var execution = DocumentFixture.FindValidStepToolExecution();
            var stepTool = WorkflowFixture.FindValidStepTool();

            var documentRepositoryMock = _mocker.GetMock<IDocumentRepository>();
            var stepToolExecutionRepositoryMock = _mocker.GetMock<IStepToolExecutionRepository>();
            var stepToolRepositoryMock = _mocker.GetMock<IStepToolRepository>();
            var tenantCacheServices = _mocker.GetMock<ITenantCacheServices>();
            documentRepositoryMock.Setup(r => r.FindDocumentIdByReferenceFile(processOcrResultDto.ReferenceFile)).Returns(idDocument);

            stepToolExecutionRepositoryMock.Setup(e => e.FindByStepToolIdAndCardIdAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(execution);
            tenantCacheServices.Setup(x => x.FindTenantAsync(It.IsAny<string>()))
                               .ReturnsAsync(tenant);
            stepToolRepositoryMock.Setup(s => s.FindDependentAsync(processOcrResultDto.Data.StepToolId)).ReturnsAsync(stepTool);

            // Act
            var result = await _documentPipelineServices.ProcessOcrResult(processOcrResultDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(ProcessOcrDataAutomationDto.CardId, result.CardId);
            Assert.Equal(ProcessOcrDataAutomationDto.StepToolId, result.StepToolId);

            documentRepositoryMock.Verify(r => r.FindDocumentIdByReferenceFile(processOcrResultDto.ReferenceFile), Times.Once);
            tenantCacheServices.Verify(a => a.FindTenantAsync(It.IsAny<string>()), Times.Once());
        }

        [Fact(DisplayName = "ProcessEmbeddingsResult should successfully process embeddings result")]
        [Trait("ProcessEmbeddingsResult", "Success")]
        public async Task ProcessEmbeddingsResult_Success()
        {
            // Arrange
            var documentEmbeddingsResultDto = DocumentFixture.FindValidDocumentEmbeddingsResultDto();
            var idDocument = 1;
            var documentRepositoryMock = _mocker.GetMock<IDocumentRepository>();
            var stepToolExecutionRepositoryMock = _mocker.GetMock<IStepToolExecutionRepository>();
            var stepToolExecution = DocumentFixture.FindValidStepToolExecution();

            documentRepositoryMock.Setup(r => r.FindDocumentIdByReferenceFile(documentEmbeddingsResultDto.ReferenceFile)).Returns(idDocument);
            stepToolExecutionRepositoryMock.Setup(r => r.FindByStepToolIdAndCardIdAsync(documentEmbeddingsResultDto.Data.StepToolId, documentEmbeddingsResultDto.Data.CardId)).ReturnsAsync(stepToolExecution);

            // Act
            await _documentPipelineServices.ProcessEmbeddingsResult(documentEmbeddingsResultDto);

            // Assert
            documentRepositoryMock.Verify(r => r.FindDocumentIdByReferenceFile(documentEmbeddingsResultDto.ReferenceFile), Times.Once);
        }

        [Fact(DisplayName = "InputToolQuestionnaire should successfully process the document questionnaire and return the data")]
        [Trait("InputToolQuestionnaire", "Success")]
        public async Task InputToolQuestionnaire_Success()
        {
            // Arrange
            var documentEmbeddingsQueryResponseDto = DocumentFixture.FindValidDocumentEmbeddingsQueryResponseDto();
            var document = DocumentFixture.FindValidDocument();
            var execution = DocumentFixture.FindValidStepToolExecution();
            var stepTool = WorkflowFixture.FindValidStepTool();

            var documentRepositoryMock = _mocker.GetMock<IDocumentRepository>();
            var stepToolExecutionRepositoryMock = _mocker.GetMock<IStepToolExecutionRepository>();
            var stepToolRepositoryMock = _mocker.GetMock<IStepToolRepository>();
            documentRepositoryMock.Setup(r => r.FindByReferenceFile(documentEmbeddingsQueryResponseDto.ReferenceFile)).Returns(document);

            stepToolExecutionRepositoryMock.Setup(e => e.FindByStepToolIdAndCardIdAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(execution);
            stepToolRepositoryMock.Setup(s => s.FindDependentAsync(execution.StepTool!.StepId)).ReturnsAsync(stepTool);

            // Act
            var result = await _documentPipelineServices.InputToolQuestionnaire(documentEmbeddingsQueryResponseDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(document.Id, result.Id);

            documentRepositoryMock.Verify(r => r.FindByReferenceFile(documentEmbeddingsQueryResponseDto.ReferenceFile), Times.Once);
        }

        [Fact(DisplayName = "InputToolQuestionnaire should not be successfully process the document questionnaire and return null")]
        [Trait("InputToolQuestionnaire", "Fail")]
        public async Task InputToolQuestionnaire_Fail()
        {
            // Arrange
            var documentEmbeddingsQueryResponseDto = DocumentFixture.FindValidDocumentEmbeddingsQueryResponseDto();
            var document = null as Document;

            var documentRepositoryMock = _mocker.GetMock<IDocumentRepository>();
            documentRepositoryMock.Setup(r => r.FindByReferenceFile(documentEmbeddingsQueryResponseDto.ReferenceFile)).Returns(document);

            // Act
            var result = await _documentPipelineServices.InputToolQuestionnaire(documentEmbeddingsQueryResponseDto);

            // Assert
            Assert.Null(result);
        }
    }
}
