using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IDocumentAnalysisRejectionServices
    {
        Task<bool> CreateRejectionAsync(CreateDocumentAnalysisRejectionDto dto, string emailCreator);
        Task<bool> CreateRejectionRangeAsync(CreateDocumentAnalysisRejectionRangeDto dto, string emailCreator);
        Task<List<DocumentAnalysisRejectionDto>> FindRejectionsByCardIdAsync(int cardId);
        Task<List<StepDto>> FindWorkflowPreviousStepsAsync(int workflowId, int cardId);
    }
}
