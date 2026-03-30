using WoopiAiHub.Domain.Enum;

namespace WoopiAiHub.Domain.Interfaces.Hubs
{
    public interface IHubNotifier
    {
        Task DocumentStatusChangedAsync(string userEmail, int documentId, DocumentStatus newStatus);
        Task CardProgessAsync(string userEmail, int cardId, double percentage, int stepId, string toolName, bool failed = false);
        Task AnonymizationReadyAsync(string userEmail, int documentId, string documentUrl);
    }
}
