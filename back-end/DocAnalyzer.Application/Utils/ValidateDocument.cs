using DocAnalyzer.Domain.Interfaces.Repository;
using DocAnalyzer.Domain.Interfaces.Services;

namespace DocAnalyzer.Application.Utils
{
    public class ValidateDocument : IValidateDocument
    {
        private readonly IDocumentRepository _documentRepository;

        public ValidateDocument(IDocumentRepository documentRepository)
        {
            _documentRepository = documentRepository;
        }

        /// <summary>
        /// Check that the inquery creator's email is the same as the request
        /// </summary>
        /// <param name="idDocument"></param>
        /// <param name="emailCreator"></param>
        /// <exception cref="AccessViolationException"></exception>
        public void VerifyCreatorEmail(int idDocument,
                                       string emailCreator)
        {
            var document = _documentRepository.FindById(idDocument);

            if (document.EmailCreator.Equals(emailCreator) is false)
                throw new AccessViolationException("You are not the creator of this inquery, unable to perform this action");
        }
    }
}
