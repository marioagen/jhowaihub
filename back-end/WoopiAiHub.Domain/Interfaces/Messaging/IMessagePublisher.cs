namespace WoopiAiHub.Domain.Interfaces.Messaging
{
    public interface IMessagePublisher<in T>
    {
        Task PublishAsync(string destination, T message);
    }
}
