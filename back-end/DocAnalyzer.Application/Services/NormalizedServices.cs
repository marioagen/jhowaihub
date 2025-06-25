using FluentValidation;
using DocAnalyzer.Domain.Interfaces.Repository;
using DocAnalyzer.Domain.Interfaces.Services;
using DocAnalyzer.Domain.Models;
using DocAnalyzer.Domain.DTOs;
using DocAnalyzer.Repository;

namespace DocAnalyzer.Application.Services
{
    public class DocumentNormalizedServices : IDocumentNormalizedServices
    {
        private readonly IDocumentNormalizedRepository _documentNormalizedRepository;
        private readonly IValidator<DocumentNormalized> _documentNormalizedValidator;
        private readonly IValidateDocument _validateDocument;

        public DocumentNormalizedServices(IDocumentNormalizedRepository documentNormalizedRepository,
                                      	  IValidator<DocumentNormalized> documentNormalizedValidator,
                                          IValidateDocument validateDocument)
        {
            _documentNormalizedRepository = documentNormalizedRepository;
            _documentNormalizedValidator = documentNormalizedValidator;
            _validateDocument = validateDocument;
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
        public DocumentNormalized FindById(int id,
                                           string emailCreator)
        {
            _validateDocument.VerifyCreatorEmail(id,
                                                 emailCreator);

            return _documentNormalizedRepository.FindById(id);
        }

        /// <summary>
        /// Count all normalized documents
        /// </summary>
        /// <returns></returns>
        public int FindDocumentNormalizedCount()
        {
            return _documentNormalizedRepository.FindDocumentNormalizedCount();
        }
    }
}
