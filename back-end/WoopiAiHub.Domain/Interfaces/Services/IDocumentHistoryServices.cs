using Microsoft.AspNetCore.Mvc;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.DTOs;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IDocumentHistoryServices
    {
        bool Create(DocumentHistory documentHistory);
        JsonResult FindById(int idDocument,
                            string emailCreator);
        bool Delete(int idDocument,
                    string emailCreator);
        bool UpdateHistory(UpdateHistoryDto updateHistoryDto,
                           string emailCreator);
    };
}
