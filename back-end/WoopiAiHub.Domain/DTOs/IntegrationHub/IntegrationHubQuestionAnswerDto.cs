namespace WoopiAiHub.Domain.DTOs.IntegrationHub;

public sealed class IntegrationHubQuestionAnswerDto
{
    public int Id { get; set; }

    public string Question { get; set; } = string.Empty;

    public string Answer { get; set; } = string.Empty;

    public List<IntegrationHubQueryUsageDto> Usage { get; set; } = [];
}
