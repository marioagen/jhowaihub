namespace WoopiAiHub.Domain.DTOs.Request
{
    public class UpdateLlmModelsSettingsDto
    {
        public Dictionary<string, string> Models { get; set; } = new();
    }
}
