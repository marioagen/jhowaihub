using Microsoft.Extensions.Configuration;
using Moq;
using Moq.AutoMock;
using System.Net;
using System.Text;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Domain.DTOs.Refit;
using WoopiAiHub.Domain.Interfaces.Refit;
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
        private readonly DocumentQuestionnaireServices _documentQuestionnaireServices;

        public DocumentQuestionnaireServicesTests(DocumentFixture documentFixture)
        {
            _fixture = documentFixture;
            _mocker = new AutoMocker();

            var configMock = new Mock<IConfiguration>();
            configMock.Setup(x => x["IndexerApiKey"]).Returns(Guid.NewGuid().ToString());
            _mocker.Use(configMock.Object);

            _documentQuestionnaireServices = _mocker.CreateInstance<DocumentQuestionnaireServices>();
        }

        [Fact(DisplayName = "InputQuestionnaire")]
        [Trait("InputQuestionnaire", "Success")]
        public async Task InputQuestionnaire_Success()
        {
            // Arrange
            var document = DocumentFixture.FindValidDocument();
            var headers = DocumentFixture.FindValidHeadersDto();
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
                Content = new StringContent("{\"response\":\"value\",\"Usage\":[]}", Encoding.UTF8, "application/json")
            };
            embeddingsApi.Setup(a => a.CustomQuery(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CustomQueryRequestRefitDto>(), It.IsAny<string>()))
                         .ReturnsAsync(httpResponseMessage);
            var tenantCacheServices = _mocker.GetMock<ITenantCacheServices>();
            tenantCacheServices.Setup(x => x.FindTenantAsync(It.IsAny<string>()))
                               .ReturnsAsync(tenant);

            //Act
            var result = await _documentQuestionnaireServices.InputQuestionnaire(documentQuestionnaireDto, headers);

            //Assert
            Assert.True(result);
            documentRepository.Verify(a => a.FindById(1), Times.Once);
            questionnaireRepository.Verify(a => a.FindById(1), Times.Once);
            embeddingsApi.Verify(a => a.CustomQuery(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CustomQueryRequestRefitDto>(), It.IsAny<string>()), Times.Once);
            tenantCacheServices.Verify(a => a.FindTenantAsync(It.IsAny<string>()), Times.Once());
        }

        [Fact(DisplayName = "InputDocument")]
        [Trait("InputDocument", "Success")]
        public async Task InputDocument_Success()
        {
            // Arrange
            var document = DocumentFixture.FindValidDocument();
            var headers = DocumentFixture.FindValidHeadersDto();
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
                Content = new StringContent("{\"response\":\"value\",\"Usage\":[]}", Encoding.UTF8, "application/json")
            };
            embeddingsApi.Setup(a => a.CustomQuery(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CustomQueryRequestRefitDto>(), It.IsAny<string>()))
                         .ReturnsAsync(httpResponseMessage);
            tenantCacheServices.Setup(x => x.FindTenantAsync(It.IsAny<string>()))
                               .ReturnsAsync(tenant);

            //Act
            var result = await _documentQuestionnaireServices.InputDocument(documentInput, headers);

            //Assert
            Assert.NotNull(result);
            documentRepository.Verify(a => a.FindById(1), Times.Once);
            embeddingsApi.Verify(a => a.CustomQuery(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CustomQueryRequestRefitDto>(), It.IsAny<string>()), Times.Once);
            documentHistoryServices.Verify(a => a.Create(It.IsAny<DocumentHistory>()), Times.Once);
            tenantCacheServices.Verify(a => a.FindTenantAsync(It.IsAny<string>()), Times.Once());
        }
    }
}
