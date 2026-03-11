using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.DTOs.Response.Auditor;

namespace WoopiAiHub.Domain.Interfaces.Services.Audit
{
    /// <summary>
    /// Service for auditor endpoints. Provides access to documents, workflows, and users for auditing.
    /// </summary>
    public interface IAuditorServices
    {
        Task<ICollection<CardAuditorSummaryDto>> FindCardsAuditSummaryAsync(int take, string? search, int? statusId);
        Task<ICollection<CardAuditorDetailDto>> FindCardAuditDetailsAsync(int cardId, int workflowId, int take, Guid? userId, int? actionType, int? stepId, bool orderDescending = true);
        Task<ICollection<AuditorWorkflowListItemDto>> FindWorkflowAuditSummaryAsync();
        Task<AuditorWorkflowResponseDto?> FindWorkflowAuditDetailsAsync(int workflowId);
        Task<ICollection<UserDto>> FindUserAuditSummaryAsync();
        Task<UserDto?> FindUserAuditDetailsAsync(Guid id);
    }
}
