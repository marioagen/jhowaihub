namespace WoopiAiHub.Domain.DTOs.IntegrationHub;

public record class IntegrationHubQuestionDto
{
    public int Id { get; set; }

    public string Question { get; set; } = string.Empty;
}
