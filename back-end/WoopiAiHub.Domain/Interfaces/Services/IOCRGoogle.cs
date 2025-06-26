namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IOcrGoogle
    {
        Task<ICollection<string>> ProcessResult(Byte[] bytesFile);
    }
}
