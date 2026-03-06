using Microsoft.Extensions.Configuration;
using Moq;
using Moq.AutoMock;
using System.Net;
using System.Text;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Domain.Interfaces.Refit;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.UnitTests.Fixture;
using Xunit;

namespace WoopiAiHub.UnitTests.Services
{
    [Collection(nameof(DocumentCollection))]
    public class DocumentDeletionServicesTests
    {
        private readonly DocumentFixture _fixture;
        private readonly AutoMocker _mocker;
        private readonly WoopiAiHub.Application.Services.DocumentDeletionServices _documentDeletionServices;

        public DocumentDeletionServicesTests(DocumentFixture documentFixture)
        {
            _fixture = documentFixture;
            _mocker = new AutoMocker();

            var configMock = new Mock<IConfiguration>();
            configMock.Setup(x => x["IndexerApiKey"]).Returns(Guid.NewGuid().ToString());
            _mocker.Use(configMock.Object);

            _documentDeletionServices = _mocker.CreateInstance<WoopiAiHub.Application.Services.DocumentDeletionServices>();
        }

        [Fact(DisplayName = "Delete - Should delete documents and related data successfully")]
        [Trait("Delete", "Success")]
        public async Task Delete_Success()
        {
            // Arrange
            var ids = new List<int> { 1, 2, 3 };
            var hashes = new List<string> { "hash1", "hash2" };
            var cardIds = new List<int> { 10, 20, 30 };
            var headers = DocumentFixture.FindValidHeadersDto();

            var documentRepository = _mocker.GetMock<IDocumentRepository>();
            var cardRepository = _mocker.GetMock<ICardRepository>();
            var stepToolExecutionRepository = _mocker.GetMock<IStepToolExecutionRepository>();
            var stepToolOutputRepository = _mocker.GetMock<IStepToolOutputRepository>();
            var embeddingsApi = _mocker.GetMock<IEmbeddingsApi>();
            var fileRepositoryApi = _mocker.GetMock<IFileRepositoryApi>();
            var unitOfWork = _mocker.GetMock<IUnitOfWork>();

            documentRepository.Setup(r => r.ClearWorkflowRelationships(ids)).Returns(true);
            documentRepository.Setup(r => r.Delete(ids)).Returns(true);
            documentRepository.Setup(r => r.FindHashById(ids)).Returns(hashes.AsQueryable());

            embeddingsApi.Setup(api => api.DeleteHash(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                         .ReturnsAsync(DocumentFixture.FindHttpResponseMessage());

            fileRepositoryApi.Setup(api => api.Delete(It.IsAny<string>()))
                            .ReturnsAsync(DocumentFixture.FindHttpResponseMessage());

            cardRepository
                .Setup(r => r.FindCardIdsByDocumentIdsAsync(ids))
                .ReturnsAsync(cardIds);
            cardRepository
                .Setup(r => r.DeleteByDocumentIds(It.IsAny<List<int>>()))
                .ReturnsAsync(true);

            stepToolExecutionRepository
                .Setup(r => r.DeleteByCardIds(It.IsAny<IEnumerable<int>>()))
                .Returns(true);

            stepToolOutputRepository
                .Setup(r => r.DeleteByCardIds(It.IsAny<IEnumerable<int>>()))
                .Returns(true);

            // Act
            var result = await _documentDeletionServices.Delete(ids, headers);

            // Assert
            Assert.True(result);
            documentRepository.Verify(r => r.ClearWorkflowRelationships(ids), Times.Once);
            documentRepository.Verify(r => r.Delete(ids), Times.Once);
            documentRepository.Verify(r => r.FindHashById(ids), Times.Once);
            embeddingsApi.Verify(api => api.DeleteHash(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(hashes.Count));
            fileRepositoryApi.Verify(api => api.Delete(It.IsAny<string>()), Times.Exactly(hashes.Count));
            cardRepository.Verify(r => r.FindCardIdsByDocumentIdsAsync(ids), Times.Once);
            cardRepository.Verify(r => r.DeleteByDocumentIds(It.IsAny<List<int>>()), Times.Once);
            stepToolExecutionRepository.Verify(r => r.DeleteByCardIds(It.IsAny<IEnumerable<int>>()), Times.Once);
            stepToolOutputRepository.Verify(r => r.DeleteByCardIds(It.IsAny<IEnumerable<int>>()), Times.Once);
            unitOfWork.Verify(u => u.BeginTransaction(), Times.Once);
            unitOfWork.Verify(u => u.Commit(), Times.Once);
            unitOfWork.Verify(u => u.Rollback(), Times.Never);
        }

        [Fact(DisplayName = "Delete - Should return false when repository delete fails")]
        [Trait("Delete", "Fail")]
        public async Task Delete_FailAsync()
        {
            // Arrange
            List<int> list = new() { 1, 2, 3 };
            List<string> stringArray = new() { "test" };
            var cardIds = new List<int> { 10, 20, 30 };
            var headers = DocumentFixture.FindValidHeadersDto();

            var documentRepository = _mocker.GetMock<IDocumentRepository>();
            var embeddingRepository = _mocker.GetMock<IEmbeddingsApi>();
            var fileRepositoryApi = _mocker.GetMock<IFileRepositoryApi>();
            var cardRepository = _mocker.GetMock<ICardRepository>();
            var stepToolExecutionRepository = _mocker.GetMock<IStepToolExecutionRepository>();
            var stepToolOutputRepository = _mocker.GetMock<IStepToolOutputRepository>();
            var unitOfWork = _mocker.GetMock<IUnitOfWork>();

            documentRepository.Setup(a => a.ClearWorkflowRelationships(list)).Returns(true);
            documentRepository.Setup(a => a.Delete(list)).Returns(false);
            documentRepository.Setup(a => a.FindHashById(list)).Returns(stringArray.AsQueryable());
            embeddingRepository
                .Setup(a => a.DeleteHash(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(DocumentFixture.FindHttpResponseMessage());
            fileRepositoryApi.Setup(api => api.Delete(It.IsAny<string>()))
                            .ReturnsAsync(DocumentFixture.FindHttpResponseMessage());
            cardRepository
                .Setup(a => a.FindCardIdsByDocumentIdsAsync(list))
                .ReturnsAsync(cardIds);
            cardRepository
                .Setup(a => a.DeleteByDocumentIds(It.IsAny<List<int>>()))
                .ReturnsAsync(false);

            stepToolExecutionRepository
                .Setup(a => a.DeleteByCardIds(It.IsAny<IEnumerable<int>>()))
                .Returns(true);

            stepToolOutputRepository
                .Setup(a => a.DeleteByCardIds(It.IsAny<IEnumerable<int>>()))
                .Returns(true);

            // Act
            var result = await _documentDeletionServices.Delete(list, headers);

            // Assert
            Assert.False(result);
            documentRepository.Verify(a => a.ClearWorkflowRelationships(list), Times.Once);
            documentRepository.Verify(a => a.Delete(list), Times.Once);
            documentRepository.Verify(a => a.FindHashById(list), Times.Once);
            embeddingRepository.Verify(a => a.DeleteHash(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            fileRepositoryApi.Verify(api => api.Delete(It.IsAny<string>()), Times.Once);
            cardRepository.Verify(a => a.FindCardIdsByDocumentIdsAsync(list), Times.Once);
            cardRepository.Verify(a => a.DeleteByDocumentIds(It.IsAny<List<int>>()), Times.Once);
            stepToolExecutionRepository.Verify(a => a.DeleteByCardIds(It.IsAny<IEnumerable<int>>()), Times.Once);
            stepToolOutputRepository.Verify(a => a.DeleteByCardIds(It.IsAny<IEnumerable<int>>()), Times.Once);
            unitOfWork.Verify(u => u.BeginTransaction(), Times.Once);
            unitOfWork.Verify(u => u.Commit(), Times.Once);
            unitOfWork.Verify(u => u.Rollback(), Times.Never);
        }

        [Fact(DisplayName = "DeleteHash - Should delete hash from embeddings successfully")]
        [Trait("DeleteHash", "Success")]
        public async Task DeleteHash_Success()
        {
            // Arrange
            bool result;
            var embeddingsRepository = _mocker.GetMock<IEmbeddingsApi>();
            embeddingsRepository.Setup(a => a.DeleteHash(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                                .ReturnsAsync(DocumentFixture.FindHttpResponseMessage());

            // Act
            try
            {
                await _documentDeletionServices.DeleteHash("test", "test");
                result = true;
            }
            catch (Exception)
            {
                result = false;
            }

            //Assert
            Assert.True(result);
        }

        [Fact(DisplayName = "DeleteHash - Should throw ArgumentException when API returns error")]
        [Trait("DeleteHash", "Fail")]
        public async Task DeleteHash_Fail()
        {
            // Arrange
            var embeddingsRepository = _mocker.GetMock<IEmbeddingsApi>();
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("{'response':'value'}", Encoding.UTF8, "application/json")
            };
            embeddingsRepository.Setup(a => a.DeleteHash(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                                .ReturnsAsync(DocumentFixture.FindInvalidHttpResponseMessage());

            // Act // Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _documentDeletionServices.DeleteHash("test", "test"));
        }
    }
}
