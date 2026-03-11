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
        Task<ICollection<AuditorWorkflowListItemDto>> FindWorkflowAuditSummaryAsync();
        Task<AuditorWorkflowResponseDto?> FindWorkflowAuditDetailsAsync(int workflowId);
        Task<ICollection<UserDto>> FindUserAuditSummaryAsync();
        Task<UserDto?> FindUserAuditDetailsAsync(Guid id);
    }
}
