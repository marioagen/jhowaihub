using WoopiAiHub.Domain.Enum.Audit;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IAuditCardService
    {
        Task CreateAndSaveAsync(int cardId, int workflowId, int documentId, AuditCardActionType actionType, string? automationUserEmail = null, CancellationToken cancellationToken = default);
        Task CreateBatchAndSaveAsync(IReadOnlyList<(int cardId, int workflowId, int documentId)> cardWorkflows, AuditCardActionType actionType, string? automationUserEmail = null, CancellationToken cancellationToken = default);
    }
}
