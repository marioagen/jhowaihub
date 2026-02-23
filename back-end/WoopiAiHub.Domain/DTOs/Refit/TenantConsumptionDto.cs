using Newtonsoft.Json;

namespace WoopiAiHub.Domain.DTOs.Refit
{
    public record class TenantConsumptionDto
    {
        [JsonProperty("tenant")]
        public string Tenant { get; set; } = string.Empty;
        [JsonProperty("usageCount")]
        public int UsageCount { get; set; }
        [JsonProperty("periodStart")]
        public DateTime PeriodStart { get; set; }
        [JsonProperty("periodEnd")]
        public DateTime PeriodEnd { get; set; }
    }
}
