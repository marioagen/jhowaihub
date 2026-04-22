using Newtonsoft.Json.Linq;

namespace WoopiAiHub.Domain.DTOs.IntegrationHub;

public record class IntegrationHubDocumentEmbeddingsQueryRequest
{
    public string? RagProvider { get; set; }

    public string ApplicationId { get; set; } = string.Empty;

    public string ApplicationKey { get; set; } = string.Empty;

    public string ApiVersion { get; set; } = string.Empty;

    public string? EmbeddingModelName { get; set; }

    public string ReferenceFile { get; set; } = string.Empty;

    public string KeyMongoAccess { get; set; } = string.Empty;

    public List<IntegrationHubQuestionDto> Questions { get; set; } = [];

    public int? kValue { get; set; }

    public string? Model { get; set; }

    public string? Template { get; set; }

    public int? Temperature { get; set; }

    public string? Refine_template { get; set; }

    public int? Max_tokens { get; set; }

    public string? SearchMode { get; set; }

    public string? Tenant { get; set; }

    public string? Email { get; set; }

    public JObject Data { get; set; } = new();
}
