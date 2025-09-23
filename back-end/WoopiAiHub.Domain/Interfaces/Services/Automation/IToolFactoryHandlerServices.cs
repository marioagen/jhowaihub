using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Services.Automation
{
    public interface IToolFactoryHandlerServices
    {
        IToolHandlerServices GetHandler(ToolType toolType);
    }
}
