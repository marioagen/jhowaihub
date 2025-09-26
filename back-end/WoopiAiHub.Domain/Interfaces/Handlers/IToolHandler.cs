using WoopiAiHub.Domain.DTOs.Request.Automation;
namespace WoopiAiHub.Domain.Interfaces.Handlers
{
    public interface IToolHandler
    {
        Task<ExecutionMessageDto> BuildPayload(string tenant, string referenceFile, string input, int stepToolId, int cardId);
    }
}