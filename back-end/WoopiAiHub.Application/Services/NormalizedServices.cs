using FluentValidation;
using Microsoft.Extensions.Logging;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Application.Services
{
    public class DocumentNormalizedServices : IDocumentNormalizedServices
    {
        private readonly IDocumentNormalizedRepository _documentNormalizedRepository;
        private readonly IValidator<DocumentNormalized> _documentNormalizedValidator;
        private readonly ILogger<DocumentNormalizedServices> _logger;

        public DocumentNormalizedServices(IDocumentNormalizedRepository documentNormalizedRepository,
                                          IValidator<DocumentNormalized> documentNormalizedValidator,
                                          ILogger<DocumentNormalizedServices> logger)
        {
            _documentNormalizedRepository = documentNormalizedRepository;
            _documentNormalizedValidator = documentNormalizedValidator;
            _logger = logger;
        }

        /// <summary>
        /// Validates if the DocumentNormalized object is valid if so requests the repository layer to save the text
        /// </summary>
        /// <param name="DocumentNormalized"></param>
        /// <returns></returns>
        public bool Create(DocumentNormalized documentNormalized)
        {
            _documentNormalizedValidator.ValidateAndThrow(documentNormalized);

            return _documentNormalizedRepository.Create(documentNormalized);
        }

        /// <summary>
        /// Update normalized document
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Update(DocumentNormalized documentNormalized)
        {

            return _documentNormalizedRepository.Update(documentNormalized);

        }

        /// <summary>
        /// Find the list of DocumentNormalized by the id of the document 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public DocumentNormalized FindById(int id)
        {
            try
            {
                return _documentNormalizedRepository.FindById(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred in the {nameof(DocumentNormalizedServices)} in the {nameof(FindById)} method");
                throw new AppException(ErrorCode.DefaultError, ex.Message, null);
            }
        }

        /// <summary>
        /// Count all normalized documents
        /// </summary>
        /// <returns></returns>
        public int FindDocumentNormalizedCount()
        {
            return _documentNormalizedRepository.FindDocumentNormalizedCount();
        }

        /// <summary>
        /// Insert or update a normalized document
        /// </summary>
        /// <param name="documentId"></param>
        /// <param name="normalizedContext"></param>
        public void InsertOrUpdate(int documentId, string normalizedContext)
        {
            var normalizedDocument = _documentNormalizedRepository.FindById(documentId);
            if (normalizedDocument is not null)
            {
                var documentNormalized = CreateDocumentNormalized(documentId, normalizedContext, normalizedDocument.Id);
                _documentNormalizedRepository.Update(documentNormalized);
            }
            else
            {
                var documentNormalized = CreateDocumentNormalized(documentId, normalizedContext, 0);
                _documentNormalizedRepository.Create(documentNormalized);
            }
        }

        /// <summary>
        /// Create a new DocumentNormalized for the database.
        /// </summary>
        /// <param name="idDocument"></param>
        /// <param name="content"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        private static DocumentNormalized CreateDocumentNormalized(int documentId,
                                                                   string content,
                                                                   int id)
        {
            return new DocumentNormalized
            (
                documentId,
                content,
                id,
                DateTime.Now
            );
        }
    }
}
