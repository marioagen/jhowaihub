using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Interfaces.Handlers;
namespace WoopiAiHub.Domain.Interfaces.Handlers
{
    public interface IToolFactoryHandler
    {
        IToolHandler GetHandler(ToolType toolType);
    }
}