using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;
using Refit;
using System.Net;
using System.Text;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Domain.DTOs.Refit;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Refit;
using WoopiAiHub.Domain.Interfaces.Refit.Functions;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Utils;
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
                OcrQueue = "ocrQueue"
            });

            _mocker.Use(mockQueues);

            var configMock = new Mock<IConfiguration>();
            configMock.Setup(x => x.GetSection("keyAccess").Value).Returns(Guid.NewGuid().ToString());
            configMock.Setup(x => x.GetSection("UseOcrGoogle").Value).Returns(() => "true");
            configMock.Setup(x => x["RefitExternalSettings:FunctionApiKey"]).Returns(Guid.NewGuid().ToString());

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

        [Fact(DisplayName = "CheckerExceededPages")]
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

        [Fact(DisplayName = "CheckerExceededPages")]
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

        [Fact(DisplayName = "FindAllPaged")]
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

        [Fact(DisplayName = "FindAllPaged")]
        [Trait("FindAllPaged", "Fail")]
        public void FindAllPaged_Fail()
        {
            // Arrange
            var documentRepository = _mocker.GetMock<IDocumentRepository>();
            var pagedData = _fixture.FindInvalidDocumentPagedDataDto();

            // Act / Assert
            Assert.Throws<ArgumentException>(() => _documentServices.FindAllPaged(pagedData, "email"));
        }

        [Fact(DisplayName = "FindDocumentSuccess")]
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

        [Fact(DisplayName = "FindDocumentFail")]
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

        [Fact(DisplayName = "FindStatusAndName")]
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

        [Fact(DisplayName = "ChangeStatus")]
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

        [Fact(DisplayName = "ChangeStatus")]
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

        [Fact(DisplayName = "FindDocumentCount")]
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

        [Fact(DisplayName = "FindDocumentCount")]
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

        [Fact(DisplayName = "ChangeStatusByReferenceFile Success")]
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
            var stepTool =  WorkflowFixture.FindValidStepTool();

            var configurationMock = new Mock<IConfiguration>();
            var documentRepositoryMock = _mocker.GetMock<IDocumentRepository>();
            var stepToolExecutionRepositoryMock = _mocker.GetMock<IStepToolExecutionRepository>();
            var stepToolRepositoryMock = _mocker.GetMock<IStepToolRepository>();
            var tenantCacheServices = _mocker.GetMock<ITenantCacheServices>();
            documentRepositoryMock.Setup(r => r.FindDocumentIdByReferenceFile(processOcrResultDto.ReferenceFile)).Returns(idDocument);
            
            stepToolExecutionRepositoryMock.Setup(e => e.FindByStepToolIdAndCardIdAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(execution);
            tenantCacheServices.Setup(x => x.FindTenantAsync(It.IsAny<string>()))
                               .ReturnsAsync(tenant);
            var documentServices = _mocker.CreateInstance<DocumentServices>();
            stepToolRepositoryMock.Setup(s=> s.FindDependentAsync(processOcrResultDto.Data.StepToolId)).ReturnsAsync(stepTool);

            // Act
            var result = await documentServices.ProcessOcrResult(processOcrResultDto);

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

             var documentServices = _mocker.CreateInstance<DocumentServices>();

            // Act
            await documentServices.ProcessEmbeddingsResult(documentEmbeddingsResultDto);

            // Assert
            documentRepositoryMock.Verify(r => r.FindDocumentIdByReferenceFile(documentEmbeddingsResultDto.ReferenceFile), Times.Once);
        }

        [Fact(DisplayName = "InputToolQuestionnaire should successfully process the document questionnaire and return the data")]
        [Trait("InputToolQuestionnaire", "Success")]
        public async Task InputToolQuestionnaire_Success()
        {
            // Arrange
            var documentEmbeddingsQueryResponseDto = DocumentFixture.FindValidDocumentEmbeddingsQueryResponseDto();
            var ProcessOcrDataAutomationDto = DocumentFixture.FindValidProcessOcrDataAutomationDto();
            var document = DocumentFixture.FindValidDocument();
            var tenant = _fixture.FindValidTenantInfoDto();
            var execution = DocumentFixture.FindValidStepToolExecution();
            var stepTool =  WorkflowFixture.FindValidStepTool();

            var configurationMock = new Mock<IConfiguration>();
            var documentRepositoryMock = _mocker.GetMock<IDocumentRepository>();
            var stepToolExecutionRepositoryMock = _mocker.GetMock<IStepToolExecutionRepository>();
            var stepToolRepositoryMock = _mocker.GetMock<IStepToolRepository>();
            // var tenantCacheServices = _mocker.GetMock<ITenantCacheServices>();
            documentRepositoryMock.Setup(r => r.FindByReferenceFile(documentEmbeddingsQueryResponseDto.ReferenceFile)).Returns(document);

            
            var dataDto = System.Text.Json.JsonSerializer.Deserialize<MetaDataAutomationDto>(documentEmbeddingsQueryResponseDto.Data.ToString());
            
            stepToolExecutionRepositoryMock.Setup(e => e.FindByStepToolIdAndCardIdAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(execution);
            // tenantCacheServices.Setup(x => x.FindTenantAsync(It.IsAny<string>()))
            //                    .ReturnsAsync(tenant);
            var documentServices = _mocker.CreateInstance<DocumentServices>();
            stepToolRepositoryMock.Setup(s=> s.FindDependentAsync(execution.StepTool!.StepId)).ReturnsAsync(stepTool);

            // Act
            var result = await documentServices.InputToolQuestionnaire(documentEmbeddingsQueryResponseDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(document.Id, result.Id);

            documentRepositoryMock.Verify(r => r.FindByReferenceFile(documentEmbeddingsQueryResponseDto.ReferenceFile), Times.Once);
            // tenantCacheServices.Verify(a => a.FindTenantAsync(It.IsAny<string>()), Times.Once());
        }

        [Fact(DisplayName = "InputToolQuestionnaire should not be successfully process the document questionnaire and return null")]
        [Trait("InputToolQuestionnaire", "Fail")]
        public async Task InputToolQuestionnaire_Fail()
        {
            // Arrange
            var documentEmbeddingsQueryResponseDto = DocumentFixture.FindValidDocumentEmbeddingsQueryResponseDto();
            var ProcessOcrDataAutomationDto = DocumentFixture.FindValidProcessOcrDataAutomationDto();
            var document = null as Document;
            var tenant = _fixture.FindValidTenantInfoDto();
            var execution = DocumentFixture.FindValidStepToolExecution();
            var stepTool =  WorkflowFixture.FindValidStepTool();

            var configurationMock = new Mock<IConfiguration>();
            var documentRepositoryMock = _mocker.GetMock<IDocumentRepository>();
            var stepToolExecutionRepositoryMock = _mocker.GetMock<IStepToolExecutionRepository>();
            var stepToolRepositoryMock = _mocker.GetMock<IStepToolRepository>();
            // var tenantCacheServices = _mocker.GetMock<ITenantCacheServices>();
            documentRepositoryMock.Setup(r => r.FindByReferenceFile(documentEmbeddingsQueryResponseDto.ReferenceFile)).Returns(document);

            
            var documentServices = _mocker.CreateInstance<DocumentServices>();

            // Act
            var result = await documentServices.InputToolQuestionnaire(documentEmbeddingsQueryResponseDto);

            // Assert
            Assert.Null(result);
        }
    }
}
