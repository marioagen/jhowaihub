using Newtonsoft.Json.Linq;

namespace WoopiAiHub.Domain.DTOs.IntegrationHub;

public record class IntegrationHubDocumentEmbeddingsQueryResponse
{
    public string ReferenceFile { get; set; } = string.Empty;

    public string KeyMongoAccess { get; set; } = string.Empty;

    public string Tenant { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public JObject Data { get; set; } = new();

    public List<IntegrationHubQuestionAnswerDto> QuestionsAnswers { get; set; } = [];
}
