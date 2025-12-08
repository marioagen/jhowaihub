using Newtonsoft.Json;

namespace WoopiAiHub.Domain.DTOs.Refit
{
    public record class ExcessManagementTenantDto
    {
        [JsonProperty("tenant")]
        public string Tenant { get; set; } = string.Empty;

        [JsonProperty("usageCount")]
        public int UsageCount { get; set; }
    }
}
