namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IPlaygroundServices
    {
        Task<string> TestPromptWithContextAsync(string promptText, string contextText, string tenantId, string email);
    }
}
