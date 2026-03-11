using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.DTOs.Response.Auditor;

namespace WoopiAiHub.Domain.Interfaces.Repository.Audit
{
    /// <summary>
    /// Repository for auditor-related queries. Used to list and retrieve documents, workflows, and users for auditing.
    /// </summary>
    public interface IAuditorRepository
    {
        Task<ICollection<AuditorCardsDto>> FindCardsAuditAsync(int take, string? search, int? statusId);
        Task<ICollection<AuditorCardResponseDto>> FindAuditByCardIdAsync(int cardId, int workflowId, int take, Guid? userId, int? actionType, int? stepId, bool orderDescending = true);
        Task<ICollection<AuditorWorkflowResponseDto>> FindWorkflowAuditAsync();
        Task<WorkflowDto?> GetWorkflowByIdAsync(int id);
        Task<ICollection<UserDto>> GetUsersAsync();
        Task<UserDto?> GetUserByIdAsync(Guid id);
    }
}
