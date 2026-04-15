namespace WoopiAiHub.Domain.DTOs.IntegrationHub;

public sealed class IntegrationHubQueryUsageDto
{
    public string Model { get; set; } = string.Empty;

    public int? Total_usage { get; set; }
}
