using WoopiAiHub.Domain.Interfaces.Handlers;

namespace WoopiAiHub.Application.ToolsHandler;
public class ToolFactoryHandler : IToolFactoryHandler
{
    private readonly Dictionary<string, IToolHandler> _handlers;

    public ToolFactoryHandler(IEnumerable<IToolHandler> handlers)
    {
        _handlers = handlers.ToDictionary(h => h.Type, h => h);
    }

    /// <summary>
    /// Retrieves the tool handler associated with the specified type.
    /// </summary>
    /// <param name="type">The type of the tool handler to retrieve. This value cannot be <see langword="null"/> or empty.</param>
    /// <returns>The <see cref="IToolHandler"/> instance associated with the specified type.</returns>
    /// <exception cref="ArgumentException">Thrown if a handler for the specified <paramref name="type"/> is not found.</exception>
    public IToolHandler GetHandler(string type)
    {
        if (!_handlers.TryGetValue(type, out var handler))
            throw new ArgumentException($"Handler for type '{type}' not found.");

        return handler;
    }
}
