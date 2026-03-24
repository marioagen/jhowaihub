using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.DTOs.Response.Auditor;
using WoopiAiHub.Domain.DTOs.Response.Auditor.Documents;

namespace WoopiAiHub.Domain.Interfaces.Services.Audit
{
    public interface IAuditorServices
    {
        Task<AuditorLoadMoreResultDto<DocumentAuditorSummaryDto>> FindDocumentsAuditSummaryAsync(int take, int skip, string? search, bool? isFinalized = null, bool? isRemoved = null);
        Task<DocumentAuditorDetailDto?> FindDocumentAuditDetailsAsync(int documentId, int workflowId, int take, string? search = null, Guid? userId = null, int? actionType = null, int? stepId = null, bool orderDescending = true);
        Task<AuditorLoadMoreResultDto<WorkflowAuditorSummaryDto>> FindWorkflowAuditSummaryAsync(int take = 10, int skip = 0, string? search = null);
        Task<WorkflowAuditorDetailsDto?> FindWorkflowAuditDetailsAsync(int workflowId, int take, string? search = null, int? stepId = null, int? actionType = null, bool orderDescending = true);
        Task<AuditorLoadMoreResultDto<UserAuditorSummaryDto>> FindUserAuditSummaryAsync(int take = 10, int skip = 0, string? userName = null, int? teamId = null);
        Task<UserAuditorDetailsDto?> FindUserAuditDetailsAsync(Guid userId, int take, string? search = null, int? actionTypeCode = null, bool orderDescending = true);
        Task<ICollection<AuditorActionTypeDto>> FindActionTypesAsync();
    }
}
