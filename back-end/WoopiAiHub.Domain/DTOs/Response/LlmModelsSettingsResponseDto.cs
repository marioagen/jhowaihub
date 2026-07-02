namespace WoopiAiHub.Domain.DTOs.Response
{
    public class LlmModelsSettingsResponseDto
    {
        public Dictionary<string, string> Models { get; set; } = new();
        public List<LlmModelOptionDto> AvailableModels { get; set; } = [];
        public bool CanEdit { get; set; }
    }
}
