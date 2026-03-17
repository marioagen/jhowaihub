using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.DTOs.Response.Auditor;

namespace WoopiAiHub.Domain.Interfaces.Repository.Audit
{
    /// <summary>
    /// Repository for auditor-related queries. Used to list and retrieve documents, workflows, and users for auditing.
    /// </summary>
    public interface IAuditorRepository
    {
        Task<ICollection<CardAuditorSummaryDto>> FindCardsAuditSummaryAsync(int take, string? search, bool? isFinalized = null);
        Task<CardAuditorDetailDto?> FindCardAuditDetailsAsync(int documentId, int workflowId, int take, string? search = null, Guid? userId = null, int? actionType = null, int? stepId = null, bool orderDescending = true);
        Task<ICollection<WorkflowAuditorSummaryDto>> FindWorkflowAuditSummaryAsync(int take = 10, string? search = null);
        Task<WorkflowAuditorDetailsDto?> FindWorkflowAuditDetailsAsync(int workflowId, string? search = null, int? stepId = null, int? actionType = null, bool orderDescending = true);
        Task<ICollection<UserAuditorSummaryDto>> FindUserAuditSummaryAsync(int take = 10, string? userName = null, int? teamId = null);
        Task<UserAuditorDetailsDto?> FindUserAuditDetailsAsync(Guid userId, string? search = null, int? actionTypeCode = null, bool orderDescending = true);
    }
}
