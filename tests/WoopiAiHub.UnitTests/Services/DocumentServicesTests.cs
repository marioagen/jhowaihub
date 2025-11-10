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
using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Domain.DTOs.Refit;
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
            var iqueryable = new List<Document>().AsQueryable();
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

        [Fact(DisplayName = "ProcessChunks")]
        [Trait("FindByIdAnalyze", "Success")]
        public async Task ProcessChunks_Success()
        {
            // Arrange
            var requestCreateDocumentDto = _fixture.FindValidRequestCreateDocumentDto();
            var fileUploadSummaryDto = _fixture.FindValidFileUploadSummaryDto();
            var tenant = _fixture.FindValidTenantInfoDto();
            var team = DocumentFixture.FindValidTeam();
            var workflows = WorkflowFixture.FindValidWorkflows();
            List<Team> teams = new List<Team> { team };

            var fileRepositoryApi = _mocker.GetMock<IFileRepositoryApi>();
            fileRepositoryApi.Setup(a => a.Upload(It.IsAny<ByteArrayPart>(), It.IsAny<string>())).ReturnsAsync(fileUploadSummaryDto);

            var teamServicesMock = _mocker.GetMock<ITeamServices>();
            teamServicesMock.Setup(a => a.FindByIdsAndUser(It.IsAny<List<int>>(), It.IsAny<string>())).Returns(teams);

            var tenantCache = _mocker.GetMock<ITenantCacheServices>();
            tenantCache.Setup(a => a.FindTenantAsync(It.IsAny<string>(), It.IsAny<ColTypeModule>())).ReturnsAsync(tenant);

            var workflowRepositoryMock = _mocker.GetMock<IWorkflowRepository>();
            workflowRepositoryMock.Setup(a => a.FindByIdsAsync(requestCreateDocumentDto.Workflows)).ReturnsAsync(workflows);

            // Act / Assert
            await _documentServices.ProcessChunks(requestCreateDocumentDto, "tenant");
            fileRepositoryApi.Verify(a => a.Upload(It.IsAny<ByteArrayPart>(), It.IsAny<string>()), Times.Once);
        }

        [Fact(DisplayName = "FindByIdAnalyzeSuccess")]
        [Trait("FindByIdAnalyze", "Success")]
        public void FindByIdAnalyze_Success()
        {
            // Arrange
            var document = _fixture.FindValidDocument();
            var documentRepository = _mocker.GetMock<IDocumentRepository>();
            var cardRepository = _mocker.GetMock<ICardRepository>();
            var functionFileRetriever = _mocker.GetMock<IFunctionFileRetriever>();
            var headers = _fixture.FindValidHeadersDto();

            var card = new Card(1, DateTime.Now, 1, document.Id, "Card Test", 1, true, null);
            documentRepository.Setup(a => a.FindById(It.IsAny<int>())).Returns(document);
            cardRepository.Setup(a => a.FindByDocumentIdCardListAsync(It.IsAny<int>())).ReturnsAsync(new List<Card> { card });

            // Act
            var result = _documentServices.FindByIdAnalyze(document.Id, headers);

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
            Document? document = null;
            var documentRepository = _mocker.GetMock<IDocumentRepository>();
            documentRepository.Setup(a => a.FindById(It.IsAny<int>())).Returns(document);
            document = _fixture.FindValidDocument();
            var headers = _fixture.FindValidHeadersDto();

            // Act / Assert
            Assert.Throws<ArgumentException>(() => _documentServices.FindByIdAnalyze(document.Id, headers));
            documentRepository.Verify(a => a.FindById(It.IsAny<int>()), Times.Once);
        }

        [Fact(DisplayName = "FindDocumentSuccess")]
        [Trait("FindDocument", "Success")]
        public async Task FindDocument_Success()
        {
            // Arrange
            var document = _fixture.FindValidDocument();
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
            var document = _fixture.FindValidDocument();
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
            var document = _fixture.FindValidDocument();
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
            var document = _fixture.FindValidDocument();
            var documentRepository = _mocker.GetMock<IDocumentRepository>();
            documentRepository.Setup(a => a.ChangeStatus(It.IsAny<int>(), It.IsAny<DocumentStatus>())).Returns(false);

            // Act
            var result = await _documentServices.ChangeStatus(document.Id, DocumentStatus.Analyzed, document.EmailCreator);

            // Assert
            Assert.False(result);
            documentRepository.Verify(r => r.ChangeStatus(It.IsAny<int>(), It.IsAny<DocumentStatus>()), Times.Once);
        }

        [Fact(DisplayName = "Delete")]
        [Trait("Delete", "Success")]
        public async Task Delete_Success()
        {
            // Arrange
            var ids = new List<int> { 1, 2, 3 };
            var hashes = new List<string> { "hash1", "hash2" };
            var headers = _fixture.FindValidHeadersDto();

            var documentRepository = _mocker.GetMock<IDocumentRepository>();
            var cardRepository = _mocker.GetMock<ICardRepository>();
            var embeddingsApi = _mocker.GetMock<IEmbeddingsApi>();
            var unitOfWork = _mocker.GetMock<IUnitOfWork>();

            documentRepository.Setup(r => r.Delete(ids)).Returns(true);
            documentRepository.Setup(r => r.FindHashById(ids)).Returns(hashes.AsQueryable());

            embeddingsApi.Setup(api => api.DeleteHash(It.IsAny<string>(), headers.Tenant, headers.KeyMongoAccess))
                         .ReturnsAsync(_fixture.FindHttpResponseMessage);

            cardRepository
                .Setup(r => r.DeleteByDocumentIds(It.IsAny<List<int>>()))
                .ReturnsAsync(true);

            // Act
            var result = await _documentServices.Delete(ids, headers);

            // Assert
            Assert.True(result);
            documentRepository.Verify(r => r.Delete(ids), Times.Once);
            documentRepository.Verify(r => r.FindHashById(ids), Times.Once);
            embeddingsApi.Verify(api => api.DeleteHash(It.IsAny<string>(), headers.Tenant, headers.KeyMongoAccess), Times.Exactly(hashes.Count));
            cardRepository.Verify(r => r.DeleteByDocumentIds(It.IsAny<List<int>>()), Times.Once);
            unitOfWork.Verify(u => u.BeginTransaction(), Times.Once);
            unitOfWork.Verify(u => u.Commit(), Times.Once);
            unitOfWork.Verify(u => u.Rollback(), Times.Never);
        }

        [Fact(DisplayName = "Delete")]
        [Trait("Delete", "Fail")]
        public async Task Delete_FailAsync()
        {
            // Arrange
            List<int> list = new() { 1, 2, 3 };
            List<string> stringArray = new() { "test" };
            var headers = _fixture.FindValidHeadersDto();

            var documentRepository = _mocker.GetMock<IDocumentRepository>();
            var embeddingRepository = _mocker.GetMock<IEmbeddingsApi>();
            var cardRepository = _mocker.GetMock<ICardRepository>();
            var unitOfWork = _mocker.GetMock<IUnitOfWork>();

            documentRepository.Setup(a => a.Delete(list)).Returns(false);
            documentRepository.Setup(a => a.FindHashById(list)).Returns(stringArray.AsQueryable());
            embeddingRepository
                .Setup(a => a.DeleteHash("test", headers.Tenant, headers.KeyMongoAccess))
                .ReturnsAsync(_fixture.FindHttpResponseMessage);
            cardRepository
                .Setup(a => a.DeleteByDocumentIds(It.IsAny<List<int>>()))
                .ReturnsAsync(false);

            // Act
            var result = await _documentServices.Delete(list, headers);

            // Assert
            Assert.False(result);
            documentRepository.Verify(a => a.Delete(list), Times.Once);
            documentRepository.Verify(a => a.FindHashById(list), Times.Once);
            embeddingRepository.Verify(a => a.DeleteHash("test", headers.Tenant, headers.KeyMongoAccess), Times.Once);
            cardRepository.Verify(a => a.DeleteByDocumentIds(It.IsAny<List<int>>()), Times.Once);
            unitOfWork.Verify(u => u.BeginTransaction(), Times.Once);
            unitOfWork.Verify(u => u.Commit(), Times.Once);   // mesmo retornando false, ainda faz commit
            unitOfWork.Verify(u => u.Rollback(), Times.Never);
        }

        [Fact(DisplayName = "InputQuestionnaire")]
        [Trait("InputQuestionnaire", "Success")]
        public async Task InputQuestionnaire_Success()
        {
            // Arrange
            var document = _fixture.FindValidDocument();
            var headers = _fixture.FindValidHeadersDto();
            var tenant = _fixture.FindValidTenantInfoDto();
            var documentQuestionnaireDto = _fixture.FindDocumentQuestionnaireDto();
            var questionnaire = _fixture.FindValidQuestionnaireDto();
            var documentRepository = _mocker.GetMock<IDocumentRepository>();
            documentRepository.Setup(a => a.FindById(1)).Returns(document);
            var questionnaireRepository = _mocker.GetMock<IQuestionnaireRepository>();
            questionnaireRepository.Setup(a => a.FindById(1)).Returns(questionnaire);
            var embeddingsApi = _mocker.GetMock<IEmbeddingsApi>();
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{'key':'value'}", Encoding.UTF8, "application/json")
            };
            embeddingsApi.Setup(a => a.CustomQuery(document.ReferenceFile, It.IsAny<CustomQueryRequestRefitDto>(), "key")).ReturnsAsync(httpResponseMessage);
            var marketPlaceApi = _mocker.GetMock<IMarketPlaceApi>();
            marketPlaceApi.Setup(a => a.ManageConsumptionQuestions(It.IsAny<string>(), It.IsAny<ConsumptionQuestionsDto>())).ReturnsAsync(true);
            var tenantCacheServices = _mocker.GetMock<ITenantCacheServices>();
            tenantCacheServices.Setup(x => x.FindTenantAsync(It.IsAny<string>(), It.IsAny<ColTypeModule>()))
                               .ReturnsAsync(tenant);

            //Act
            var result = await _documentServices.InputQuestionnaire(documentQuestionnaireDto, headers);

            //Assert
            Assert.True(result);
            documentRepository.Verify(a => a.FindById(1), Times.Once);
            questionnaireRepository.Verify(a => a.FindById(1), Times.Once);
            embeddingsApi.Verify(a => a.CustomQuery(document.ReferenceFile, It.IsAny<CustomQueryRequestRefitDto>(), "key"), Times.Once);
            marketPlaceApi.Verify(a => a.ManageConsumptionQuestions(It.IsAny<string>(), It.IsAny<ConsumptionQuestionsDto>()), Times.Once);
            tenantCacheServices.Verify(a => a.FindTenantAsync(It.IsAny<string>(), It.IsAny<ColTypeModule>()), Times.Once());
        }

        [Fact(DisplayName = "InputQuestionnaire")]
        [Trait("InputQuestionnaire", "Fail")]
        public async Task InputQuestionnaire_Fail()
        {
            //Arrange
            var document = _fixture.FindValidDocument();
            var questionnaire = _fixture.FindValidQuestionnaireDto();
            var documentRepository = _mocker.GetMock<IDocumentRepository>();
            var documentQuestionnaireDto = _fixture.FindDocumentQuestionnaireDto();
            documentRepository.Setup(a => a.FindById(1)).Returns(document);
            var questionnaireRepository = _mocker.GetMock<IQuestionnaireRepository>();
            questionnaireRepository.Setup(a => a.FindById(1)).Returns(questionnaire);
            var embeddingsApi = _mocker.GetMock<IEmbeddingsApi>();
            var marketPlaceApi = _mocker.GetMock<IMarketPlaceApi>();
            marketPlaceApi.Setup(a => a.ManageConsumptionQuestions(It.IsAny<string>(), It.IsAny<ConsumptionQuestionsDto>())).ReturnsAsync(false);
            var headers = _fixture.FindValidHeadersDto();

            //Act / Assert
            await Assert.ThrowsAsync<HttpException>(() => _documentServices.InputQuestionnaire(documentQuestionnaireDto, headers));
            documentRepository.Verify(a => a.FindById(1), Times.Once);
            questionnaireRepository.Verify(a => a.FindById(1), Times.Once);
        }

        [Fact(DisplayName = "InputQuestionnaire")]
        [Trait("InputQuestionnaireEmb", "Fail")]
        public async Task InputQuestionnaireEmb_Fail()
        {
            //Arrange
            var documentQuestionnaireDto = _fixture.FindDocumentQuestionnaireDto();
            var headers = _fixture.FindValidHeadersDto();
            headers.KeyMongoAccess = null;
            //Act / Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _documentServices.InputQuestionnaire(documentQuestionnaireDto, headers));
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

        [Fact(DisplayName = "InputDocument")]
        [Trait("InputDocument", "Success")]
        public async Task InputDocument_Success()
        {
            // Arrange
            var document = _fixture.FindValidDocument();
            var headers = _fixture.FindValidHeadersDto();
            var tenant = _fixture.FindValidTenantInfoDto();
            var documentInput = _fixture.FindValidDocumentInputDto();
            var documentRepository = _mocker.GetMock<IDocumentRepository>();
            documentRepository.Setup(a => a.FindById(1)).Returns(document);
            var embeddingsApi = _mocker.GetMock<IEmbeddingsApi>();
            var documentHistoryServices = _mocker.GetMock<IDocumentHistoryServices>();
            var tenantCacheServices = _mocker.GetMock<ITenantCacheServices>();
            documentHistoryServices.Setup(a => a.Create(It.IsAny<DocumentHistory>())).Returns(true);

            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{'response':'value'}", Encoding.UTF8, "application/json")
            };
            embeddingsApi.Setup(a => a.CustomQuery(document.ReferenceFile, It.IsAny<CustomQueryRequestRefitDto>(), "key")).ReturnsAsync(httpResponseMessage);
            var marketPlaceApi = _mocker.GetMock<IMarketPlaceApi>();
            marketPlaceApi.Setup(a => a.ManageConsumptionQuestions(It.IsAny<string>(), It.IsAny<ConsumptionQuestionsDto>())).ReturnsAsync(true);
            tenantCacheServices.Setup(x => x.FindTenantAsync(It.IsAny<string>(), It.IsAny<ColTypeModule>()))
                               .ReturnsAsync(tenant);

            //Act
            var result = await _documentServices.InputDocument(documentInput, headers);

            //Assert
            Assert.NotNull(result);
            documentRepository.Verify(a => a.FindById(1), Times.Once);
            embeddingsApi.Verify(a => a.CustomQuery(document.ReferenceFile, It.IsAny<CustomQueryRequestRefitDto>(), "key"), Times.Once);
            marketPlaceApi.Verify(a => a.ManageConsumptionQuestions(It.IsAny<string>(), It.IsAny<ConsumptionQuestionsDto>()), Times.Once);
            documentHistoryServices.Verify(a => a.Create(It.IsAny<DocumentHistory>()), Times.Once);
            tenantCacheServices.Verify(a => a.FindTenantAsync(It.IsAny<string>(), It.IsAny<ColTypeModule>()), Times.Once());
        }

        [Fact(DisplayName = "InputDocument")]
        [Trait("InputDocument", "Fail")]
        public async Task InputDocument_Fail()
        {
            // Arrange
            var marketPlaceApi = _mocker.GetMock<IMarketPlaceApi>();
            var headers = _fixture.FindValidHeadersDto();
            var documentInput = _fixture.FindValidDocumentInputDto();
            marketPlaceApi.Setup(a => a.ManageConsumptionQuestions(It.IsAny<string>(), It.IsAny<ConsumptionQuestionsDto>())).ReturnsAsync(false);

            // Act /Assert
            await Assert.ThrowsAsync<AppException>(() => _documentServices.InputDocument(documentInput, headers));
        }

        [Fact(DisplayName = "InputDocument")]
        [Trait("InputDocument", "Null")]
        public async Task InputDocument_Null()
        {
            //Arrange
            var headers = _fixture.FindValidHeadersDto();
            var documentInput = _fixture.FindValidDocumentInputDto();
            headers.KeyMongoAccess = null;

            // Act /Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _documentServices.InputDocument(documentInput, headers));
        }

        [Fact(DisplayName = "DeleteHash")]
        [Trait("DeleteHash", "Success")]
        public async Task DeleteHash_Success()
        {
            // Arrange
            bool result;
            var embeddingsRepository = _mocker.GetMock<IEmbeddingsApi>();
            embeddingsRepository.Setup(a => a.DeleteHash("test", "test", "test")).ReturnsAsync(_fixture.FindHttpResponseMessage);

            // Act
            try
            {
                await _documentServices.DeleteHash("test", "test", "test");
                result = true;
            }
            catch (Exception)
            {
                result = false;
            }

            //Assert
            Assert.True(result);
        }

        [Fact(DisplayName = "DeleteHash")]
        [Trait("DeleteHash", "Fail")]
        public async Task DeleteHash_Fail()
        {
            // Arrange
            var embeddingsRepository = _mocker.GetMock<IEmbeddingsApi>();
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("{'response':'value'}", Encoding.UTF8, "application/json")
            };
            embeddingsRepository.Setup(a => a.DeleteHash("test", "test", "test")).ReturnsAsync(_fixture.FindInvalidHttpResponseMessage);

            // Act // Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _documentServices.DeleteHash("test", "test", "test"));
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
            var generatedKey = Guid.NewGuid().ToString();
            var tenant = _fixture.FindValidTenantInfoDto();
            var execution = DocumentFixture.FindValidStepToolExecution();
            var stepTool =  WorkflowFixture.FindValidStepTool();

            var configurationMock = new Mock<IConfiguration>();
            var documentRepositoryMock = _mocker.GetMock<IDocumentRepository>();
            var stepToolExecutionRepositoryMock = _mocker.GetMock<IStepToolExecutionRepository>();
            var stepToolRepositoryMock = _mocker.GetMock<IStepToolRepository>();
            var keyGeneratorMock = _mocker.GetMock<IKeyGeneratorApi>();
            var tenantCacheServices = _mocker.GetMock<ITenantCacheServices>();
            configurationMock.Setup(x => x["keyAccess"]).Returns(Guid.NewGuid().ToString);
            _mocker.Use(configurationMock.Object);
            documentRepositoryMock.Setup(r => r.FindDocumentIdByReferenceFile(processOcrResultDto.ReferenceFile)).Returns(idDocument);
            
            keyGeneratorMock.Setup(k => k.GetKey(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(generatedKey);
            stepToolExecutionRepositoryMock.Setup(e => e.FindByStepToolIdAndCardIdAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(execution);
            tenantCacheServices.Setup(x => x.FindTenantAsync(It.IsAny<string>(), It.IsAny<ColTypeModule>()))
                               .ReturnsAsync(tenant);
            var documentServices = _mocker.CreateInstance<DocumentServices>();
            stepToolRepositoryMock.Setup(s=> s.FindDependentAsync(processOcrResultDto.Data.StepToolId)).ReturnsAsync(stepTool);

            // Act
            var result = await documentServices.ProcessOcrResult(processOcrResultDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(ProcessOcrDataAutomationDto.CardId, result.CardId);
            Assert.Equal(ProcessOcrDataAutomationDto.StepToolId, result.StepToolId);

            configurationMock.Verify(c => c["keyAccess"], Times.Exactly(1));
            documentRepositoryMock.Verify(r => r.FindDocumentIdByReferenceFile(processOcrResultDto.ReferenceFile), Times.Once);
            keyGeneratorMock.Verify(k => k.GetKey(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            tenantCacheServices.Verify(a => a.FindTenantAsync(It.IsAny<string>(), It.IsAny<ColTypeModule>()), Times.Once());
        }

        //teste uniário para ProcessEmbeddingsResult method
        [Fact(DisplayName = "ProcessEmbeddingsResult should successfully process embeddings result")]
        [Trait("ProcessEmbeddingsResult", "Success")]
        public async Task ProcessEmbeddingsResult_Success()
        {
            // Arrange
            var documentEmbeddingsResultDto = DocumentFixture.FindValidDocumentEmbeddingsResultDto();
            var idDocument = 1;
            var marketPlaceApi = _mocker.GetMock<IMarketPlaceApi>();
            var documentRepositoryMock = _mocker.GetMock<IDocumentRepository>();
            var stepToolExecutionRepositoryMock = _mocker.GetMock<IStepToolExecutionRepository>();
            var stepToolExecution = DocumentFixture.FindValidStepToolExecution();


            marketPlaceApi.Setup(a => a.ManageConsumptionPages(It.IsAny<string>(), It.IsAny<ConsumptionPagesDto>())).ReturnsAsync(true);
            documentRepositoryMock.Setup(r => r.FindDocumentIdByReferenceFile(documentEmbeddingsResultDto.ReferenceFile)).Returns(idDocument);
            stepToolExecutionRepositoryMock.Setup(r => r.FindByStepToolIdAndCardIdAsync(documentEmbeddingsResultDto.Data.StepToolId, documentEmbeddingsResultDto.Data.CardId)).ReturnsAsync(stepToolExecution);

             var documentServices = _mocker.CreateInstance<DocumentServices>();

            // Act
            await documentServices.ProcessEmbeddingsResult(documentEmbeddingsResultDto);

            // Assert
            documentRepositoryMock.Verify(r => r.FindDocumentIdByReferenceFile(documentEmbeddingsResultDto.ReferenceFile), Times.Once);
            marketPlaceApi.Verify(s => s.ManageConsumptionPages(It.IsAny<string>(), It.IsAny<ConsumptionPagesDto>()), Times.Once);
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
                DocumentStatus.OCR, true, "test@email.com", documentId, new List<Workflow>(), DateTime.UtcNow);

            var toolType = new ToolType(1, DateTime.UtcNow, HandlersTypes.Ocr, true);
            var tool = new Tool(1, DateTime.UtcNow, "OCR Tool", true, 1, 1, 1, false, null, null);
            typeof(Tool).GetProperty("ToolType")!.SetValue(tool, toolType);

            var stepTool = new StepTool(stepToolId, DateTime.UtcNow, 1, 1, 1, 0, 0);
            typeof(StepTool).GetProperty("Tool")!.SetValue(stepTool, tool);

            var execution = new StepToolExecution(1, DateTime.UtcNow, stepToolId, StatusExecution.Ready, cardId);
            typeof(StepToolExecution).GetProperty("StepTool")!.SetValue(execution, stepTool);

            var card = new Card(cardId, DateTime.UtcNow, 1, documentId, "Card Name", 1, true, null);
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

            var documentServices = _mocker.CreateInstance<DocumentServices>();

            // Act
            var result = await documentServices.FindOcrTextByDocumentId(documentId);

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

            var documentServices = _mocker.CreateInstance<DocumentServices>();

            // Act
            var result = await documentServices.FindOcrTextByDocumentId(documentId);

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
                DocumentStatus.NotAnalyzed, true, "test@email.com", documentId, new List<Workflow>(), DateTime.UtcNow);

            var card = new Card(cardId, DateTime.UtcNow, 1, documentId, "Card Name", 1, true, null);
            typeof(Card).GetProperty("Executions")!.SetValue(card, new List<StepToolExecution>());

            var documentRepositoryMock = _mocker.GetMock<IDocumentRepository>();
            documentRepositoryMock.Setup(r => r.FindById(documentId)).Returns(document);

            var cardRepositoryMock = _mocker.GetMock<ICardRepository>();
            cardRepositoryMock.Setup(r => r.FindByDocumentIdCardAsync(documentId)).ReturnsAsync(card);

            var documentServices = _mocker.CreateInstance<DocumentServices>();

            // Act
            var result = await documentServices.FindOcrTextByDocumentId(documentId);

            // Assert
            Assert.False(result.HasOcr);
            Assert.Empty(result.Content);
            Assert.Equal(referenceFile, result.ReferenceFile);
            documentRepositoryMock.Verify(r => r.FindById(documentId), Times.Once);
            cardRepositoryMock.Verify(r => r.FindByDocumentIdCardAsync(documentId), Times.Once);
        }

        [Fact(DisplayName = "FindByIdAnalyzeWithStepsSuccess")]
        [Trait("FindByIdAnalyzeWithSteps", "Success")]
        public async Task FindByIdAnalyzeWithSteps_Success()
        {
            // Arrange
            var cardId = 1;
            var headers = _fixture.FindValidHeadersDto();
            var document = _fixture.FindValidDocument();
            
            // Create workflow with steps
            var workflow = new Workflow(1, DateTime.Now, new List<Team>(), "Test Workflow");
            var step = new Step(1, DateTime.Now, 1, "Step Test", 1, 1, 1);
            var tool = new Tool(1, DateTime.Now, "Test Tool", true, 2, 1, 1, false, null, null); // ToolTypeId=2 (not OCR)
            var toolType = new ToolType(2, DateTime.Now, "Prompt", true);
            var stepTool = new StepTool(1, DateTime.Now, 1, 1, 1, 0, 0);
            
            // Set up relationships using reflection
            typeof(Tool).GetProperty("ToolType")!.SetValue(tool, toolType);
            typeof(StepTool).GetProperty("Tool")!.SetValue(stepTool, tool);
            typeof(Step).GetProperty("StepTools")!.SetValue(step, new List<StepTool> { stepTool });
            typeof(Step).GetProperty("Workflow")!.SetValue(step, workflow);
            typeof(Workflow).GetProperty("Steps")!.SetValue(workflow, new List<Step> { step });
            
            var card = new Card(cardId, DateTime.Now, 1, document.Id, "Card Test", 1, true, null);
            typeof(Card).GetProperty("Step")!.SetValue(card, step);
            typeof(Card).GetProperty("Document")!.SetValue(card, document);
            
            var outputValue = "{\"Campo1\": \"Valor1\", \"Campo2\": \"Valor2\"}";
            var output = new StepToolOutput(1, DateTime.Now, 1, cardId, outputValue);
            typeof(StepToolOutput).GetProperty("StepTool")!.SetValue(output, stepTool);
            typeof(Card).GetProperty("Outputs")!.SetValue(card, new List<StepToolOutput> { output });

            var cardRepository = _mocker.GetMock<ICardRepository>();
            cardRepository.Setup(a => a.FindById(cardId)).ReturnsAsync(card);

            // Act
            var result = await _documentServices.FindByIdAnalyzeWithSteps(cardId, headers);

            // Assert
            Assert.NotNull(result);
            Assert.Equal($"doc-{document.Id}", result.DocumentId);
            Assert.Equal(document.Name, result.Name);
            Assert.Equal(document.Description, result.Description);
            Assert.Equal(document.ReferenceFile, result.ReferenceFile);
            Assert.NotEmpty(result.Steps);
            cardRepository.Verify(a => a.FindById(cardId), Times.Once);
        }

        [Fact(DisplayName = "FindByIdAnalyzeWithStepsFail")]
        [Trait("FindByIdAnalyzeWithSteps", "Fail")]
        public async Task FindByIdAnalyzeWithSteps_Fail()
        {
            // Arrange
            var cardId = 1;
            var headers = _fixture.FindValidHeadersDto();
            var cardRepository = _mocker.GetMock<ICardRepository>();
            cardRepository.Setup(a => a.FindById(cardId)).ReturnsAsync((Card)null!);

            // Act / Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _documentServices.FindByIdAnalyzeWithSteps(cardId, headers));
            cardRepository.Verify(a => a.FindById(cardId), Times.Once);
        }

        [Fact(DisplayName = "FindByIdAnalyzeWithStepsFailsWhenDocumentNotFound")]
        [Trait("FindByIdAnalyzeWithSteps", "Fail")]
        public async Task FindByIdAnalyzeWithSteps_FailsWhenDocumentNotFound()
        {
            // Arrange
            var cardId = 1;
            var headers = _fixture.FindValidHeadersDto();
            var card = new Card(cardId, DateTime.Now, 1, 1, "Card Test", 1, true, null);
            
            var cardRepository = _mocker.GetMock<ICardRepository>();
            cardRepository.Setup(a => a.FindById(cardId)).ReturnsAsync(card);

            // Act / Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _documentServices.FindByIdAnalyzeWithSteps(cardId, headers));
            cardRepository.Verify(a => a.FindById(cardId), Times.Once);
        }

        [Fact(DisplayName = "FindByIdAnalyzeWithStepsFailsWhenWorkflowNotFound")]
        [Trait("FindByIdAnalyzeWithSteps", "Fail")]
        public async Task FindByIdAnalyzeWithSteps_FailsWhenWorkflowNotFound()
        {
            // Arrange
            var cardId = 1;
            var headers = _fixture.FindValidHeadersDto();
            var document = _fixture.FindValidDocument();
            var card = new Card(cardId, DateTime.Now, 1, document.Id, "Card Test", 1, true, null);
            typeof(Card).GetProperty("Document")!.SetValue(card, document);
            
            var cardRepository = _mocker.GetMock<ICardRepository>();
            cardRepository.Setup(a => a.FindById(cardId)).ReturnsAsync(card);

            // Act / Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _documentServices.FindByIdAnalyzeWithSteps(cardId, headers));
            cardRepository.Verify(a => a.FindById(cardId), Times.Once);
        }

        [Fact(DisplayName = "FindByIdAnalyzeWithStepsFiltersOCROutputs")]
        [Trait("FindByIdAnalyzeWithSteps", "Success")]
        public async Task FindByIdAnalyzeWithSteps_FiltersOCROutputs()
        {
            // Arrange
            var cardId = 1;
            var headers = _fixture.FindValidHeadersDto();
            var document = _fixture.FindValidDocument();
            
            var workflow = new Workflow(1, DateTime.Now, new List<Team>(), "Test Workflow");
            var step = new Step(1, DateTime.Now, 1, "Step Test", 1, 1, 1);
            var ocrTool = new Tool(1, DateTime.Now, "OCR Tool", true, 1, 1, 1, false, null, null);
            var ocrToolType = new ToolType(1, DateTime.Now, "OCR", true);
            var stepTool = new StepTool(1, DateTime.Now, 1, 1, 1, 0, 0);
            
            typeof(Tool).GetProperty("ToolType")!.SetValue(ocrTool, ocrToolType);
            typeof(StepTool).GetProperty("Tool")!.SetValue(stepTool, ocrTool);
            typeof(Step).GetProperty("StepTools")!.SetValue(step, new List<StepTool> { stepTool });
            typeof(Step).GetProperty("Workflow")!.SetValue(step, workflow);
            typeof(Workflow).GetProperty("Steps")!.SetValue(workflow, new List<Step> { step });
            
            var card = new Card(cardId, DateTime.Now, 1, document.Id, "Card Test", 1, true, null);
            typeof(Card).GetProperty("Step")!.SetValue(card, step);
            typeof(Card).GetProperty("Document")!.SetValue(card, document);
            
            var outputValue = "{\"text\": \"OCR Result\"}";
            var output = new StepToolOutput(1, DateTime.Now, 1, cardId, outputValue);
            typeof(StepToolOutput).GetProperty("StepTool")!.SetValue(output, stepTool);
            typeof(Card).GetProperty("Outputs")!.SetValue(card, new List<StepToolOutput> { output });

            var cardRepository = _mocker.GetMock<ICardRepository>();
            cardRepository.Setup(a => a.FindById(cardId)).ReturnsAsync(card);

            // Act
            var result = await _documentServices.FindByIdAnalyzeWithSteps(cardId, headers);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Steps);
            // OCR outputs should be filtered out, so no outputs in the step
            Assert.Empty(result.Steps[0].Outputs);
        }

        [Fact(DisplayName = "FindByIdAnalyzeWithStepsFiltersEmbeddingsOutputs")]
        [Trait("FindByIdAnalyzeWithSteps", "Success")]
        public async Task FindByIdAnalyzeWithSteps_FiltersEmbeddingsOutputs()
        {
            // Arrange
            var cardId = 1;
            var headers = _fixture.FindValidHeadersDto();
            var document = _fixture.FindValidDocument();
            
            var workflow = new Workflow(1, DateTime.Now, new List<Team>(), "Test Workflow");
            var step = new Step(1, DateTime.Now, 1, "Step Test", 1, 1, 1);
            var embeddingsTool = new Tool(1, DateTime.Now, "Embeddings Tool", true, 3, 1, 1, false, null, null);
            var embeddingsToolType = new ToolType(3, DateTime.Now, "Embeddings", true);
            var stepTool = new StepTool(1, DateTime.Now, 1, 1, 1, 0, 0);
            
            typeof(Tool).GetProperty("ToolType")!.SetValue(embeddingsTool, embeddingsToolType);
            typeof(StepTool).GetProperty("Tool")!.SetValue(stepTool, embeddingsTool);
            typeof(Step).GetProperty("StepTools")!.SetValue(step, new List<StepTool> { stepTool });
            typeof(Step).GetProperty("Workflow")!.SetValue(step, workflow);
            typeof(Workflow).GetProperty("Steps")!.SetValue(workflow, new List<Step> { step });
            
            var card = new Card(cardId, DateTime.Now, 1, document.Id, "Card Test", 1, true, null);
            typeof(Card).GetProperty("Step")!.SetValue(card, step);
            typeof(Card).GetProperty("Document")!.SetValue(card, document);
            
            var outputValue = "{\"embedding\": \"[0.1, 0.2, 0.3]\"}";
            var output = new StepToolOutput(1, DateTime.Now, 1, cardId, outputValue);
            typeof(StepToolOutput).GetProperty("StepTool")!.SetValue(output, stepTool);
            typeof(Card).GetProperty("Outputs")!.SetValue(card, new List<StepToolOutput> { output });

            var cardRepository = _mocker.GetMock<ICardRepository>();
            cardRepository.Setup(a => a.FindById(cardId)).ReturnsAsync(card);

            // Act
            var result = await _documentServices.FindByIdAnalyzeWithSteps(cardId, headers);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Steps);
            // Embeddings outputs should be filtered out
            Assert.Empty(result.Steps[0].Outputs);
        }

        [Fact(DisplayName = "FindByIdAnalyzeWithStepsParsesJsonOutput")]
        [Trait("FindByIdAnalyzeWithSteps", "Success")]
        public async Task FindByIdAnalyzeWithSteps_ParsesJsonOutput()
        {
            // Arrange
            var cardId = 1;
            var headers = _fixture.FindValidHeadersDto();
            var document = _fixture.FindValidDocument();
            
            var workflow = new Workflow(1, DateTime.Now, new List<Team>(), "Test Workflow");
            var step = new Step(1, DateTime.Now, 1, "Step Test", 1, 1, 1);
            var tool = new Tool(1, DateTime.Now, "Test Tool", true, 2, 1, 1, false, null, null);
            var toolType = new ToolType(2, DateTime.Now, "Prompt", true);
            var stepTool = new StepTool(1, DateTime.Now, 1, 1, 1, 0, 0);
            
            typeof(Tool).GetProperty("ToolType")!.SetValue(tool, toolType);
            typeof(StepTool).GetProperty("Tool")!.SetValue(stepTool, tool);
            typeof(Step).GetProperty("StepTools")!.SetValue(step, new List<StepTool> { stepTool });
            typeof(Step).GetProperty("Workflow")!.SetValue(step, workflow);
            typeof(Workflow).GetProperty("Steps")!.SetValue(workflow, new List<Step> { step });
            
            var card = new Card(cardId, DateTime.Now, 1, document.Id, "Card Test", 1, true, null);
            typeof(Card).GetProperty("Step")!.SetValue(card, step);
            typeof(Card).GetProperty("Document")!.SetValue(card, document);
            
            var outputValue = "{\"Nome\": \"João Silva\", \"Email\": \"joao@example.com\"}";
            var output = new StepToolOutput(1, DateTime.Now, 1, cardId, outputValue);
            typeof(StepToolOutput).GetProperty("StepTool")!.SetValue(output, stepTool);
            typeof(Card).GetProperty("Outputs")!.SetValue(card, new List<StepToolOutput> { output });

            var cardRepository = _mocker.GetMock<ICardRepository>();
            cardRepository.Setup(a => a.FindById(cardId)).ReturnsAsync(card);

            // Act
            var result = await _documentServices.FindByIdAnalyzeWithSteps(cardId, headers);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Steps);
            Assert.Equal(2, result.Steps[0].Outputs.Count);
            Assert.Contains(result.Steps[0].Outputs, o => o.Label == "Nome" && o.Value == "João Silva");
            Assert.Contains(result.Steps[0].Outputs, o => o.Label == "Email" && o.Value == "joao@example.com");
        }

        [Fact(DisplayName = "FindByIdAnalyzeWithStepsHandlesPlainTextOutput")]
        [Trait("FindByIdAnalyzeWithSteps", "Success")]
        public async Task FindByIdAnalyzeWithSteps_HandlesPlainTextOutput()
        {
            // Arrange
            var cardId = 1;
            var headers = _fixture.FindValidHeadersDto();
            var document = _fixture.FindValidDocument();
            
            var workflow = new Workflow(1, DateTime.Now, new List<Team>(), "Test Workflow");
            var step = new Step(1, DateTime.Now, 1, "Step Test", 1, 1, 1);
            var tool = new Tool(1, DateTime.Now, "Test Tool", true, 2, 1, 1, false, null, null);
            var toolType = new ToolType(2, DateTime.Now, "Prompt", true);
            var stepTool = new StepTool(1, DateTime.Now, 1, 1, 1, 0, 0);
            
            typeof(Tool).GetProperty("ToolType")!.SetValue(tool, toolType);
            typeof(StepTool).GetProperty("Tool")!.SetValue(stepTool, tool);
            typeof(Step).GetProperty("StepTools")!.SetValue(step, new List<StepTool> { stepTool });
            typeof(Step).GetProperty("Workflow")!.SetValue(step, workflow);
            typeof(Workflow).GetProperty("Steps")!.SetValue(workflow, new List<Step> { step });
            
            var card = new Card(cardId, DateTime.Now, 1, document.Id, "Card Test", 1, true, null);
            typeof(Card).GetProperty("Step")!.SetValue(card, step);
            typeof(Card).GetProperty("Document")!.SetValue(card, document);
            
            var outputValue = "This is a plain text response without JSON structure";
            var output = new StepToolOutput(1, DateTime.Now, 1, cardId, outputValue);
            typeof(StepToolOutput).GetProperty("StepTool")!.SetValue(output, stepTool);
            typeof(Card).GetProperty("Outputs")!.SetValue(card, new List<StepToolOutput> { output });

            var cardRepository = _mocker.GetMock<ICardRepository>();
            cardRepository.Setup(a => a.FindById(cardId)).ReturnsAsync(card);

            // Act
            var result = await _documentServices.FindByIdAnalyzeWithSteps(cardId, headers);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Steps);
            Assert.Single(result.Steps[0].Outputs);
            Assert.Equal("Test Tool", result.Steps[0].Outputs[0].Label);
            Assert.Equal(outputValue, result.Steps[0].Outputs[0].Value);
        }

        [Fact(DisplayName = "FindByIdAnalyzeWithStepsHandlesMultipleSteps")]
        [Trait("FindByIdAnalyzeWithSteps", "Success")]
        public async Task FindByIdAnalyzeWithSteps_HandlesMultipleSteps()
        {
            // Arrange
            var cardId = 1;
            var headers = _fixture.FindValidHeadersDto();
            var document = _fixture.FindValidDocument();
            
            var workflow = new Workflow(1, DateTime.Now, new List<Team>(), "Test Workflow");
            var step1 = new Step(1, DateTime.Now, 1, "Step 1", 1, 1, 1);
            var step2 = new Step(2, DateTime.Now, 2, "Step 2", 1, 1, 1);
            var tool = new Tool(1, DateTime.Now, "Test Tool", true, 2, 1, 1, false, null, null);
            var toolType = new ToolType(2, DateTime.Now, "Prompt", true);
            var stepTool1 = new StepTool(1, DateTime.Now, 1, 1, 1, 0, 0);
            var stepTool2 = new StepTool(2, DateTime.Now, 2, 1, 1, 0, 0);
            
            typeof(Tool).GetProperty("ToolType")!.SetValue(tool, toolType);
            typeof(StepTool).GetProperty("Tool")!.SetValue(stepTool1, tool);
            typeof(StepTool).GetProperty("Tool")!.SetValue(stepTool2, tool);
            typeof(Step).GetProperty("StepTools")!.SetValue(step1, new List<StepTool> { stepTool1 });
            typeof(Step).GetProperty("StepTools")!.SetValue(step2, new List<StepTool> { stepTool2 });
            typeof(Step).GetProperty("Workflow")!.SetValue(step1, workflow);
            typeof(Step).GetProperty("Workflow")!.SetValue(step2, workflow);
            typeof(Workflow).GetProperty("Steps")!.SetValue(workflow, new List<Step> { step1, step2 });
            
            var card = new Card(cardId, DateTime.Now, 1, document.Id, "Card Test", 1, true, null);
            typeof(Card).GetProperty("Step")!.SetValue(card, step1);
            typeof(Card).GetProperty("Document")!.SetValue(card, document);
            
            var output1 = new StepToolOutput(1, DateTime.Now, 1, cardId, "{\"Field1\": \"Value1\"}");
            var output2 = new StepToolOutput(2, DateTime.Now, 2, cardId, "{\"Field2\": \"Value2\"}");
            typeof(StepToolOutput).GetProperty("StepTool")!.SetValue(output1, stepTool1);
            typeof(StepToolOutput).GetProperty("StepTool")!.SetValue(output2, stepTool2);
            typeof(Card).GetProperty("Outputs")!.SetValue(card, new List<StepToolOutput> { output1, output2 });

            var cardRepository = _mocker.GetMock<ICardRepository>();
            cardRepository.Setup(a => a.FindById(cardId)).ReturnsAsync(card);

            // Act
            var result = await _documentServices.FindByIdAnalyzeWithSteps(cardId, headers);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Steps.Count);
            Assert.NotEmpty(result.Steps[0].Outputs);
            Assert.NotEmpty(result.Steps[1].Outputs);
            Assert.Equal("2", result.LastProcessedStepId);
        }

        [Fact(DisplayName = "FindByIdAnalyzeWithStepsReturnsEmptyOutputsWhenNoOutputs")]
        [Trait("FindByIdAnalyzeWithSteps", "Success")]
        public async Task FindByIdAnalyzeWithSteps_ReturnsEmptyOutputsWhenNoOutputs()
        {
            // Arrange
            var cardId = 1;
            var headers = _fixture.FindValidHeadersDto();
            var document = _fixture.FindValidDocument();
            
            var workflow = new Workflow(1, DateTime.Now, new List<Team>(), "Test Workflow");
            var step = new Step(1, DateTime.Now, 1, "Step Test", 1, 1, 1);
            var tool = new Tool(1, DateTime.Now, "Test Tool", true, 2, 1, 1, false, null, null);
            var toolType = new ToolType(2, DateTime.Now, "Prompt", true);
            var stepTool = new StepTool(1, DateTime.Now, 1, 1, 1, 0, 0);
            
            typeof(Tool).GetProperty("ToolType")!.SetValue(tool, toolType);
            typeof(StepTool).GetProperty("Tool")!.SetValue(stepTool, tool);
            typeof(Step).GetProperty("StepTools")!.SetValue(step, new List<StepTool> { stepTool });
            typeof(Step).GetProperty("Workflow")!.SetValue(step, workflow);
            typeof(Workflow).GetProperty("Steps")!.SetValue(workflow, new List<Step> { step });
            
            var card = new Card(cardId, DateTime.Now, 1, document.Id, "Card Test", 1, true, null);
            typeof(Card).GetProperty("Step")!.SetValue(card, step);
            typeof(Card).GetProperty("Document")!.SetValue(card, document);
            typeof(Card).GetProperty("Outputs")!.SetValue(card, new List<StepToolOutput>());

            var cardRepository = _mocker.GetMock<ICardRepository>();
            cardRepository.Setup(a => a.FindById(cardId)).ReturnsAsync(card);

            // Act
            var result = await _documentServices.FindByIdAnalyzeWithSteps(cardId, headers);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Steps);
            Assert.Empty(result.Steps[0].Outputs);
            // Last processed step should still be the last step even if no outputs
            Assert.Equal("1", result.LastProcessedStepId);
        }

        [Fact(DisplayName = "FindByIdAnalyzeWithStepsHandlesInvalidJsonGracefully")]
        [Trait("FindByIdAnalyzeWithSteps", "Success")]
        public async Task FindByIdAnalyzeWithSteps_HandlesInvalidJsonGracefully()
        {
            // Arrange
            var cardId = 1;
            var headers = _fixture.FindValidHeadersDto();
            var document = _fixture.FindValidDocument();
            
            var workflow = new Workflow(1, DateTime.Now, new List<Team>(), "Test Workflow");
            var step = new Step(1, DateTime.Now, 1, "Step Test", 1, 1, 1);
            var tool = new Tool(1, DateTime.Now, "Test Tool", true, 2, 1, 1, false, null, null);
            var toolType = new ToolType(2, DateTime.Now, "Prompt", true);
            var stepTool = new StepTool(1, DateTime.Now, 1, 1, 1, 0, 0);
            
            typeof(Tool).GetProperty("ToolType")!.SetValue(tool, toolType);
            typeof(StepTool).GetProperty("Tool")!.SetValue(stepTool, tool);
            typeof(Step).GetProperty("StepTools")!.SetValue(step, new List<StepTool> { stepTool });
            typeof(Step).GetProperty("Workflow")!.SetValue(step, workflow);
            typeof(Workflow).GetProperty("Steps")!.SetValue(workflow, new List<Step> { step });
            
            var card = new Card(cardId, DateTime.Now, 1, document.Id, "Card Test", 1, true, null);
            typeof(Card).GetProperty("Step")!.SetValue(card, step);
            typeof(Card).GetProperty("Document")!.SetValue(card, document);
            
            var outputValue = "{\"field\": \"value\", invalid json";
            var output = new StepToolOutput(1, DateTime.Now, 1, cardId, outputValue);
            typeof(StepToolOutput).GetProperty("StepTool")!.SetValue(output, stepTool);
            typeof(Card).GetProperty("Outputs")!.SetValue(card, new List<StepToolOutput> { output });

            var cardRepository = _mocker.GetMock<ICardRepository>();
            cardRepository.Setup(a => a.FindById(cardId)).ReturnsAsync(card);

            // Act
            var result = await _documentServices.FindByIdAnalyzeWithSteps(cardId, headers);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Steps);
            Assert.Single(result.Steps[0].Outputs);
            // Should fall back to plain text display
            Assert.Equal("Test Tool", result.Steps[0].Outputs[0].Label);
            Assert.Equal(outputValue, result.Steps[0].Outputs[0].Value);
        }
    }
}
