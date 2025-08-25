namespace WoopiAiHub.Domain.Interfaces.Messaging
{
    public interface IMessageConsumer<T>
    {
        Task ConsumerAsync(string destination, Func<T, Task> process);
    }
}
