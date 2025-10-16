using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using WoopiAiHub.Domain.Interfaces.Handlers;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Application.ToolsHandler;

public class ToolFactoryHandler : IToolFactoryHandler
{
    private readonly Dictionary<string, IToolHandler> _handlers;

    public ToolFactoryHandler(IEnumerable<IToolHandler> handlers)
    {
        _handlers = handlers.ToDictionary(h => h.Type, h => h);
    }

    /// <summary>
    /// Return tool handler
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public IToolHandler GetHandler(ToolType type)
    {
        string typeName = type.Name;
        if (!_handlers.TryGetValue(typeName, out var handler))
            throw new ArgumentException($"Handler for type '{typeName}' not found.");

        return handler;
    }
}