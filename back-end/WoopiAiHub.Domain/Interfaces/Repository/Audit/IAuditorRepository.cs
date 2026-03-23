using WoopiAiHub.Domain.DTOs.Response.Auditor.Documents;
using WoopiAiHub.Domain.DTOs.Response.Auditor.Users;
using WoopiAiHub.Domain.DTOs.Response.Auditor.Workflows;

namespace WoopiAiHub.Domain.Interfaces.Repository.Audit
{
    public interface IAuditorRepository
    {
        Task<List<int>> FindDocumentIdsForDocumentsSummaryAsync(int take, int skip, string? search, bool? isFinalized = null, bool? isRemoved = null);
        Task<List<DocumentAuditorSummaryRowDto>> FindAuditRowsForDocumentsSummaryAsync(IReadOnlyList<int> documentIds, string? search, bool? isFinalized = null, bool? isRemoved = null);

        Task<List<DocumentAuditorDetailRowDto>> FindAuditRowsForDocumentDetailAsync(int documentId, int workflowId, int take, string? search, Guid? userId, int? actionType, int? stepId, bool orderDescending);

        Task<List<int>> FindWorkflowIdsForWorkflowSummaryAsync(int take, int skip, string? search);
        Task<List<WorkflowAuditorSummaryRowDto>> FindAuditRowsForWorkflowSummaryAsync(IReadOnlyList<int> workflowIds);

        Task<List<WorkflowAuditorDetailsRowDto>> FindAuditRowsForWorkflowDetailsAsync(int workflowId, string? search, int? stepId, int? actionType, bool orderDescending);

        Task<List<Guid>> FindUserIdsForUserSummaryAsync(int take, int skip, string? userName, int? teamId);
        Task<List<UserAuditorSummaryRowDto>> FindAuditRowsForUserSummaryAsync(IReadOnlyList<Guid> userIds);

        Task<List<UserAuditorDetailsRowDto>> FindAuditRowsForUserDetailsAsync(Guid userId, int take, string? search, int? actionTypeCode, bool orderDescending);
    }
}
