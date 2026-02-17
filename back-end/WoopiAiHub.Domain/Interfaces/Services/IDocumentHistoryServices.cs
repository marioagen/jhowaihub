using Microsoft.AspNetCore.Mvc;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Response;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IDocumentHistoryServices
    {
        bool Create(DocumentHistory documentHistory);
        JsonResult FindById(int idDocument,
                            string emailCreator);
        IEnumerable<DocumentHistoryDto> FindByIdWithTake(int idDocument, int take, string? search = null, string? order = null, string? orderBy = null, Guid? userId = null);
        bool Delete(int idDocument,
                    string emailCreator);
        bool UpdateHistory(UpdateHistoryDto updateHistoryDto,
                           string emailCreator);
    };
}
