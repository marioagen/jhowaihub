using WoopiAiHub.Domain.Enum;

namespace WoopiAiHub.Domain.DTOs.IntegrationHub;

public record class IntegrationHubDocumentEmbeddingsDeleteRequest
{
    public RagProvider? RagProvider { get; set; }

    public string ReferenceFile { get; set; } = string.Empty;

    public string KeyMongoAccess { get; set; } = string.Empty;

    public string Tenant { get; set; } = string.Empty;
}
