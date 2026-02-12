using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Response;

namespace WoopiAiHub.Application.Services
{
    public class DocumentHistoryServices : IDocumentHistoryServices
    {
        private readonly IDocumentHistoryRepository _documentHistoryRepository;
        private readonly IValidator<DocumentHistory> _documentHistoryValidator;

        public DocumentHistoryServices(IDocumentHistoryRepository documentHistoryRepository,
                                       IValidator<DocumentHistory> documentHistoryValidator)
        {
            _documentHistoryRepository = documentHistoryRepository;
            _documentHistoryValidator = documentHistoryValidator;
        }
        

        /// <summary>
        /// Validates if the DocumentHistory object is valid if so requests the repository layer to save the history
        /// </summary>
        /// <param name="DocumentHistory"></param>
        /// <returns></returns>
        public bool Create(DocumentHistory documentHistory)
        {
            _documentHistoryValidator.ValidateAndThrow(documentHistory);

            return _documentHistoryRepository.Create(documentHistory);
        }

        /// <summary>
        /// Find the list of DocumentHistory by the id of the Document 
        /// </summary>
        /// <param name="idDocument"></param>
        /// <returns></returns>
        public JsonResult FindById(int idDocument,
                                   string emailCreator)
        {
            return new JsonResult(_documentHistoryRepository.FindById(idDocument));
        }

        /// <summary>
        /// Find the first N DocumentHistory entries by the id of the Document (cumulative load: 10, then 20, then 30...).
        /// </summary>
        /// <param name="idDocument"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        public IEnumerable<DocumentHistoryDto> FindByIdWithTake(int idDocument, int take)
        {
            var entries = _documentHistoryRepository.FindByIdWithTake(idDocument, take);
            return entries.Select(h => new DocumentHistoryDto
            {
                Id = h.Id,
                IdDocument = h.IdDocument,
                Input = h.Input,
                Output = h.Output,
                IsEdited = h.IsEdited,
                Created = h.Created
            });
        }

        /// <summary>
        /// Verify the email and proceed to update the history output of an Document
        /// </summary>
        /// <param name="updateHistoryDto"></param>
        /// <returns></returns>
        public bool UpdateHistory(UpdateHistoryDto updateHistoryDto,
                                  string emailCreator)
        {
            var result = _documentHistoryRepository.UpdateHistory(updateHistoryDto);

            return result;
        }

        /// <summary>
        /// Delete a DocumentHistory by the id of the Document 
        /// </summary>
        /// <param name="idDocument"></param>
        /// <returns></returns>
        public bool Delete(int idDocument,
                           string emailCreator)
        {
            var result = _documentHistoryRepository.Delete(idDocument);

            return result;
        }
    }
}