using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WoopiAiHub.Domain.Interfaces.Refit.Functions;

namespace WoopiAiHub.Application.Utils;

public class DocumentFileRetriever
{
    private readonly IFunctionFileRetriever _functionFileRetriever;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DocumentFileRetriever> _logger;

    public DocumentFileRetriever(
        IFunctionFileRetriever functionFileRetriever,
        IConfiguration configuration,
        ILogger<DocumentFileRetriever> logger)
    {
        _functionFileRetriever = functionFileRetriever;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<byte[]> DownloadAsync(string referenceFile, string tenant)
    {
        var functionApiKeyAuth = _configuration["RefitExternalSettings:FunctionApiKey"];
        if (string.IsNullOrEmpty(functionApiKeyAuth))
        {
            _logger.LogError("Function API key is missing in the configuration.");
            throw new ArgumentNullException(nameof(functionApiKeyAuth),
                "Function API key is missing in the configuration.");
        }

        var response = await _functionFileRetriever.Get(referenceFile, functionApiKeyAuth, tenant);
        return await response.Content.ReadAsByteArrayAsync();
    }
}
