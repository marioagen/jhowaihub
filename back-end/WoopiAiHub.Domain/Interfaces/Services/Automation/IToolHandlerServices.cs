using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Request.Automation;

namespace WoopiAiHub.Domain.Interfaces.Services.Automation
{
    public interface IToolHandlerServices
    {
        ExecutionMessageDto BuildPayload(string input, int stepToolId, int cardId);
    }
}
