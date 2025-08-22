using WoopiAiHub.Domain.Enum;

namespace WoopiAiHub.Domain.Interfaces.Hubs
{
    public interface IDocumentNotifier
    {
        Task NotifyStatusChangedAsync(string userEmail, int documentId, DocumentStatus newStatus);
    }
}