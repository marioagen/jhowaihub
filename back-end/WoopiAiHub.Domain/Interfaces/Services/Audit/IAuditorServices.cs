using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.DTOs.Response.Auditor;

namespace WoopiAiHub.Domain.Interfaces.Services.Audit
{
    /// <summary>
    /// Service for auditor endpoints. Provides access to documents, workflows, and users for auditing.
    /// </summary>
    public interface IAuditorServices
    {
        Task<ICollection<AuditorCardsDto>> FindCardsAuditAsync(int take, string? search, int? statusId);
        Task<ICollection<AuditorCardResponseDto>> FindAuditByCardIdAsync(int cardId, int workflowId, int take, Guid? userId, int? actionType, int? stepId, bool orderDescending = true);
        Task<ICollection<WorkflowDto>> GetWorkflowsAsync();
        Task<WorkflowDto?> GetWorkflowByIdAsync(int id);
        Task<ICollection<UserDto>> GetUsersAsync();
        Task<UserDto?> GetUserByIdAsync(Guid id);
    }
}
