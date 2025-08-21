namespace WoopiAiHub.Domain.Interfaces.Messenging
{
    public interface IMessagePublisher<T>
    {
        Task PublishAsync(string destination, T message);
    }
}
