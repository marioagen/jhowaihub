using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.DTOs.WorkflowTemplate;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IWorkflowTemplateServices
    {
        Task<List<WorkflowTemplateListItemDto>> FindTemplatesAsync(string? query, string? orderBy);
        Task<WorkflowTemplatePackageDto?> FindTemplateByIdAsync(Guid id);
        Task<WorkflowTemplatePackageDto> ExportAsync(int workflowId);
        Task<List<WorkflowTemplateImportResultDto>> ImportByIdsAsync(WorkflowTemplateImportRequestDto request, string email);
    }
}
