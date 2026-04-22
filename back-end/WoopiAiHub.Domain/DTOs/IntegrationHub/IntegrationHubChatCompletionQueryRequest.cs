using Newtonsoft.Json.Linq;

namespace WoopiAiHub.Domain.DTOs.IntegrationHub;

public record class IntegrationHubChatCompletionQueryRequest
{
    public string ReferenceFile { get; set; } = string.Empty;

    public string Tenant { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public string ApiVersion { get; set; } = string.Empty;

    public string ApplicationId { get; set; } = string.Empty;

    public string ApplicationKey { get; set; } = string.Empty;

    public string ResponseQueue { get; set; } = string.Empty;

    public JObject Data { get; set; } = new();

    public IntegrationHubChatCompletionBodyDto ChatCompletion { get; set; } = new();
}
