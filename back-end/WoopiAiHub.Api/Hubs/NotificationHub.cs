using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using WoopiAiHub.Domain.Interfaces.Hubs;

namespace WoopiAiHub.Api.Hubs
{
    public class NotificationHub : Hub
    {
        private readonly IConnectionMappingService _connectionMapping;

        public NotificationHub(IConnectionMappingService connectionMapping)
        {
            _connectionMapping = connectionMapping;
        }

        /// <summary>
        /// Adds the connection to the group based on the user's email.
        /// </summary>
        /// <returns></returns>
        public override Task OnConnectedAsync()
        {
            var email = Context.User?.FindFirst(ClaimTypes.Email)?.Value;
            if (!string.IsNullOrEmpty(email))
            {
                var connectionId = Context.ConnectionId;
                _connectionMapping.AddConnection(email, connectionId);
            }

            return base.OnConnectedAsync();
        }

        /// <summary>
        /// Removes the connection from the group based on the user's email when disconnected.
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public override Task OnDisconnectedAsync(Exception? exception)
        {
            var email = Context.User?.FindFirst(ClaimTypes.Email)?.Value;
            if (!string.IsNullOrEmpty(email))
            {
                _connectionMapping.RemoveConnection(email, Context.ConnectionId);
            }

            return base.OnDisconnectedAsync(exception);
        }
    }
}