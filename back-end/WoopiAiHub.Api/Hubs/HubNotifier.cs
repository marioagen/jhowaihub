using Microsoft.AspNetCore.SignalR;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Hubs;

namespace WoopiAiHub.Api.Hubs
{
    public class HubNotifier : IHubNotifier
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IConnectionMappingService _connectionMapping;

        public HubNotifier(IHubContext<NotificationHub> hubContext,
                           IConnectionMappingService connectionMapping)
        {
            _hubContext = hubContext;
            _connectionMapping = connectionMapping;
        }

        /// <summary>
        /// Notifies all active connections for the specified user about the progress of a card execution.
        /// </summary>
        /// <remarks>This method retrieves all active connections associated with the specified user and
        /// sends a notification to each connection. The notification includes the card ID and the updated progress
        /// percentage.</remarks>
        /// <param name="userEmail">The email address of the user whose connections will be notified. Cannot be <see langword="null"/> or empty.</param>
        /// <param name="cardId">The unique identifier of the card whose progress has changed.</param>
        /// <param name="percentage">The progress percentage of the card execution. Must be a value between 0.0 and 100.0.</param>
        /// <returns></returns>
        public async Task CardProgessAsync(string userEmail, 
                                           int cardId, 
                                           double percentage,
                                           int stepId,
                                           string toolName,
                                           bool failed = false)
        {
            var connections = _connectionMapping.GetConnections(userEmail);
            foreach (var connectionId in connections)
            {
                await _hubContext.Clients.Client(connectionId).SendAsync("CardExecutionChanged", new
                {
                    CardId = cardId,
                    Percentage = (int)percentage,
                    StepId = stepId,
                    ToolName = toolName,
                    Failed = failed
                });
            }
        }

        /// <summary>
        /// Notifies all connected clients about the status change of a document.
        /// </summary>
        /// <param name="userEmail"></param>
        /// <param name="documentId"></param>
        /// <param name="newStatus"></param>
        /// <returns></returns>
        public async Task DocumentStatusChangedAsync(string userEmail, int documentId, DocumentStatus newStatus)
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

        /// <summary>
        /// Notifies all active connections for the specified user that a document is ready for anonymization.
        /// </summary>
        /// <remarks>This method sends a real-time notification to all active SignalR connections
        /// associated with the specified user. Each connection receives the document ID and URL to indicate that the
        /// document is ready for further processing.</remarks>
        /// <param name="userEmail">The email address of the user whose connections will receive the notification. Cannot be null or empty.</param>
        /// <param name="documentId">The unique identifier of the document that is ready for anonymization.</param>
        /// <param name="documentUrl">The URL where the document can be accessed for anonymization. Cannot be null or empty.</param>
        /// <returns>A task that represents the asynchronous notification operation.</returns>
        public async Task AnonymizationReadyAsync(string userEmail, int documentId, string documentUrl)
        {
            var connections = _connectionMapping.GetConnections(userEmail);
            foreach (var connectionId in connections)
            {
                await _hubContext.Clients.Client(connectionId).SendAsync("AnonymizationReady", new
                {
                    DocumentId = documentId,
                    Url = documentUrl
                });
            }
        }
    }
}
