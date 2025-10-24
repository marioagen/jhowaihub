using WoopiAiHub.Domain.Models;
namespace WoopiAiHub.Domain.Interfaces.Handlers
{
    public interface IToolFactoryHandler
    {
        IToolHandler GetHandler(string type);
    }
}