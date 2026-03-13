using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.DTOs.Response.Auditor;

namespace WoopiAiHub.Domain.Interfaces.Repository.Audit
{
    /// <summary>
    /// Repository for auditor-related queries. Used to list and retrieve documents, workflows, and users for auditing.
    /// </summary>
    public interface IAuditorRepository
    {
        Task<ICollection<CardAuditorSummaryDto>> FindCardsAuditSummaryAsync(int take, string? search, int? statusId);
        Task<ICollection<CardAuditorDetailDto>> FindCardAuditDetailsAsync(int cardId, int workflowId, int take, Guid? userId, int? actionType, int? stepId, bool orderDescending = true);
        Task<ICollection<WorkflowAuditorSummaryDto>> FindWorkflowAuditSummaryAsync();
        Task<WorkflowAuditorDetailsDto?> FindWorkflowAuditDetailsAsync(int workflowId);
        Task<ICollection<UserAuditorSummaryDto>> FindUserAuditSummaryAsync(int skip = 0, string? userName = null, int? teamId = null);
        Task<UserAuditorDetailsDto?> FindUserAuditDetailsAsync(Guid userId, int? actionTypeCode = null, bool orderDescending = true);
    }
}
