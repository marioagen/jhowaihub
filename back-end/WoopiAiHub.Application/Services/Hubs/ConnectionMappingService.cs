using System.Collections.Concurrent;
using WoopiAiHub.Domain.Interfaces.Hubs;

namespace WoopiAiHub.Application.Services.Hubs
{
    public class ConnectionMappingService : IConnectionMappingService
    {
        private readonly ConcurrentDictionary<string, HashSet<string>> _connections = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Adds a new connection ID to the list of connections for the specified email.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="connectionId"></param>
        public void AddConnection(string email, string connectionId)
        {
            _connections.AddOrUpdate(email,
                _ => new HashSet<string> { connectionId },
                (_, existing) =>
                {
                    lock (existing)
                    {
                        existing.Add(connectionId);
                    }
                    return existing;
                });
        }

        /// <summary>
        /// Removes the connection ID from the list of connections for the specified email.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="connectionId"></param>
        public void RemoveConnection(string email, string connectionId)
        {
            if (_connections.TryGetValue(email, out var connections))
            {
                lock (connections)
                {
                    connections.Remove(connectionId);
                    if (connections.Count == 0)
                    {
                        _connections.TryRemove(email, out _);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the list of connection IDs for the specified email.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public IReadOnlyCollection<string> GetConnections(string email)
        {
            if (!_connections.TryGetValue(email, out var connections))
                return Array.Empty<string>();

            lock (connections)
            {
                return connections.ToList();
            }
        }
    }
}
