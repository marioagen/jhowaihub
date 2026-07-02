namespace WoopiAiHub.Domain.Models
{
    public class TenantLlmModelSetting
    {
        public string Scope { get; set; } = string.Empty;
        public string ModelName { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
        public string UpdatedByEmail { get; set; } = string.Empty;
    }
}
