namespace WoopiAiHub.Domain.Interfaces.Messaging
{
    public interface IMessagePublisher<T>
    {
        Task PublishAsync(string destination, T message);
    }
}
