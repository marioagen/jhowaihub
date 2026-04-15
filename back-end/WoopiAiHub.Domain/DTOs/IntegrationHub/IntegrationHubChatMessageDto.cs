namespace WoopiAiHub.Domain.DTOs.IntegrationHub;

public sealed class IntegrationHubChatMessageDto
{
    public string Role { get; set; } = "system";

    public string Content { get; set; } = string.Empty;
}
