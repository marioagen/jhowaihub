namespace WoopiAiHub.Domain.DTOs.IntegrationHub;

public record class IntegrationHubChatMessageDto
{
    public string Role { get; set; } = "system";

    public string Content { get; set; } = string.Empty;
}
