using WoopiAiHub.Domain.DTOs.Response.Auditor.Rows;

namespace WoopiAiHub.Domain.Interfaces.Repository.Audit
{
    /// <summary>
    /// Repository for auditor-related queries. Used to list and retrieve documents, workflows, and users for auditing.
    /// Returns raw/projection data; service layer performs validation and DTO assembly.
    /// </summary>
    public interface IAuditorRepository
    {
        Task<List<int>> FindDocumentIdsForDocumentsSummaryAsync(int take, string? search, bool? isFinalized = null);
        Task<List<DocumentAuditorSummaryRowDto>> FindAuditRowsForDocumentsSummaryAsync(IReadOnlyList<int> documentIds, string? search, bool? isFinalized = null);

        Task<List<DocumentAuditorDetailRowDto>> FindAuditRowsForDocumentDetailAsync(int documentId, int workflowId, int take, string? search, Guid? userId, int? actionType, int? stepId, bool orderDescending);

        Task<List<int>> FindWorkflowIdsForWorkflowSummaryAsync(int take, string? search);
        Task<List<WorkflowAuditorSummaryRowDto>> FindAuditRowsForWorkflowSummaryAsync(IReadOnlyList<int> workflowIds);

        Task<List<WorkflowAuditorDetailsRowDto>> FindAuditRowsForWorkflowDetailsAsync(int workflowId, string? search, int? stepId, int? actionType, bool orderDescending);

        Task<List<Guid>> FindUserIdsForUserSummaryAsync(int take, string? userName, int? teamId);
        Task<List<UserAuditorSummaryRowDto>> FindAuditRowsForUserSummaryAsync(IReadOnlyList<Guid> userIds);

        Task<List<UserAuditorDetailsRowDto>> FindAuditRowsForUserDetailsAsync(Guid userId, string? search, int? actionTypeCode, bool orderDescending);
    }
}
