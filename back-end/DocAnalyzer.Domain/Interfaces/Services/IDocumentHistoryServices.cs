using Microsoft.AspNetCore.Mvc;
using DocAnalyzer.Domain.Models;
using DocAnalyzer.Domain.DTOs;

namespace DocAnalyzer.Domain.Interfaces.Services
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
