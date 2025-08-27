namespace WoopiAiHub.Domain.Interfaces.Hubs
{
    public interface IConnectionMappingService
    {
        void AddConnection(string email, string connectionId);
        void RemoveConnection(string email, string connectionId);
        IReadOnlyCollection<string> GetConnections(string email);
    }
}
