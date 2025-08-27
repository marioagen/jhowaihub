namespace WoopiAiHub.Domain.Interfaces.Messaging
{
    public interface IMessageManager
    {
        Task CreateQueuesAsync();
        Task<T> CreateChannel<T>();
    }
}
