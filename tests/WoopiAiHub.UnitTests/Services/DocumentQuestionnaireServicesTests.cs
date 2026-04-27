using Microsoft.Extensions.Configuration;
using Moq;
using Moq.AutoMock;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Refit;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.UnitTests.Fixture;
using Xunit;

namespace WoopiAiHub.UnitTests.Services
{
    [Collection(nameof(DocumentCollection))]
    public class DocumentQuestionnaireServicesTests
    {
        private readonly DocumentFixture _fixture;
        private readonly AutoMocker _mocker;
        private readonly WoopiAiHub.Application.Services.DocumentQuestionnaireServices _documentQuestionnaireServices;

        public DocumentQuestionnaireServicesTests(DocumentFixture documentFixture)
        {
            _fixture = documentFixture;
            _mocker = new AutoMocker();

            var configMock = new Mock<IConfiguration>();
            configMock.Setup(x => x["IndexerApiKey"]).Returns(Guid.NewGuid().ToString());
            _mocker.Use(configMock.Object);

            _documentQuestionnaireServices = _mocker.CreateInstance<WoopiAiHub.Application.Services.DocumentQuestionnaireServices>();
        }

        [Fact(DisplayName = "InputQuestionnaire - Should process questionnaire and return true when valid")]
        [Trait("InputQuestionnaire", "Success")]
        public async Task InputQuestionnaire_Success()
        {
            // Arrange
            var document = DocumentFixture.FindValidDocument();
            var headers = DocumentFixture.FindValidHeadersDto();
            var tenant = DocumentFixture.FindValidTenantInfoDto();
            var documentQuestionnaireDto = DocumentFixture.FindDocumentQuestionnaireDto();
            var questionnaire = DocumentFixture.FindValidQuestionnaireDto();
            var documentRepository = _mocker.GetMock<IDocumentRepository>();
            documentRepository.Setup(a => a.FindById(1)).Returns(document);
            var questionnaireRepository = _mocker.GetMock<IQuestionnaireRepository>();
            questionnaireRepository.Setup(a => a.FindById(1)).Returns(questionnaire);
            var ragRouter = _mocker.GetMock<IRagInvocationRouter>();
            ragRouter.Setup(a => a.ExecuteCustomQueryAsync(
                    It.IsAny<TenantInfoDto>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CustomQueryRequestRefitDto>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CustomQueryExecutionResult("value", Array.Empty<QueryUsageDto>()));
            var tenantCacheServices = _mocker.GetMock<ITenantCacheServices>();
            tenantCacheServices.Setup(x => x.FindTenantAsync(It.IsAny<string>()))
                               .ReturnsAsync(tenant);

            //Act
            var result = await _documentQuestionnaireServices.InputQuestionnaire(documentQuestionnaireDto, headers);

            //Assert
            Assert.True(result);
            documentRepository.Verify(a => a.FindById(1), Times.Once);
            questionnaireRepository.Verify(a => a.FindById(1), Times.Once);
            ragRouter.Verify(a => a.ExecuteCustomQueryAsync(
                It.IsAny<TenantInfoDto>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CustomQueryRequestRefitDto>(),
                It.IsAny<CancellationToken>()), Times.Once);
            tenantCacheServices.Verify(a => a.FindTenantAsync(It.IsAny<string>()), Times.Once());
        }

        [Fact(DisplayName = "InputDocument - Should process document input and return result when valid")]
        [Trait("InputDocument", "Success")]
        public async Task InputDocument_Success()
        {
            // Arrange
            var document = DocumentFixture.FindValidDocument();
            var headers = DocumentFixture.FindValidHeadersDto();
            var tenant = DocumentFixture.FindValidTenantInfoDto();
            var documentInput = DocumentFixture.FindValidDocumentInputDto();
            var documentRepository = _mocker.GetMock<IDocumentRepository>();
            documentRepository.Setup(a => a.FindById(1)).Returns(document);
            var ragRouter = _mocker.GetMock<IRagInvocationRouter>();
            var documentHistoryServices = _mocker.GetMock<IDocumentHistoryServices>();
            var tenantCacheServices = _mocker.GetMock<ITenantCacheServices>();
            documentHistoryServices.Setup(a => a.Create(It.IsAny<DocumentHistory>())).Returns(true);

            ragRouter.Setup(a => a.ExecuteCustomQueryAsync(
                    It.IsAny<TenantInfoDto>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CustomQueryRequestRefitDto>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CustomQueryExecutionResult("value", Array.Empty<QueryUsageDto>()));
            tenantCacheServices.Setup(x => x.FindTenantAsync(It.IsAny<string>()))
                               .ReturnsAsync(tenant);

            //Act
            var result = await _documentQuestionnaireServices.InputDocument(documentInput, headers);

            //Assert
            Assert.NotNull(result);
            documentRepository.Verify(a => a.FindById(1), Times.Once);
            ragRouter.Verify(a => a.ExecuteCustomQueryAsync(
                It.IsAny<TenantInfoDto>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CustomQueryRequestRefitDto>(),
                It.IsAny<CancellationToken>()), Times.Once);
            documentHistoryServices.Verify(a => a.Create(It.IsAny<DocumentHistory>()), Times.Once);
            tenantCacheServices.Verify(a => a.FindTenantAsync(It.IsAny<string>()), Times.Once());
        }
    }
}
