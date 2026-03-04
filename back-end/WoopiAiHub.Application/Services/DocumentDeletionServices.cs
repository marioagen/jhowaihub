using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Refit;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Utils;

namespace WoopiAiHub.Application.Services
{
    public class DocumentDeletionServices : IDocumentDeletionServices
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly ICardRepository _cardRepository;
        private readonly IStepToolExecutionRepository _stepToolExecutionRepository;
        private readonly IStepToolOutputRepository _stepToolOutputRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmbeddingsApi _embbedingsApi;
        private readonly IConfiguration _config;
        private readonly IFileRepositoryApi _fileRepositoryApi;
        private readonly ILogger<DocumentDeletionServices> _logger;

        public DocumentDeletionServices(
            IDocumentRepository documentRepository,
            ICardRepository cardRepository,
            IStepToolExecutionRepository stepToolExecutionRepository,
            IStepToolOutputRepository stepToolOutputRepository,
            IUnitOfWork unitOfWork,
            IEmbeddingsApi embbedingsApi,
            IConfiguration config,
            IFileRepositoryApi fileRepositoryApi,
            ILogger<DocumentDeletionServices> logger)
        {
            _documentRepository = documentRepository;
            _cardRepository = cardRepository;
            _stepToolExecutionRepository = stepToolExecutionRepository;
            _stepToolOutputRepository = stepToolOutputRepository;
            _unitOfWork = unitOfWork;
            _embbedingsApi = embbedingsApi;
            _config = config;
            _fileRepositoryApi = fileRepositoryApi;
            _logger = logger;
        }

        /// <summary>
        /// Delete documents by ids
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="headersDto"></param>
        /// <returns></returns>
        public async Task<bool> Delete(List<int> ids, HeadersDto headersDto)
        {
            ArgumentNullException.ThrowIfNull(ids);

            var referenceFilesToRemove = _documentRepository.FindHashById(ids).ToList();
            var hashList = referenceFilesToRemove;

            _unitOfWork.BeginTransaction();
            try
            {
                _documentRepository.ClearWorkflowRelationships(ids);

                var cardIds = await _cardRepository.FindCardIdsByDocumentIdsAsync(ids);
                if (cardIds.Any())
                {
                    _stepToolExecutionRepository.DeleteByCardIds(cardIds);
                    _stepToolOutputRepository.DeleteByCardIds(cardIds);
                }

                await _cardRepository.DeleteByDocumentIds(ids);
                var deleted = _documentRepository.Delete(ids);
                await Task.WhenAll(hashList.Select(hash => DeleteHash(hash, headersDto.Tenant)));

                if (referenceFilesToRemove.Any())
                {
                    await DeleteBlobFilesAsync(referenceFilesToRemove, headersDto.Tenant);
                }

                _unitOfWork.Commit();
                return deleted;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred in the {nameof(DocumentDeletionServices)} in the {nameof(Delete)} method");
                _unitOfWork.Rollback();
                throw new AppException(ErrorCode.DefaultError, ex.Message, null);
            }
        }

        /// <summary>
        /// Delete hash from Embeddings API
        /// </summary>
        /// <param name="hash"></param>
        /// <param name="tenant"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task DeleteHash(string hash,
            string tenant)
        {
            if (string.IsNullOrEmpty(hash))
                throw new ArgumentException("Hash cannot be null or empty.", nameof(hash));
            if (string.IsNullOrEmpty(tenant))
                throw new ArgumentException("Tenant cannot be null or empty.", nameof(tenant));

            var apikey = _config["IndexerApiKey"]!;
            var resultRequest = await _embbedingsApi.DeleteHash(tenant,
                hash,
                tenant,
                apikey);

            if (!resultRequest.IsSuccessStatusCode && resultRequest.StatusCode != HttpStatusCode.NotFound)
            {
                throw new ArgumentException("Error while sending delete hash in Embeddings API");
            }
        }

        /// <summary>
        /// Deletes blob files from Azure Storage
        /// </summary>
        /// <param name="referenceFiles"></param>
        /// <param name="tenant"></param>
        /// <returns></returns>
        private async Task DeleteBlobFilesAsync(List<string> referenceFiles, string tenant)
        {
            foreach (var referenceFile in referenceFiles)
            {
                if (!string.IsNullOrEmpty(referenceFile))
                {
                    string blobPath = $"{tenant}/{referenceFile}";
                    await _fileRepositoryApi.Delete(blobPath);
                }
            }
        }

    }
}