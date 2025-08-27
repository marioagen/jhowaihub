using Microsoft.AspNetCore.SignalR;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Hubs;

namespace WoopiAiHub.Api.Hubs
{
    public class DocumentNotifier : IDocumentNotifier
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IConnectionMappingService _connectionMapping;

        public DocumentNotifier(IHubContext<NotificationHub> hubContext,
                                IConnectionMappingService connectionMapping)
        {
            _hubContext = hubContext;
            _connectionMapping = connectionMapping;
        }

        /// <summary>
        /// Notifies all connected clients about the status change of a document.
        /// </summary>
        /// <param name="userEmail"></param>
        /// <param name="documentId"></param>
        /// <param name="newStatus"></param>
        /// <returns></returns>
        public async Task NotifyStatusChangedAsync(string userEmail, int documentId, DocumentStatus newStatus)
        {
            var connections = _connectionMapping.GetConnections(userEmail);
            foreach (var connectionId in connections)
            {
                await _hubContext.Clients.Client(connectionId).SendAsync("StatusChanged", new
                {
                    DocumentId = documentId,
                    Status = newStatus
                });
            }
        }
    }
}
