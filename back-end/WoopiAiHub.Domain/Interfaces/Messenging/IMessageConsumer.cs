namespace WoopiAiHub.Domain.Interfaces.Messenging
{
    public interface IMessageConsumer<T>
    {
        Task ConsumerAsync(string destination, Func<T, Task> process);
    }
}
