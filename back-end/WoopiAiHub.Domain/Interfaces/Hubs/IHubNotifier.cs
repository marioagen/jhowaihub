using WoopiAiHub.Domain.Enum;

namespace WoopiAiHub.Domain.Interfaces.Hubs
{
    public interface IHubNotifier
    {
        Task DocumentStatusChangedAsync(string userEmail, int documentId, DocumentStatus newStatus);
        Task CardProgessAsync(string userEmail, int cardId, double percentage, int stepId, string toolName, bool failed = false, string? labelError = null);
        Task AnonymizationReadyAsync(string userEmail, int documentId, string documentUrl);
        Task WorkflowKanbanRefreshAsync(string userEmail, int workflowId);
        Task ToolUpdatedInWorkflowAsync(string userEmail, int workflowId, string workflowName, int toolId, string toolName);
    }
}
