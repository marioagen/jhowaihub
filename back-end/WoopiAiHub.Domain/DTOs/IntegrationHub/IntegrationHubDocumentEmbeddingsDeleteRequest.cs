namespace WoopiAiHub.Domain.DTOs.IntegrationHub;

public record class IntegrationHubDocumentEmbeddingsDeleteRequest
{
    public string? RagProvider { get; set; }

    public string ReferenceFile { get; set; } = string.Empty;

    public string KeyMongoAccess { get; set; } = string.Empty;

    public string Tenant { get; set; } = string.Empty;
}
